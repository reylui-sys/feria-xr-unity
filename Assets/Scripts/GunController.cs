using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using TMPro;

public class GunController : MonoBehaviour
{
    [Header("Referencias escena")]
    public Transform muzzle;
    public LineRenderer aimLine;
    public GameObject muzzleFlash;

    [Header("Medidor de fuerza")]
    public Slider powerSlider;           // Slider de Unity UI en World Space
    public Image powerFill;              // La barra del slider para cambiar color
    public float chargeSpeed = 1.5f;     // Velocidad a la que sube la barra
    public float maxCharge = 1f;

    [Header("Puntuación por fuerza")]
    // Si disparas con más del 80% de carga, puntos extra
    public int basePoints = 10;
    public int bonusPoints = 20;         // Bonus por disparo cargado al máximo
    public float bonusThreshold = 0.8f;  // 80% de carga para el bonus

    [Header("Munición")]
    public int maxAmmo = 6;
    public TextMeshProUGUI ammoText;

    [Header("Sonidos")]
    public AudioClip shootSound;
    public AudioClip chargeSound;        // Sonido mientras carga
    public AudioClip emptySound;
    public AudioClip reloadSound;

    [Header("Rango")]
    public float maxRange = 20f;

    private float currentCharge = 0f;
    private bool isCharging = false;
    private bool isHeld = false;
    private int currentAmmo;
    private AudioSource audioSource;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;
    public int CurrentAmmo => currentAmmo;

    [Header("Proyectiles")]
    public GameObject bulletPrefab;   // Prefab de la bala
    public float bulletSpeed = 20f;   // Velocidad de la bala


    void Start()
    {
        currentAmmo = maxAmmo;
        audioSource = GetComponent<AudioSource>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Eventos de agarre
        grabInteractable.selectEntered.AddListener(_ => isHeld = true);
        grabInteractable.selectExited.AddListener(_ => {
            isHeld = false;
            CancelCharge();
        });

        // Trigger apretado → empieza a cargar
        grabInteractable.activated.AddListener(_ => StartCharge());
        // Trigger soltado → dispara con la carga acumulada
        grabInteractable.deactivated.AddListener(_ => ReleaseShoot());

        // Inicializar UI
        if (powerSlider) powerSlider.value = 0;
        UpdateAmmoUI();
    }

    void Update()
    {
        // Actualizar línea de mirilla
        UpdateAimLine();

        // Si está cargando, subir la barra
        if (isCharging)
        {
            currentCharge = Mathf.MoveTowards(currentCharge, maxCharge,
                                               chargeSpeed * Time.deltaTime);
            UpdatePowerUI();

            // Vibración háptica que aumenta con la carga (feedback físico)
            SendHapticFeedback(currentCharge * 0.3f);

            // Si llega al máximo, parpadea la barra
            if (currentCharge >= maxCharge && powerFill != null)
                powerFill.color = Color.Lerp(Color.yellow, Color.red,
                                            Mathf.PingPong(Time.time * 3f, 1f));
        }
    }

    void StartCharge()
    {
        Debug.Log("StartCharge llamado");
        if (!GameManager.Instance.IsGameActive()) return;
        if (currentAmmo <= 0)
        {
            audioSource?.PlayOneShot(emptySound);
            return;
        }

        isCharging = true;
        currentCharge = 0f;

        if (chargeSound)
        {
            audioSource.clip = chargeSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void ReleaseShoot()
    {
        Debug.Log("ReleaseShoot llamado");
        if (!isCharging) 
        {
            Debug.Log("No se está cargando");
            return;
        }
        isCharging = false;
        audioSource?.Stop();

        // if (currentCharge < 0.1f)
        // {
        //     // Carga demasiado corta, no dispara
        //     CancelCharge();
        //     return;
        // }

        Shoot(currentCharge);
        currentCharge = 0f;
        UpdatePowerUI();
    }

    void Shoot(float charge)
    {
        Debug.Log($"Shoot llamado | charge: {charge} | bulletPrefab: {bulletPrefab != null}");
    
        currentAmmo--;
        UpdateAmmoUI();

        audioSource?.PlayOneShot(shootSound);
        if (muzzleFlash) StartCoroutine(ShowMuzzleFlash());

        if (bulletPrefab)
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

            Collider bulletCollider = bullet.GetComponent<Collider>();
            Collider[] gunColliders = GetComponentsInChildren<Collider>();
            foreach (Collider c in gunColliders)
                Physics.IgnoreCollision(bulletCollider, c);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = muzzle.forward * bulletSpeed * charge;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.pointsOnHit = charge >= bonusThreshold
                    ? basePoints + Mathf.RoundToInt(bonusPoints * charge)
                    : basePoints;
            }

            Destroy(bullet, 4f);
        }
    }

    void CancelCharge()
    {
        isCharging = false;
        currentCharge = 0f;
        audioSource?.Stop();
        UpdatePowerUI();
    }

    public void Reload()
    {
        if (currentAmmo > 0)
        {
            Debug.Log("No puedes recargar, todavía quedan balas.");
            audioSource?.PlayOneShot(emptySound); // feedback al jugador
            return;
        }

        currentAmmo = maxAmmo;
        audioSource?.PlayOneShot(reloadSound);
        UpdateAmmoUI();
        Debug.Log("¡Recargado!");
    }

    void UpdateAimLine()
    {
        if (aimLine == null || !isHeld) return;

        aimLine.SetPosition(0, muzzle.position);
        RaycastHit hit;
        Vector3 endPoint = Physics.Raycast(muzzle.position, muzzle.forward, out hit, maxRange)
            ? hit.point
            : muzzle.position + muzzle.forward * maxRange;

        aimLine.SetPosition(1, endPoint);

        // La línea cambia de color según la carga
        Color lineColor = Color.Lerp(Color.white, Color.red, currentCharge);
        aimLine.startColor = lineColor;
        aimLine.endColor = lineColor;
    }

    void UpdatePowerUI()
    {
        if (powerSlider == null) return;
        powerSlider.value = currentCharge / maxCharge;

        // Verde → amarillo → rojo según la carga
        if (powerFill != null && !isCharging)
            powerFill.color = Color.green;
        else if (powerFill != null)
            powerFill.color = Color.Lerp(Color.green, Color.yellow, currentCharge);
    }

    void UpdateAmmoUI()
    {
        if (ammoText == null) return;
        // Muestra balas como círculos: ●●●○○○
        string ammoDisplay = "";
        for (int i = 0; i < maxAmmo; i++)
            ammoDisplay += i < currentAmmo ? "●" : "○";
        ammoText.text = ammoDisplay;
    }

    void SendHapticFeedback(float amplitude)
    {
        // Vibración en el mando que sostiene la pistola
        var interactor = grabInteractable.firstInteractorSelecting
            as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor;
        interactor?.SendHapticImpulse(amplitude, 0.1f);
    }

    System.Collections.IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(0.05f);
        muzzleFlash.SetActive(false);
    }
}