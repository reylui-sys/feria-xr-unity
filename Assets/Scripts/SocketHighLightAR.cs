using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Ilumina un objeto visual (aro/disco) en el socket de la pistola.
///
/// Estados:
///   - Pistola en el socket  → glow apagado
///   - Pistola fuera         → pulsación azul suave (invitando a devolver)
///   - Pistola cerca (hover) → verde brillante
/// </summary>
public class SocketHighlightAR : MonoBehaviour
{
    [Header("Referencia visual")]
    [Tooltip("Objeto hijo que brilla (disco, torus, cylinder aplastado)")]
    public GameObject highlightObject;

    [Header("Colores de emisión")]
    public Color idleColor = new Color(0.2f, 0.5f, 1f, 1f);
    public Color hoverColor = new Color(0.2f, 1f, 0.4f, 1f);

    [Header("Intensidades")]
    public float idleIntensityMin = 0.3f;
    public float idleIntensityMax = 1.0f;
    public float hoverIntensity = 2.0f;
    public float pulseSpeed = 2f;

    // Estado
    private Material glowMat;
    private XRSocketInteractor socket;
    private bool gunInSocket = true;
    private bool gunHovering = false;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        // Buscar socket en este mismo GameObject
        socket = GetComponent<XRSocketInteractor>();
        if (socket == null)
        {
            Debug.LogError("[SocketHighlightAR] No hay XR Socket Interactor en este GameObject.");
            return;
        }

        // Conectar eventos
        socket.hoverEntered.AddListener(OnHoverEnter);
        socket.hoverExited.AddListener(OnHoverExit);
        socket.selectEntered.AddListener(OnGunPlaced);
        socket.selectExited.AddListener(OnGunTaken);

        // Material instanciado para no afectar a otros objetos
        if (highlightObject != null)
        {
            Renderer rend = highlightObject.GetComponent<Renderer>();
            if (rend != null)
                glowMat = rend.material; // crea copia automática

            highlightObject.SetActive(false);
        }
    }

    void Update()
    {
        if (highlightObject == null || glowMat == null) return;

        // Pistola en socket → apagar
        if (gunInSocket)
        {
            if (highlightObject.activeSelf)
                highlightObject.SetActive(false);
            return;
        }

        // Pistola fuera → encender
        if (!highlightObject.activeSelf)
            highlightObject.SetActive(true);

        // Color + intensidad
        Color color;
        float intensity;

        if (gunHovering)
        {
            color = hoverColor;
            intensity = hoverIntensity;
        }
        else
        {
            color = idleColor;
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            intensity = Mathf.Lerp(idleIntensityMin, idleIntensityMax, t);
        }

        glowMat.SetColor(EmissionColor, color * intensity);
    }

    // Eventos del XR Socket Interactor 

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        gunHovering = true;
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        gunHovering = false;
    }

    void OnGunPlaced(SelectEnterEventArgs args)
    {
        gunInSocket = true;
        gunHovering = false;
    }

    void OnGunTaken(SelectExitEventArgs args)
    {
        gunInSocket = false;
    }

    void OnDestroy()
    {
        if (glowMat != null)
            Destroy(glowMat);
    }
}