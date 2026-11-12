using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public int goalPoints = 50;
    public AudioClip goalSound;
    private AudioSource audioSource;

    [Header("Panel de la caseta")]
    public MinigamePanel minigamePanel; 

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Throwable")) return;
        if (!GameManager.Instance.IsGameActive()) return;

        // ← CAMBIA ESTO
        if (minigamePanel != null)
            minigamePanel.AddPoints(goalPoints);
        else
            GameManager.Instance.AddScore(goalPoints);

        audioSource?.PlayOneShot(goalSound);
        Debug.Log("¡GOOOOL! +" + goalPoints);
    }
}