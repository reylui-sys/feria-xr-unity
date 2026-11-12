using UnityEngine;

public class CanTarget : MonoBehaviour
{
    [Header("Configuración")]
    public float respawnTime = 4f;

    [Header("Panel de la caseta")]
    public MinigamePanel minigamePanel;

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Rigidbody rb;

    void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public void GetShot(int points)
    {
       
        if (minigamePanel != null)
            minigamePanel.AddPoints(points);
        else
            GameManager.Instance.AddScore(points);

        rb.isKinematic = false;
        rb.AddForce(Random.insideUnitSphere * 3f + Vector3.up * 2f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        rb.isKinematic = true;
        gameObject.SetActive(false);
        transform.position = originalPos;
        transform.rotation = originalRot;
        gameObject.SetActive(true);
    }
}