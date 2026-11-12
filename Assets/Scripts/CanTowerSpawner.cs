using UnityEngine;

public class CanTowerSpawner : MonoBehaviour
{
    public GameObject canPrefab;
    public float spacingX = 0.06f;
    public float spacingY = 0.11f;

    [Header("Panel de la caseta")]
    public MinigamePanel minigamePanel;

    void Start()
    {
        SpawnTower();
    }

    public void SpawnTower()
    {
        int rows = 3;
        Vector3 startPos = transform.position;

        for (int y = 0; y < rows; y++)
        {
            int cansInRow = rows - y;
            for (int x = 0; x < cansInRow; x++)
            {
                Vector3 pos = startPos
                    + transform.right * (x - (cansInRow - 1) / 2f) * spacingX
                    + transform.up * y * spacingY;

                GameObject can = Instantiate(canPrefab, pos, transform.rotation);

                // Pasar el panel a cada lata
                CanTarget ct = can.GetComponent<CanTarget>();
                if (ct != null)
                    ct.minigamePanel = minigamePanel;
            }
        }
    }
}

