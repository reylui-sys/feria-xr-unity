using UnityEngine;

public class DartBoard : MonoBehaviour
{
    [Header("Zonas de puntuación")]
    public Transform center;
    public int bullseyePoints = 50;
    public int innerPoints = 25;
    public int outerPoints = 10;
    public float bullseyeRadius = 0.05f;
    public float innerRadius = 0.15f;

    [Header("Panel de la caseta")]
    public MinigamePanel minigamePanel;

    private ObstacleOscillator osc;

    void Start()
    {
        ApplyDifficulty();
    }

    // Llamado desde VoiceManager y DifficultyUI cuando cambia la dificultad
    public void ApplyDifficulty()
    {
        if (DifficultyManager.Instance == null)
        {
            Debug.LogWarning("[DartBoard] DifficultyManager no encontrado.");
            return;
        }

        int level = DifficultyManager.Instance.currentDifficulty;
        Debug.Log($"[DartBoard] Aplicando dificultad {level}");

        float[] scales = { 1.5f, 1f, 0.6f };
        transform.localScale = Vector3.one * scales[level];
    }

    public void OnDartHit(Vector3 hitPosition)
    {
        if (!GameManager.Instance.IsGameActive()) return;

        float dist = Vector3.Distance(hitPosition, center.position);

        int points;
        if (dist <= bullseyeRadius) points = bullseyePoints;
        else if (dist <= innerRadius) points = innerPoints;
        else points = outerPoints;

        if (minigamePanel != null)
            minigamePanel.AddPoints(points);
        else
            GameManager.Instance.AddScore(points);

        Debug.Log($"¡Dardo! Distancia: {dist:F3}m → +{points} puntos");
    }
}