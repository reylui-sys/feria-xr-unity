using UnityEngine;

public class HockeyDisc : MonoBehaviour
{
    [Header("Referencias")]
    public Transform stick;              // Arrastrad el palo aquí en el Inspector

    [Header("Configuración")]
    public float triggerDistance = 0.3f; // Distancia a la que el palo "golpea"
    public float launchForce = 8f;       // Fuerza del lanzamiento
    public float cooldown = 0.5f;        // Segundos entre golpes para evitar spam

    [Header("Boost")]
    public float boostMultiplier = 2.5f;  // Ajustadlo en Inspector

    private Rigidbody rb;
    private float lastHitTime;
    private Vector3 lastStickPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        lastStickPosition = stick.position;
    }

    void FixedUpdate()
    {
        if (stick == null) return;
        if (Time.time - lastHitTime < cooldown) return;

        float distance = Vector3.Distance(transform.position, stick.position);

        if (distance < triggerDistance)
        {
            // Dirección = movimiento del palo en este frame
            Vector3 stickVelocity = (stick.position - lastStickPosition) / Time.fixedDeltaTime;

            // Solo lanzar si el palo se está moviendo de verdad
            if (stickVelocity.magnitude > 0.5f)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(stickVelocity.normalized * launchForce * boostMultiplier, ForceMode.Impulse);
                lastHitTime = Time.time;
                Debug.Log("¡Golpe! Velocidad palo: " + stickVelocity.magnitude);
            }
        }

        lastStickPosition = stick.position;
    }
}
