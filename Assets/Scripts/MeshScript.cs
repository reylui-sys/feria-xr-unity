using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;

/// <summary>
/// Se coloca en el MeshPrefab del AR Mesh Manager.
/// Espera a que la malla del entorno esté lista, filtra los triángulos de pared,
/// crea un collider invisible para colisiones y envía los puntos de spawn
/// al GameManagerAR.
/// </summary>
public class MeshScript : MonoBehaviour
{
    [Tooltip("Umbral para la componente Y de la normal. |Y| < umbral = pared. Más bajo = más estricto.")]
    [Range(0.01f, 0.9f)]
    public float wallNormalThreshold = 0.2f;

    [Tooltip("Distancia mínima entre puntos de spawn para evitar concentración")]
    public float minDistanceBetweenPoints = 0.6f;

    private bool meshProcessed = false;

    void LateUpdate()
    {
        if (meshProcessed || Time.time < 2f) return;

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh mesh = mf.sharedMesh;
        if (mesh.vertices.Length < 3) return;

        Debug.Log($"[MeshScript] Malla recibida: {mesh.vertices.Length} vértices, {mesh.triangles.Length / 3} triángulos.");

        List<Vector3> wallPoints = new List<Vector3>();
        List<Vector3> wallNormals = new List<Vector3>();
        List<int> wallTriangles = new List<int>();

        ProcessWalls(mesh, wallTriangles, wallPoints, wallNormals);

        if (wallTriangles.Count > 0)
            CreateWallCollider(mesh, wallTriangles);

        MeshCollider mc = GetComponent<MeshCollider>();
        if (mc == null)
        {
            mc = gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }

        ARMeshManager arMeshManager = FindFirstObjectByType<ARMeshManager>();
        if (arMeshManager != null)
        {
            arMeshManager.enabled = false;
            Debug.Log("[MeshScript] ARMeshManager desactivado.");
        }

        meshProcessed = true;

        if (GameManagerAR.Instance != null)
            GameManagerAR.Instance.OnMeshReady(wallPoints, wallNormals);
        else
            Debug.LogWarning("[MeshScript] GameManagerAR.Instance es null.");
    }

    void ProcessWalls(Mesh mesh, List<int> wallTriangles,
                      List<Vector3> spawnPoints, List<Vector3> spawnNormals)
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        Camera cam = Camera.main;
        float cameraY = cam != null ? cam.transform.position.y : 1.5f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int i0 = triangles[i];
            int i1 = triangles[i + 1];
            int i2 = triangles[i + 2];

            Vector3 v0 = vertices[i0];
            Vector3 v1 = vertices[i1];
            Vector3 v2 = vertices[i2];

            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 localNormal = Vector3.Cross(edge1, edge2).normalized;
            Vector3 worldNormal = transform.rotation * localNormal;

            if (Mathf.Abs(worldNormal.y) >= wallNormalThreshold)
                continue;

            wallTriangles.Add(i0);
            wallTriangles.Add(i1);
            wallTriangles.Add(i2);

            Vector3 localCenter = (v0 + v1 + v2) / 3f;
            Vector3 worldCenter = transform.TransformPoint(localCenter);

            // Si el punto está más de 0.8m por encima de la cámara,
            // probablemente es techo. Si está más de 1.5m por debajo, es suelo.
            float heightDiff = worldCenter.y - cameraY;
            if (heightDiff > 0.8f || heightDiff < -1.5f)
                continue;

            // Distancia mínima entre puntos
            bool tooClose = false;
            for (int j = 0; j < spawnPoints.Count; j++)
            {
                if (Vector3.Distance(worldCenter, spawnPoints[j]) < minDistanceBetweenPoints)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                // Asegurar que la normal apunte hacia el jugador
                if (cam != null && Vector3.Dot(worldNormal, cam.transform.position - worldCenter) < 0)
                    worldNormal = -worldNormal;

                spawnPoints.Add(worldCenter);
                spawnNormals.Add(worldNormal.normalized);
            }
        }

        Debug.Log($"[MeshScript] Paredes: {wallTriangles.Count / 3} triángulos. Puntos de spawn: {spawnPoints.Count}");
    }

    void CreateWallCollider(Mesh originalMesh, List<int> wallTriangles)
    {
        Mesh wallsMesh = new Mesh
        {
            name = "WallsMesh",
            vertices = originalMesh.vertices,
            triangles = wallTriangles.ToArray()
        };
        wallsMesh.RecalculateNormals();
        wallsMesh.RecalculateBounds();

        GameObject wallsObj = new GameObject("WallsCollider");
        wallsObj.transform.SetParent(transform, worldPositionStays: false);
        MeshCollider mc = wallsObj.AddComponent<MeshCollider>();
        mc.sharedMesh = wallsMesh;

        Debug.Log($"[MeshScript] WallsCollider creado con {wallTriangles.Count / 3} triángulos.");
    }
}