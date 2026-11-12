using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gestiona el estado global de la partida AR.
/// </summary>
public class GameManagerAR : MonoBehaviour
{
    public static GameManagerAR Instance { get; private set; }

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject menuPanel;
    public Button playButton;
    public TextMeshProUGUI playButtonLabel;
    public TextMeshProUGUI finalScoreText;

    [Header("Configuración de partida")]
    public float gameDuration = 20f;
    public bool startOnGrab = true;

    [Header("Spawning de aliens")]
    public GameObject alienPrefab;
    public int maxAliensAlive = 3;
    public float spawnIntervalMin = 1.5f;
    public float spawnIntervalMax = 3f;

    [Tooltip("Offset base desde la pared. Se suma alienScale/2.")]
    public float wallOffset = 0.05f;
    public float alienScale = 0.15f;

    [Header("Filtro de spawn")]
    [Tooltip("Altura mínima del alien respecto al suelo (m)")]
    public float minSpawnHeight = 0.5f;

    [Tooltip("Máxima distancia por encima de la cámara (m). Evita spawns en el techo.")]
    public float maxHeightAboveCamera = 0.5f;

    [Header("Debug")]
    public bool spawnDebugAlien = true;

    [Header("Sonidos")]
    public AudioClip startGameSound;
    public AudioClip endGameSound;

    // Estado interno
    private int score = 0;
    private float timeLeft = 0f;
    private bool gameActive = false;
    private int aliensAlive = 0;
    private Coroutine spawnCoroutine;
    private bool meshReady = false;
    private AudioSource audioSource;

    private List<Vector3> wallPoints = new List<Vector3>();
    private List<Vector3> wallNormals = new List<Vector3>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (playButton != null)
            playButton.onClick.AddListener(StartGame);

        if (gameDuration <= 0f)
        {
            Debug.LogWarning("[GameManagerAR] gameDuration era 0, forzando a 20.");
            gameDuration = 20f;
        }

