using UnityEngine;

public class BottleTarget : MonoBehaviour
{
    [Header("Configuración")]
    public int points = 15;
    public float respawnTime = 3f;
    public GameObject breakEffect;

    [Header("Panel de la caseta")]
    public MinigamePanel minigamePanel; 

    private Vector3 originalPos;
    private Quaternion originalRot;
    private Renderer rend;

    void Start()
    {
        originalPos = transform.position;
        originalRot = transform.rotation;
        rend = GetComponent<Renderer>();
    }

    public void GetShot(float charge = 1f)
    {
        if (!gameObject.activeSelf) return;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 knockback = (transform.position - Camera.main.transform.position).normalized;
            rb.AddForce(knockback * charge * 300f);
            rb.AddTorque(Random.insideUnitSphere * charge * 100f);
        }

        if (minigamePanel != null)
            minigamePanel.AddPoints(points);
        else
            GameManager.Instance.AddScore(points);

        if (breakEffect)
            Instantiate(breakEffect, transform.position, Quaternion.identity);

        Invoke(nameof(Respawn), respawnTime);
    }

    void Respawn()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        transform.position = originalPos;
        transform.rotation = originalRot;
        gameObject.SetActive(true);
    }
}

