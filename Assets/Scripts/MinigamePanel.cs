using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MinigamePanel : MonoBehaviour
{

    [Header("Identificación")]
    public string gameName = "Pistola";
    public string gameType = "Disparo";

    [Header("¿Tiene munición esta caseta?")]
    public bool showAmmo = true;

    [Header("UI — Textos")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI gameTypeText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI localScoreText;
    public TextMeshProUGUI recordText;

    [Header("UI — Voice box")]
    public TextMeshProUGUI voiceText;

    [Header("UI — Dificultad")]
    public Button easyButton;
    public Button normalButton;
    public Button hardButton;
    public Color selectedColor = Color.yellow;
    public Color defaultColor = Color.white;

    [Header("Configuración")]
    public float defaultDuration = 60f;

    [Header("Sonido fin de tiempo")]
    public AudioClip timeUpSound;

    private AudioSource audioSource;

    private float timeRemaining;
    private int localScore;
    private int record;
    private bool isActive = false;

    public bool IsActive => isActive;
    public int LocalScore => localScore;


    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (titleText != null) titleText.text = gameName;
        if (gameTypeText != null) gameTypeText.text = gameType;

        UpdateVoiceText();

        if (easyButton != null) easyButton.onClick.AddListener(() => SelectDifficulty(0));
        if (normalButton != null) normalButton.onClick.AddListener(() => SelectDifficulty(1));
        if (hardButton != null) hardButton.onClick.AddListener(() => SelectDifficulty(2));

        SelectDifficulty(1, playSound: false);

        // Mostrar el tiempo inicial sin que corra

        RefreshDuration();
        RefreshUI();
    }

    void Update()
    {
        if (!isActive) return;
        timeRemaining -= Time.deltaTime;
        UpdateTimerUI();
        if (timeRemaining <= 0f) EndMinigame(byVoice: false);
    }

    // API

    public void StartMinigame()
    {
        RefreshDuration();
        localScore = 0;
        isActive = true;

        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive())
            GameManager.Instance.StartGame();

        RefreshUI();
        Debug.Log($"[{gameName}] ¡Partida iniciada! Duración: {timeRemaining}s");
    }

    public void AddPoints(int points)
    {
        if (!isActive) return;
        int multiplied = points * (DifficultyManager.Instance?.GetMultiplier() ?? 1);
        localScore += multiplied;
        UpdateLocalScoreUI();
        GameManager.Instance?.AddScore(multiplied);
    }

    public void ForceEnd()
    {
        if (!isActive) return; 
        EndMinigame(byVoice: true);
    }

    void EndMinigame(bool byVoice = false)
    {
        isActive = false;
        timeRemaining = 0f;
        UpdateTimerUI();

        if (localScore > record)
        {
            record = localScore;
            UpdateRecordUI();
        }

        // Solo suena timeUpSound si se acabó el tiempo (no por voz)
        if (!byVoice && timeUpSound != null)
            audioSource?.PlayOneShot(timeUpSound);

        // Avisar al GameManager sin sonido
        GameManager.Instance?.EndGame(byVoice: false);

        Debug.Log($"[{gameName}] Terminada. Puntos: {localScore}");
    }

    // Dificultad

    public void SelectDifficulty(int level, bool playSound = true)
    {
        if (isActive)
        {
            Debug.Log($"[{gameName}] No se puede cambiar la dificultad con la partida activa.");
            return;
        }

        DifficultyManager.Instance?.SetDifficulty(level, playSound);

        SetButtonColor(easyButton, defaultColor);
        SetButtonColor(normalButton, defaultColor);
        SetButtonColor(hardButton, defaultColor);
        Button selected = level == 0 ? easyButton :
                          level == 1 ? normalButton : hardButton;
        SetButtonColor(selected, selectedColor);

        // Actualizar el timer con el nuevo tiempo
        RefreshDuration();
        UpdateTimerUI();
    }

    void SetButtonColor(Button btn, Color color)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = color;
    }

    // Voice text

    void UpdateVoiceText()
    {
        if (voiceText == null) return;

        if (showAmmo)
        {
            // Pistola
            voiceText.text = "· Di \"EMPEZAR DISPAROS\" para iniciar\n" +
                             "· Di \"RECARGAR\" para munición\n" +
                             "· Di \"DIFICULTAD FÁCIL/NORMAL/DIFÍCIL\"\n" +
                             "· Di \"TERMINAR PARTIDA\" para finalizar";
        }
        else if (gameName == "Dardos")
        {
            voiceText.text = "· Di \"EMPEZAR DARDOS\" para iniciar\n" +
                             "· Di \"DIFICULTAD FÁCIL/NORMAL/DIFÍCIL\"\n" +
                             "· Di \"TERMINAR PARTIDA\" para finalizar";
        }
        else if (gameName == "Hockey")
        {
            voiceText.text = "· Di \"EMPEZAR HOCKEY\" para iniciar\n" +
                             "· Di \"DIFICULTAD FÁCIL/NORMAL/DIFÍCIL\"\n" +
                             "· Di \"TERMINAR PARTIDA\" para finalizar";
        }
    }


    void RefreshDuration()
    {
        if (DifficultyManager.Instance != null)
            timeRemaining = DifficultyManager.Instance.timeLimits[DifficultyManager.Instance.currentDifficulty];
        else
            timeRemaining = defaultDuration;
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;
        int secs = Mathf.CeilToInt(timeRemaining);
        timerText.text = secs.ToString();
        timerText.color = secs <= 10 ? Color.red : Color.white;
    }

    void UpdateLocalScoreUI()
    {
        if (localScoreText != null)
            localScoreText.text = localScore.ToString();
    }

    void UpdateRecordUI()
    {
        if (recordText != null)
            recordText.text = record.ToString();
    }

    void RefreshUI()
    {
        UpdateTimerUI();
        UpdateLocalScoreUI();
        UpdateRecordUI();
    }
}