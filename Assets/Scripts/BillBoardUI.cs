using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform playerCamera;

    void Start()
    {
        // Busca la cámara principal (la del casco)
        playerCamera = Camera.main.transform;
        // Posicion SR del mundo
        
    }

    void LateUpdate()
    {
        // Rota el canvas para mirar siempre al jugador
        transform.LookAt(transform.position, Vector3.up);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }
}