        ShowMenu(isFirstTime: true);
    }

    void Update()
    {
        if (!gameActive) return;

        timeLeft -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(timeLeft, 0f)).ToString();

        if (timeLeft <= 0f)
            EndGame();
    }

    // ══════════════════════════════════════════
    //  API pública
    // ══════════════════════════════════════════

    public bool IsGameActive() => gameActive;

    public void AddScore(int points)
    {
        if (!gameActive) return;
        score += points;
        UpdateScoreUI();
    }

    public void OnAlienDestroyed()
    {
        aliensAlive = Mathf.Max(0, aliensAlive - 1);
    }

    public void OnMeshReady(List<Vector3> points, List<Vector3> normals)
    {
        wallPoints = points;
        wallNormals = normals;
        meshReady = true;
        Debug.Log($"[GameManagerAR] Malla lista. {wallPoints.Count} puntos de pared.");
    }

    public void OnGunGrabbed()
    {
        if (startOnGrab && !gameActive)
            StartGame();
    }

    // ══════════════════════════════════════════
    //  Flujo de partida
    // ══════════════════════════════════════════

    public void StartGame()
    {
        if (!meshReady)
        {
            Debug.LogWarning("[GameManagerAR] La malla aún no está lista.");
            return;
        }

        if (gameDuration <= 0f) gameDuration = 20f;

        score = 0;
        timeLeft = gameDuration;
        aliensAlive = 0;
        gameActive = true;

        UpdateScoreUI();
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(gameDuration).ToString();

        if (menuPanel != null)
            menuPanel.SetActive(false);

        foreach (var a in FindObjectsByType<Alien>(FindObjectsSortMode.None))
            Destroy(a.gameObject);

        if (startGameSound != null)
            audioSource.PlayOneShot(startGameSound);

        if (spawnDebugAlien && alienPrefab != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 debugPos = cam.transform.position + cam.transform.forward * 2f;
                debugPos.y = cam.transform.position.y;
                Quaternion debugRot = Quaternion.LookRotation(cam.transform.position - debugPos);
                GameObject debugAlien = Instantiate(alienPrefab, debugPos, debugRot);
                debugAlien.transform.localScale = Vector3.one * alienScale;
                debugAlien.name = "DEBUG_ALIEN";

                Alien alienComp = debugAlien.GetComponent<Alien>();
                if (alienComp != null)
                    alienComp.wallNormal = -cam.transform.forward;

                aliensAlive++;
            }
        }

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void EndGame()
    {
        gameActive = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        foreach (var a in FindObjectsByType<Alien>(FindObjectsSortMode.None))
            Destroy(a.gameObject);

        if (endGameSound != null)
            audioSource.PlayOneShot(endGameSound);

        ShowMenu(isFirstTime: false);
        Debug.Log($"[GameManagerAR] Partida terminada. Puntuación: {score}");
    }

    // ══════════════════════════════════════════
    //  UI
    // ══════════════════════════════════════════

    void ShowMenu(bool isFirstTime)
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);

        if (playButtonLabel != null)
            playButtonLabel.text = isFirstTime ? "Jugar" : "Volver a jugar";

        if (finalScoreText != null)
        {
            finalScoreText.gameObject.SetActive(!isFirstTime);
            if (!isFirstTime)
                finalScoreText.text = $"{score}";
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"{score}";
    }

    // ══════════════════════════════════════════
    //  Spawning de aliens
    // ══════════════════════════════════════════

    IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(0.5f);

        while (gameActive)
        {
            if (aliensAlive < maxAliensAlive)
                TrySpawnAlien();

            float interval = Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(interval);
        }
    }

    void TrySpawnAlien()
    {
        if (alienPrefab == null)
        {
            Debug.LogError("[GameManagerAR] alienPrefab es NULL.");
            return;
        }

        if (wallPoints.Count == 0)
        {
            Debug.LogWarning("[GameManagerAR] No hay puntos de spawn.");
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        float cameraY = cam.transform.position.y;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            int index = Random.Range(0, wallPoints.Count);
            Vector3 wallPoint = wallPoints[index];
            Vector3 normal = wallNormals[index];

            // FILTRO 1: descartar si la normal tiene mucha componente vertical 
            // Una pared real tiene normal casi horizontal (Y ≈ 0).
            // Si |Y| > 0.25 es techo, suelo o superficie inclinada.
            if (Mathf.Abs(normal.y) > 0.25f)
            {
                Debug.Log($"[Spawn] Descartado intento {attempt}: normal.y={normal.y:F2} (techo/suelo)");
                continue;
            }

            // FILTRO 2: altura relativa a la cámara 
            // Demasiado alto = techo. Demasiado bajo = suelo.
            float heightRelCamera = wallPoint.y - cameraY;
            if (heightRelCamera > maxHeightAboveCamera)
            {
                Debug.Log($"[Spawn] Descartado intento {attempt}: +{heightRelCamera:F2}m sobre cámara (techo)");
                continue;
            }
            if (wallPoint.y < minSpawnHeight)
            {
                continue;
            }

            // Re-validar normal contra la cámara AHORA 
            Vector3 toCamera = cam.transform.position - wallPoint;
            if (Vector3.Dot(normal, toCamera) < 0f)
                normal = -normal;

            // Offset total 
            float totalOffset = wallOffset + (alienScale * 0.5f);
            Vector3 spawnPos = wallPoint + normal * totalOffset;

            // FILTRO 3: raycast de verificación 
            // Lanzar un rayo desde el punto de spawn hacia la pared.
            // Si no toca nada, el punto está en el aire (no hay pared real).
            if (!Physics.Raycast(spawnPos, -normal, totalOffset + 0.1f))
            {
                Debug.Log($"[Spawn] Descartado intento {attempt}: sin pared detrás (raycast falló)");
                continue;
            }

            // FILTRO 4: distancia mínima entre aliens 
            bool tooClose = false;
            foreach (var a in FindObjectsByType<Alien>(FindObjectsSortMode.None))
            {
                if (Vector3.Distance(spawnPos, a.transform.position) < 0.4f)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // Orientar mirando al jugador 
            Vector3 lookDir = cam.transform.position - spawnPos;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude < 0.01f) lookDir = normal;
            Quaternion rotation = Quaternion.LookRotation(lookDir, Vector3.up);

            // Instanciar 
            GameObject alienGO = Instantiate(alienPrefab, spawnPos, rotation);
            alienGO.transform.localScale = Vector3.one * alienScale;

            Alien alienComp = alienGO.GetComponent<Alien>();
            if (alienComp != null)
                alienComp.wallNormal = normal.normalized;

            Rigidbody alienRb = alienGO.GetComponent<Rigidbody>();
            if (alienRb != null)
            {
                alienRb.isKinematic = true;
                alienRb.useGravity = false;
            }

            aliensAlive++;

            Debug.DrawRay(wallPoint, normal * 0.3f, Color.green, 5f);
            Debug.DrawRay(spawnPos, Vector3.up * 0.1f, Color.yellow, 5f);
            Debug.Log($"[GameManagerAR] Alien OK en {spawnPos}, normal {normal}, offset {totalOffset:F3}. Vivos: {aliensAlive}");

            return;
        }

        Debug.LogWarning("[GameManagerAR] Sin punto válido tras 20 intentos.");
    }
}