using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Timer, puntuación, estado del juego...
    public static GameManager Instance;

    [Header("Configuración")]
    public float gameDuration = 60f;

    [Header("UI")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI scoreText;
    public GameObject endGamePanel;
    public TextMeshProUGUI finalScoreText;

    [Header("Sonidos")]
    public AudioClip backgroundSound;
    public AudioClip startGameSound;
    public AudioClip endGameSound;

    private float timeRemaining;
    private int score;
    private bool gameActive = false;

    private AudioSource audioSource;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        endGamePanel.SetActive(false);
        UpdateTimerUI();
        UpdateScoreUI();
        Debug.Log("¡Bienvenido a la Feria VR! Presiona el botón de inicio para comenzar.");
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = backgroundSound;
        audioSource.loop = true;
        if (audioSource != null)
        {
            Debug.Log("audiosource");
            audioSource.Play();
        }
        // StartGame(); // Puedes llamar a esto desde un botón UI si prefieres no iniciar automáticamente
    }

    void Update()
    {
        if (!gameActive) return;

        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (timeRemaining <= 0)
            EndGame();
    }

    public void StartGame()
    {
        score = 0;
        timeRemaining = gameDuration;
        gameActive = true;
        endGamePanel.SetActive(false);
        UpdateScoreUI();
        audioSource?.PlayOneShot(startGameSound);
        Debug.Log("¡Juego iniciado!");
    }

    public void AddScore(int points)
    {
        if (!gameActive) return;
        score += points;
        UpdateScoreUI();
    }

    public void EndGame(bool byVoice = false)
    {
        gameActive = false;
        endGamePanel.SetActive(true);
        finalScoreText.text = "Puntuación: " + score;

        if (byVoice && endGameSound != null)
            audioSource?.PlayOneShot(endGameSound,1f);

        Debug.Log("Juego terminado. Puntuación: " + score);
    }

    void UpdateTimerUI()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        timerText.text = seconds.ToString();
        // Parpadeo rojo en los últimos 10 segundos
        timerText.color = seconds <= 10 ? Color.red : Color.white;
    }

    void UpdateScoreUI()
    {
        scoreText.text = "Puntos: " + score;
    }

    public bool IsGameActive() => gameActive;
}