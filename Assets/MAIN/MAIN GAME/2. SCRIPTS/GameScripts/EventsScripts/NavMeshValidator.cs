using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Valida en el editor que la escena tenga un NavMesh horneado.
/// Se adjunta a un GameObject vacío al inicio de la escena.
/// </summary>
public class NavMeshValidator : MonoBehaviour
{
    void Start()
    {
        // Solo comprobamos en el editor o en desarrollo
        if (!Application.isEditor) return;

        // Intentar obtener un triángulo del NavMesh
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        if (triangulation.vertices.Length == 0)
        {
            Debug.LogError("[NavMeshValidator] No se encontró NavMesh horneado en la escena. " +
                           "Por favor, hornea el NavMesh (Window > AI > Navigation).");
        }
        else
        {
            Debug.Log("[NavMeshValidator] NavMesh válido encontrado.");
        }
    }
}