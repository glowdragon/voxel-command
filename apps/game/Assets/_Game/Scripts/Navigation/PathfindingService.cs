using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    public interface IPathfindingService
    {
        bool CalculatePath(Vector3 start, Vector3 destination, List<Vector3> resultPath);
        bool TryGetRandomPointNear(Vector3 center, float radius, out Vector3 result);
        bool IsReachable(Vector3 start, Vector3 destination);
        float GetPathLength(List<Vector3> path);
    }

    public class PathfindingService : MonoBehaviour, IPathfindingService
    {
        private NavMeshPath _navMeshPath;
        private readonly List<Vector3> _tempPathPoints = new();

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
            resultPath.Clear();
            
            if (!NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, _navMeshPath))
            {
                return false;
            }

            if (_navMeshPath.status != NavMeshPathStatus.PathComplete)
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
            result = Vector3.zero;
            return NavMesh.SamplePosition(
                center + UnityEngine.Random.insideUnitSphere * radius,
                out NavMeshHit hit,
                radius,
                NavMesh.AllAreas
            ) && (result = hit.position) != Vector3.zero;
        }

        /// <summary>
        /// Checks if there is a path between two points
        /// </summary>
        public bool IsReachable(Vector3 start, Vector3 destination)
        {
            if (!NavMesh.CalculatePath(start, destination, NavMesh.AllAreas, _navMeshPath))
            {
                return false;
            }

            return _navMeshPath.status == NavMeshPathStatus.PathComplete;
        }

        /// <summary>
        /// Calculate the total length of a path
        /// </summary>
        public float GetPathLength(List<Vector3> path)
        {
            if (path.Count < 2)
            {
                return 0f;
            }

            float length = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += Vector3.Distance(path[i], path[i + 1]);
            }

            return length;
        }
    }
}
