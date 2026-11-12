using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    [Header("Sonido de dificultad")]
    public AudioClip difficultySound;
    private AudioSource audioSource;

    // En cada obstáculo que se mueva delante de la portería
    public static DifficultyManager Instance;

    [Header("Configuración por dificultad")]
    public float[] targetSpeeds = { 0.5f, 1f, 2f };    // Velocidad de dianas
    public int[] timeLimits = { 90, 60, 45 };            // Segundos de partida
    public int[] pointMultipliers = { 1, 1, 2 };         // Multiplicador de puntos

    [HideInInspector] public int currentDifficulty = 1; // 0=Fácil, 1=Normal, 2=Difícil

    void Awake()
    {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SetDifficulty(int level, bool playSound = true)
    {
        currentDifficulty = Mathf.Clamp(level, 0, 2);
        // Actualizar el GameManager con el tiempo correcto
        GameManager.Instance.gameDuration = timeLimits[currentDifficulty];
        if (playSound && difficultySound != null)
            audioSource?.PlayOneShot(difficultySound);
        Debug.Log("Dificultad: " + level);
    }

    public float GetTargetSpeed() => targetSpeeds[currentDifficulty];
    public int GetMultiplier()    => pointMultipliers[currentDifficulty];
}