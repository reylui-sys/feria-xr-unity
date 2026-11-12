using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class DartStick : MonoBehaviour
{
    [Header("Lanzamiento")]
    public float dartSpeed = 8f;
    public float lineLength = 0.5f;

    [Header("Socket y respawn")]
    public float respawnTime = 5f;
    public float fallDistanceLimit = 5f;

    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private LineRenderer lineRenderer;
    private NearFarInteractor lastInteractor;

    private bool isStuck = false;
    private bool isHeld = false;
    private bool isFlying = false;
    private Vector3 launchDirection;
    private Vector3 socketPos;
    private Quaternion socketRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();

        socketPos = transform.position;
        socketRot = transform.rotation;

        // LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.005f;
        lineRenderer.endWidth = 0.005f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = false;

        grabInteractable.selectEntered.AddListener(_ => OnGrabbed());
        grabInteractable.selectExited.AddListener(_ => OnReleased());
    }

    void OnGrabbed()
    {
        isHeld = true;
        isFlying = false;
        CancelInvoke();
        Unstick();
        lineRenderer.enabled = true;

        // Guardar el interactor antes de que se suelte
        lastInteractor = grabInteractable.firstInteractorSelecting as NearFarInteractor;
    }

    void OnReleased()
    {
        isHeld = false;
        lineRenderer.enabled = false;

        if (!isStuck)
        {
            // Dirección del rayo del mando en el momento de soltar
            launchDirection = lastInteractor != null
                ? lastInteractor.transform.forward
                : Camera.main.transform.forward;

            // Orientar el dardo hacia la dirección de lanzamiento
            transform.rotation = Quaternion.LookRotation(launchDirection);

            // Kinematic para moverlo por código, sin físicas
            rb.isKinematic = true;
            isFlying = true;
        }

        Invoke(nameof(CheckRespawn), respawnTime);
    }

    void Update()
    {
        // Línea guía mientras se sostiene
        if (isHeld)
        {
            Vector3 tip = transform.position + transform.forward * 0.1f;
            lineRenderer.SetPosition(0, tip);
            lineRenderer.SetPosition(1, tip + transform.forward * lineLength);
        }

        // Vuelo recto por código
        if (isFlying)
        {
            transform.position += launchDirection * dartSpeed * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(launchDirection);

            // Respawn si cae muy lejos del socket
            float dist = Vector3.Distance(transform.position, socketPos);
            if (dist > fallDistanceLimit && !IsInvoking(nameof(ReturnToSocket)))
                Invoke(nameof(ReturnToSocket), 1f);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isStuck || !isFlying) return;
        if (!other.CompareTag("DartBoard")) return;

        Stick(other.transform);

        // Notificar puntos a la diana
        DartBoard dartBoard = other.GetComponent<DartBoard>();
        if (dartBoard != null)
            dartBoard.OnDartHit(transform.position);
    }

    void Stick(Transform target)
    {
        isFlying = false;
        isStuck = true;
        rb.isKinematic = true;
        CancelInvoke();

        // Parar completamente el movimiento
        transform.SetParent(target);
        // Orientar la punta hacia la diana
        transform.rotation = Quaternion.LookRotation(target.forward * -1);

        Invoke(nameof(ReturnToSocket), respawnTime);
    }

    void Unstick()
    {
        isStuck = false;
        isFlying = false;
        rb.isKinematic = false;
        transform.SetParent(null);
    }

    void CheckRespawn()
    {
        if (!isStuck && !isHeld)
            ReturnToSocket();
    }

    void ReturnToSocket()
    {
        if (isHeld) return;

        CancelInvoke();
        isFlying = false;
        Unstick();
        rb.isKinematic = true;
        transform.position = socketPos;
        transform.rotation = socketRot;
        rb.isKinematic = false;
    }

    public void ForceReturn()
    {
        if (isHeld) return;
        ReturnToSocket();
    }
}