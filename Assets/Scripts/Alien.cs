using UnityEngine;
using System.Collections;

/// <summary>
/// Comportamiento de cada alien individual.
/// </summary>
public class Alien : MonoBehaviour
{
    [Header("Vida")]
    public int health = 1;

    [Header("Puntos")]
    public int basePoints = 10;

    [Header("Tiempo de vida")]
    public float lifetime = 10f;

    [Header("Animación de aparición")]
    public float spawnAnimDuration = 0.4f;

    [Header("Movimiento")]
    public float moveSpeed = 0.3f;
    public float minDistanceToPlayer = 1.0f;
    public float floatAmplitude = 0.05f;
    public float floatFrequency = 2f;
    public bool chasePlayer = false;
    public float changeDirectionInterval = 3f;

    [Range(0f, 1f)]
    public float chaseProbability = 0.5f;

    [Header("Detección de paredes")]
    [Tooltip("Distancia del raycast para detectar paredes delante del alien")]
    public float wallDetectDistance = 0.2f;

    [Tooltip("Si choca con pared, elegir nueva dirección automáticamente")]
    public bool bounceOnWall = true;

    [Header("Efectos")]
    public GameObject deathVFXPrefab;
    public AudioClip hitSound;
    public AudioClip deathSound;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    // Asignado por el spawner
    [HideInInspector] public Vector3 wallNormal = Vector3.forward;

    // Estado interno
    private AudioSource audioSource;
    private bool isDead = false;
    private Vector3 targetScale;
    private bool spawnAnimDone = false;
    private Transform playerCamera;
    private float floatPhase;
    private Vector3 randomMoveDir;
    private float directionTimer;
    private Vector3 spawnPosition;

    void Start()
    {
        targetScale = transform.localScale;
        transform.localScale = Vector3.zero;
        StartCoroutine(SpawnAnimation());

        if (lifetime > 0f)
            Destroy(gameObject, lifetime + spawnAnimDuration);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;

        playerCamera = Camera.main?.transform;
        floatPhase = Random.Range(0f, Mathf.PI * 2f);
        chasePlayer = Random.value < chaseProbability;

        spawnPosition = transform.position;

        PickRandomDirection();
        directionTimer = changeDirectionInterval;
    }

    void Update()
    {
        if (isDead || !spawnAnimDone || playerCamera == null) return;

        // Mirar al jugador 
        Vector3 dirToPlayer = playerCamera.position - transform.position;
        dirToPlayer.y = 0f;
        if (dirToPlayer.sqrMagnitude > 0.01f)
        {
            Quaternion lookRot = Quaternion.LookRotation(dirToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 3f);
        }

        // Calcular movimiento deseado 
        Vector3 movement = Vector3.zero;

        if (chasePlayer)
        {
            float dist = Vector3.Distance(transform.position, playerCamera.position);
            if (dist > minDistanceToPlayer)
            {
                Vector3 moveDir = (playerCamera.position - transform.position).normalized;
                movement = moveDir * moveSpeed * Time.deltaTime;
            }
        }
        else
        {
            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0f)
            {
                PickRandomDirection();
                directionTimer = changeDirectionInterval;
            }
            movement = randomMoveDir * moveSpeed * Time.deltaTime;
        }

        // Lanzar un rayo en la dirección del movimiento horizontal.
        // Si hay pared, no moverse en esa dirección.
        if (movement.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir3D = movement.normalized;
            Vector3 moveDirHorizontal = new Vector3(moveDir3D.x, 0f, moveDir3D.z).normalized;

            if (moveDirHorizontal.sqrMagnitude > 0.01f)
            {
                // Raycast desde el centro del alien en dirección del movimiento
                if (Physics.Raycast(transform.position, moveDirHorizontal, out RaycastHit hit, wallDetectDistance))
                {
                    // Hay pared delante → no moverse en esa dirección
                    Debug.DrawRay(transform.position, moveDirHorizontal * wallDetectDistance, Color.red, 0.5f);

                    if (bounceOnWall && !chasePlayer)
                    {
                        // Reflejar la dirección usando la normal de la pared
                        randomMoveDir = Vector3.Reflect(randomMoveDir, hit.normal);
                        randomMoveDir.y = 0f;
                        randomMoveDir.Normalize();
                        directionTimer = changeDirectionInterval;
                    }

                    // Anular el movimiento este frame
                    movement = Vector3.zero;
                }
                else
                {
                    Debug.DrawRay(transform.position, moveDirHorizontal * wallDetectDistance, Color.green, 0.1f);
                }
            }
        }

