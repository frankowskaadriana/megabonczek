using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class AutoClimbLinkGenerator : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask wallLayer;
    public float wallHeight = 3f;
    public float linkWidth = 1f;

    void Start()
    {
        GenerateClimbLinks();
    }

    void GenerateClimbLinks()
    {
        Collider[] walls = Physics.OverlapSphere(transform.position, 50f, wallLayer);
        List<Vector3> climbPoints = new List<Vector3>();

        foreach (Collider wall in walls)
        {
            Bounds bounds = wall.bounds;

            // Dodaj punkt na dole œciany
            Vector3 bottomPoint = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            Vector3 topPoint = new Vector3(bounds.center.x, bounds.min.y + wallHeight, bounds.center.z);

            climbPoints.Add(bottomPoint);
            climbPoints.Add(topPoint);
        }

        // Utwórz NavMeshLink dla ka¿dej pary punktów
        for (int i = 0; i < climbPoints.Count; i += 2)
        {
            if (i + 1 < climbPoints.Count)
            {
                CreateNavMeshLink(climbPoints[i], climbPoints[i + 1]);
            }
        }
    }

    void CreateNavMeshLink(Vector3 start, Vector3 end)
    {
        GameObject linkObject = new GameObject("ClimbLink");
        linkObject.transform.position = (start + end) / 2;

        NavMeshLink link = linkObject.AddComponent<NavMeshLink>();
        link.startPoint = start - linkObject.transform.position;
        link.endPoint = end - linkObject.transform.position;
        link.width = linkWidth;
        link.autoUpdate = true;

        linkObject.transform.SetParent(transform);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 50f);
    }
}