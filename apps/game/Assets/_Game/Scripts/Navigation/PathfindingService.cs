using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    public class PathfindingService : MonoBehaviour, IPathfindingService
    {
        private NavMeshPath _navMeshPath;

        [Inject]
        private void Initialize()
        {
            _navMeshPath = new NavMeshPath();
        }

        /// <summary>
        /// Calculate a path from start to destination using NavMesh
        /// </summary>
        /// <returns>True if a path was found, false otherwise</returns>
        public bool CalculatePath(Vector3 start, Vector3 destination, List<Vector3> resultPath)
        {
            return CalculatePath(start, destination, resultPath, NavMesh.AllAreas);
        }

        /// <summary>
        /// Calculate a path from start to destination using NavMesh with specific area mask
        /// </summary>
        /// <returns>True if a path was found, false otherwise</returns>
        public bool CalculatePath(Vector3 start, Vector3 destination, List<Vector3> resultPath, int areaMask)
        {
            resultPath.Clear();

            if (!TryCalculateNavMeshPath(start, destination, areaMask))
            {
                return false;
            }

            resultPath.AddRange(_navMeshPath.corners);
            return true;
        }

        /// <summary>
        /// Attempts to find a random point on the NavMesh within the specified radius of the center point
        /// </summary>
        public bool TryGetRandomPointNear(Vector3 center, float radius, out Vector3 result)
        {
            return TryGetRandomPointNear(center, radius, out result, NavMesh.AllAreas);
        }

        /// <summary>
        /// Attempts to find a random point on the NavMesh within the specified radius of the center point with specific area mask
        /// </summary>
        public bool TryGetRandomPointNear(Vector3 center, float radius, out Vector3 result, int areaMask)
        {
            result = Vector3.zero;
            return NavMesh.SamplePosition(center + UnityEngine.Random.insideUnitSphere * radius, out NavMeshHit hit, radius, areaMask)
                && (result = hit.position) != Vector3.zero;
        }

        /// <summary>
        /// Checks if there is a path between two points
        /// </summary>
        public bool IsReachable(Vector3 start, Vector3 destination)
        {
            return IsReachable(start, destination, NavMesh.AllAreas);
        }

        /// <summary>
        /// Checks if there is a path between two points with specific area mask
        /// </summary>
        public bool IsReachable(Vector3 start, Vector3 destination, int areaMask)
        {
            return TryCalculateNavMeshPath(start, destination, areaMask);
        }

        /// <summary>
        /// Calculate the total length of a path
        /// </summary>
        public float GetPathLength(List<Vector3> path)
        {
            if (path.Count < 2)
                return 0f;

            float length = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += Vector3.Distance(path[i], path[i + 1]);
            }
            return length;
        }

        /// <summary>
        /// Helper method to calculate NavMesh path and check its status
        /// </summary>
        private bool TryCalculateNavMeshPath(Vector3 start, Vector3 destination, int areaMask)
        {
            if (!NavMesh.CalculatePath(start, destination, areaMask, _navMeshPath))
                return false;

            return _navMeshPath.status == NavMeshPathStatus.PathComplete;
        }
    }
}
