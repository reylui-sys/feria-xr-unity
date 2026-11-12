using UnityEngine;

public class ObstacleOscillator : MonoBehaviour
{
    [Header("Movimiento")]
    public float amplitude = 2f;      // Distancia máxima a cada lado
    public float speed = 1f;          // Velocidad base

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        // Offset aleatorio para que no todos vayan sincronizados
        float randomOffset = Random.Range(0f, Mathf.PI * 2);
        _phase = randomOffset;
    }

    private float _phase;

    void Update()
    {
        float difficulty = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.GetTargetSpeed()
            : 1f;

        float x = Mathf.Sin(Time.time * speed * difficulty + _phase) * amplitude;
        transform.position = startPosition + transform.right * x;
    }
}