        //Protección extra: no retroceder detrás de la pared de spawn 
        if (movement.sqrMagnitude > 0.0001f)
        {
            float towardSpawnWall = Vector3.Dot(movement, -wallNormal);
            if (towardSpawnWall > 0f)
                movement += wallNormal * towardSpawnWall;
        }

        // Aplicar movimiento 
        transform.position += movement;

        // Flotación sinusoidal (solo vertical, no horizontal para evitar atravesar) 
        float floatY = Mathf.Sin(Time.time * floatFrequency + floatPhase) * floatAmplitude;
        transform.position += new Vector3(0f, floatY, 0f) * Time.deltaTime;

        // Seguridad: si retrocedió detrás de su spawn, empujar 
        float distFromSpawn = Vector3.Dot(transform.position - spawnPosition, wallNormal);
        if (distFromSpawn < 0f)
            transform.position += wallNormal * -distFromSpawn;
    }

    void PickRandomDirection()
    {
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0f, Mathf.Sin(angle * Mathf.Deg2Rad));

            // Evitar ir hacia la pared de spawn
            if (Vector3.Dot(dir, -wallNormal) < 0.3f)
            {
                // Verificar con raycast que no haya pared en esa dirección
                if (!Physics.Raycast(transform.position, dir, wallDetectDistance))
                {
                    randomMoveDir = dir;
                    return;
                }
            }
        }
        // Fallback: moverse a lo largo de la pared de spawn
        randomMoveDir = Vector3.Cross(wallNormal, Vector3.up).normalized;
    }

    // ══════════════════════════════════════════
    //  Impactos
    // ══════════════════════════════════════════

    public void TakeHit(int damage = 1, int pointsFromBulletAR = 0)
    {
        if (isDead) return;
        health -= damage;

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound, soundVolume);

        StartCoroutine(HitSquish());

        if (health <= 0)
            Die(pointsFromBulletAR);
    }

    void Die(int pointsFromBulletAR)
    {
        isDead = true;

        if (pointsFromBulletAR == 0 && GameManagerAR.Instance != null)
            GameManagerAR.Instance.AddScore(basePoints);

        if (GameManagerAR.Instance != null)
            GameManagerAR.Instance.OnAlienDestroyed();

        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position, soundVolume);

        if (deathVFXPrefab != null)
            Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);

        StartCoroutine(DeathAnimation());
    }

    // ══════════════════════════════════════════
    //  Colisiones
    // ══════════════════════════════════════════

    void OnCollisionEnter(Collision collision) => HandleBulletARHit(collision.gameObject);
    void OnTriggerEnter(Collider other) => HandleBulletARHit(other.gameObject);

    void HandleBulletARHit(GameObject other)
    {
        if (isDead) return;
        BulletAR bullet = other.GetComponent<BulletAR>();
        if (bullet == null) return;
        TakeHit(1, bullet.pointsOnHit);
        Destroy(other);
    }

    // ══════════════════════════════════════════
    //  Animaciones
    // ══════════════════════════════════════════

    IEnumerator SpawnAnimation()
    {
        float elapsed = 0f;
        while (elapsed < spawnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / spawnAnimDuration;
            transform.localScale = targetScale * EaseOutBack(t);
            yield return null;
        }
        transform.localScale = targetScale;
        spawnAnimDone = true;
    }

    IEnumerator HitSquish()
    {
        float duration = 0.12f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;
            transform.localScale = new Vector3(targetScale.x / s, targetScale.y * s, targetScale.z / s);
            yield return null;
        }
        transform.localScale = targetScale;
    }

    IEnumerator DeathAnimation()
    {
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null;
        }
        Destroy(gameObject);
    }

    float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}