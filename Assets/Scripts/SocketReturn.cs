using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Si la pistola cae lejos o se suelta fuera del socket,
/// la devuelve a su posición original tras unos segundos.
/// </summary>
public class SocketReturn : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Segundos antes de devolver la pistola al socket si cae lejos")]
    public float returnDelay = 3f;

    [Tooltip("Distancia máxima antes de activar retorno automático")]
    public float fallDistanceLimit = 2f;

    private Vector3 socketPos;
    private Quaternion socketRot;
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private bool isHeld = false;
    private bool returningToSocket = false;

    void Start()
    {
        socketPos = transform.position;
        socketRot = transform.rotation;
        rb = GetComponent<Rigidbody>();

        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(_ => OnGrabbed());
            grabInteractable.selectExited.AddListener(_ => OnReleased());

            // Evitar que la pistola salga volando al soltarla
            grabInteractable.throwOnDetach = false;
        }

        // Estado inicial: kinematic en el socket
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void Update()
    {
        if (isHeld || returningToSocket) return;

        float distance = Vector3.Distance(transform.position, socketPos);
        if (distance > fallDistanceLimit && !IsInvoking(nameof(ReturnToSocket)))
        {
            Debug.Log($"[SocketReturn] Pistola a {distance:F1}m del socket. Retorno en {returnDelay}s.");
            Invoke(nameof(ReturnToSocket), returnDelay);
        }
    }

    void OnGrabbed()
    {
        isHeld = true;
        returningToSocket = false;
        CancelInvoke(nameof(ReturnToSocket));

        // No kinematic para que XR mueva la pistola correctamente
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
        }
    }

    void OnReleased()
    {
        isHeld = false;

        // Dejar NO kinematic para que el XR Socket Interactor
        // pueda detectar el collider de la pistola y aceptarla
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Si no cae en el socket, retorno automático
        Invoke(nameof(ReturnToSocket), returnDelay);
    }

    void ReturnToSocket()
    {
        if (isHeld) return;

        returningToSocket = true;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        transform.position = socketPos;
        transform.rotation = socketRot;
        returningToSocket = false;

        Debug.Log("[SocketReturn] Pistola devuelta al socket.");
    }
}