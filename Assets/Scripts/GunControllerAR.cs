using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controlador de la pistola en modo AR.
/// Balas infinitas: no hay límite de munición.
/// </summary>
public class GunControllerAR : MonoBehaviour
{
    [Header("Referencias de escena")]
    public Transform muzzle;
    public LineRenderer aimLine;
    public GameObject muzzleFlash;

    [Header("Medidor de fuerza")]
    public Slider powerSlider;
    public Image powerFill;
    public float chargeSpeed = 1.5f;
    public float maxCharge = 1f;

    [Header("Puntuación por fuerza")]
    public int basePoints = 10;
    public int bonusPoints = 20;
    public float bonusThreshold = 0.8f;

    [Header("Sonidos")]
    public AudioClip shootSound;
    public AudioClip chargeSound;

    [Header("Rango")]
    public float maxRange = 20f;

    [Header("Proyectiles")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    // Estado interno
    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isHeld = false;
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        grabInteractable = GetComponent<XRGrabInteractable>();

        grabInteractable.selectEntered.AddListener(_ =>
        {
            isHeld = true;
            if (GameManagerAR.Instance != null)
                GameManagerAR.Instance.OnGunGrabbed();
        });

        grabInteractable.selectExited.AddListener(_ =>
        {
            isHeld = false;
            CancelCharge();
        });

        grabInteractable.activated.AddListener(_ => StartCharge());
        grabInteractable.deactivated.AddListener(_ => ReleaseShoot());

        if (powerSlider) powerSlider.value = 0;
    }

    void Update()
    {
        UpdateAimLine();

        if (isCharging)
        {
            currentCharge = Mathf.MoveTowards(currentCharge, maxCharge,
                                               chargeSpeed * Time.deltaTime);
            UpdatePowerUI();
            SendHapticFeedback(currentCharge * 0.3f);

            if (currentCharge >= maxCharge && powerFill != null)
                powerFill.color = Color.Lerp(Color.yellow, Color.red,
                                             Mathf.PingPong(Time.time * 3f, 1f));
        }
    }

    void StartCharge()
    {
        // Solo disparar si hay partida activa
        if (GameManagerAR.Instance == null || !GameManagerAR.Instance.IsGameActive())
            return;

        // BALAS INFINITAS: no comprobamos munición

        isCharging = true;
        currentCharge = 0f;

        if (chargeSound != null)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void ReleaseShoot()
    {
        if (!isCharging) return;

        isCharging = false;
        audioSource.Stop();
        Shoot(currentCharge);
        currentCharge = 0f;
        UpdatePowerUI();
    }

    void Shoot(float charge)
    {
        // BALAS INFINITAS: no decrementamos munición

        if (shootSound != null) audioSource.PlayOneShot(shootSound);
        if (muzzleFlash != null) StartCoroutine(ShowMuzzleFlash());

        if (bulletPrefab == null)
        {
            Debug.LogError("[GunControllerAR] bulletPrefab es NULL. Asígnalo en el Inspector.");
            return;
        }

        GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

        // Ignorar colisiones entre bala y pistola
        Collider bulletCol = bullet.GetComponent<Collider>();
        if (bulletCol != null)
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
                Physics.IgnoreCollision(bulletCol, c);
        }

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
            rb.linearVelocity = muzzle.forward * bulletSpeed * Mathf.Max(charge, 0.3f);

        BulletAR bulletScript = bullet.GetComponent<BulletAR>();
        if (bulletScript != null)
        {
            bulletScript.pointsOnHit = charge >= bonusThreshold
                ? basePoints + Mathf.RoundToInt(bonusPoints * charge)
                : basePoints;
        }

        // Autodestruir bala tras 4 segundos para no saturar
        Destroy(bullet, 4f);

        Debug.Log($"[GunControllerAR] Disparo con carga {charge:F2}");
    }

    public void Reload()
    {
        // Ya no es necesario con balas infinitas, pero lo dejamos por compatibilidad
    }

    void CancelCharge()
    {
        isCharging = false;
        currentCharge = 0f;
        audioSource.Stop();
        UpdatePowerUI();
    }

    void UpdateAimLine()
    {
        if (aimLine == null || !isHeld) return;

        aimLine.SetPosition(0, muzzle.position);
        Vector3 endPoint = Physics.Raycast(muzzle.position, muzzle.forward, out RaycastHit hit, maxRange)
            ? hit.point
            : muzzle.position + muzzle.forward * maxRange;
        aimLine.SetPosition(1, endPoint);

        Color lineColor = Color.Lerp(Color.white, Color.red, currentCharge);
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
    }

    void UpdatePowerUI()
    {
        if (powerSlider == null) return;
        powerSlider.value = currentCharge / maxCharge;

        if (powerFill != null && !isCharging)
            powerFill.color = Color.green;
        else if (powerFill != null)
            powerFill.color = Color.Lerp(Color.green, Color.yellow, currentCharge);
    }

    void SendHapticFeedback(float amplitude)
    {
        var interactor = grabInteractable.firstInteractorSelecting
            as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        interactor?.SendHapticImpulse(amplitude, 0.1f);
    }

    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}