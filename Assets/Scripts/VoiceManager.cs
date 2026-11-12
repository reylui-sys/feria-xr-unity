using UnityEngine;

public class VoiceManager : MonoBehaviour
{
    [Header("Paneles — arrastra el MinigamePanel de cada caseta")]
    public MinigamePanel gunPanel;
    public MinigamePanel dartsPanel;
    public MinigamePanel hockeyPanel;

    [Header("Referencias de juego")]
    public GunController gun;
    public DartStick dart;
    public HockeyDisc hockeyDisc;
    public Transform hockeyCenter;

    [Header("Voz solo en PC/Editor")]
    public bool useVoiceOnPC = true;

    [Header("Diana de dardos")]
    public DartBoard dartBoard;

    public GameObject partida;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private UnityEngine.Windows.Speech.KeywordRecognizer keywordRecognizer;
    private System.Collections.Generic.Dictionary<string, System.Action> commands = new();
#endif


    void Start()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        if (useVoiceOnPC) InitVoice();
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        keywordRecognizer?.Stop();
        keywordRecognizer?.Dispose();
#endif
    }

    //Inicialización de voz

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    void InitVoice()
    {
        //Iniciar cada juego
        commands["empezar disparos"] = () => StartGame(gunPanel, "Pistola");
        commands["empezar dardos"] = () => StartGame(dartsPanel, "Dardos");
        commands["empezar hockey"] = () => StartGame(hockeyPanel, "Hockey");

        commands["terminar partida"] = () =>
        {
            bool hayPartidaActiva = (gunPanel != null && gunPanel.IsActive) ||
                                    (dartsPanel != null && dartsPanel.IsActive) ||
                                    (hockeyPanel != null && hockeyPanel.IsActive);

            if (!hayPartidaActiva)
            {
                Debug.Log("[VoiceManager] No hay ninguna partida activa.");
                return;
            }

            gunPanel?.ForceEnd();
            dartsPanel?.ForceEnd();
            hockeyPanel?.ForceEnd();
            GameManager.Instance.EndGame(byVoice: true); // ← sonido de voz
            Debug.Log("[VoiceManager] Partida terminada por voz.");
        };

        // Acciones
        commands["recargar"] = ReloadGun;

        //Dificultad 
        commands["dificultad fácil"] = () => SetDifficulty(0);
        commands["dificultad normal"] = () => SetDifficulty(1);
        commands["dificultad difícil"] = () => SetDifficulty(2);
    

        keywordRecognizer = new UnityEngine.Windows.Speech.KeywordRecognizer(
            new System.Collections.Generic.List<string>(commands.Keys).ToArray()
        );

        keywordRecognizer.OnPhraseRecognized += args =>
        {
            Debug.Log($"[VoiceManager] Reconocido: '{args.text}'");
            if (commands.TryGetValue(args.text, out var action))
                action.Invoke();
        };

        keywordRecognizer.Start();
        Debug.Log($"[VoiceManager] Voice mode listo con {commands.Count} comandos.");
    }
#endif

    //Acciones

    void StartGame(MinigamePanel panel, string name)
    {
        if (panel == null)
        {
            Debug.LogError($"[VoiceManager] El panel de {name} no está asignado en el Inspector.");
            return;
        }
        Debug.Log($"[VoiceManager] Iniciando {name}...");
        panel.StartMinigame();
    }

    void ReloadGun()
    {
        if (gun == null) { Debug.LogWarning("[VoiceManager] GunController no asignado."); return; }
        gun.Reload();
        Debug.Log("[VoiceManager] Recargando pistola.");
    }


    void SetDifficulty(int level)
    {
        // No dejar cambiar si algún juego está activo
        if ((gunPanel != null && gunPanel.IsActive) ||
            (dartsPanel != null && dartsPanel.IsActive) ||
            (hockeyPanel != null && hockeyPanel.IsActive))
        {
            Debug.Log("[VoiceManager] No se puede cambiar la dificultad con una partida activa.");
            return;
        }

        DifficultyManager.Instance?.SetDifficulty(level);

        gunPanel?.SelectDifficulty(level);
        dartsPanel?.SelectDifficulty(level);
        hockeyPanel?.SelectDifficulty(level);

        dartBoard?.ApplyDifficulty();

        Debug.Log($"[VoiceManager] Dificultad cambiada a {level}.");
    }
}