using UnityEngine;

/// <summary>
/// Componente de la bala en modo AR.
/// Detecta impactos con aliens y con paredes (crea agujero).
/// </summary>
public class BulletAR : MonoBehaviour
{
    [HideInInspector]
    public int pointsOnHit = 10;

    [Tooltip("Prefab del agujero (HoleMask / portal / quad)")]
    public GameObject holeMaskPrefab;

    [Tooltip("Escala del agujero")]
    public float holeScale = 0.15f;

    [Tooltip("Distancia del agujero a la pared (m). Bajar si flota, subir si parpadea.")]
    public float wallOffsetDistance = 0.001f;

    [Tooltip("Máximo de agujeros simultáneos (0 = sin límite)")]
    public int maxHoles = 15;

    [Tooltip("Prefab opcional de efecto de impacto en pared (partículas)")]
    public GameObject wallImpactVFXPrefab;

    // Lista estática para limitar agujeros
    private static System.Collections.Generic.List<GameObject> activeHoles
        = new System.Collections.Generic.List<GameObject>();

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[BulletAR] Impacto con: {collision.gameObject.name}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");

        // Impacto con alien 
        Alien alien = collision.gameObject.GetComponent<Alien>();
        if (alien == null)
            alien = collision.gameObject.GetComponentInParent<Alien>();

        if (alien != null)
        {
            Debug.Log($"[BulletAR] ¡Alien impactado! Puntos: {pointsOnHit}");
            if (GameManagerAR.Instance != null)
                GameManagerAR.Instance.AddScore(pointsOnHit);

            alien.TakeHit(1, pointsOnHit);
            Destroy(gameObject);
            return;
        }

        // Impacto con pared/entorno 
        ContactPoint contact = collision.contacts[0];

        Debug.DrawRay(contact.point, contact.normal * 0.3f, Color.red, 10f);
        Debug.DrawRay(contact.point, -contact.normal * 0.3f, Color.blue, 10f);

        // Crear agujero
        if (holeMaskPrefab != null)
        {
            // Orientar mirando en dirección opuesta a la normal (hacia la pared)
            // + rotación aleatoria en Z para que no parezcan todos iguales
            Quaternion decalRotation = Quaternion.LookRotation(-contact.normal);
            decalRotation *= Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            // Separar MÍNIMAMENTE de la pared para evitar z-fighting
            // pero sin que parezca que flota
            Vector3 decalPos = contact.point + contact.normal * wallOffsetDistance;

            GameObject hole = Instantiate(holeMaskPrefab, decalPos, decalRotation);
            hole.transform.localScale = Vector3.one * holeScale;

            // Limitar número de agujeros
            if (maxHoles > 0)
            {
                activeHoles.RemoveAll(h => h == null);
                activeHoles.Add(hole);
                while (activeHoles.Count > maxHoles)
                {
                    GameObject oldest = activeHoles[0];
                    activeHoles.RemoveAt(0);
                    if (oldest != null) Destroy(oldest);
                }
            }

            Debug.Log($"[BulletAR] Agujero en {decalPos}, offset {wallOffsetDistance}m, escala {holeScale}");
        }
        else
        {
            Debug.LogWarning("[BulletAR] holeMaskPrefab NO asignado.");
        }

        if (wallImpactVFXPrefab != null)
        {
            Quaternion vfxRot = Quaternion.LookRotation(contact.normal);
            Instantiate(wallImpactVFXPrefab, contact.point, vfxRot);
        }

        Destroy(gameObject);
    }
}