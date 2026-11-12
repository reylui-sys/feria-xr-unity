using UnityEngine;

public class Bullet : MonoBehaviour
{
    [HideInInspector] public int pointsOnHit = 10;

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Bala golpeó: {collision.gameObject.name}");
        
        // Intentar golpear lata
        CanTarget can = collision.gameObject.GetComponent<CanTarget>();
        if (can != null)
        {
            can.GetShot(pointsOnHit);
        }

        // Destruir la bala al impactar
        Destroy(gameObject);
    }
}