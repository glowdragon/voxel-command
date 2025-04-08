using System.Collections.Generic;
using UnityEngine;

namespace VoxelCommand.Client
{
    public interface IPathfindingService
    {
        bool CalculatePath(Vector3 start, Vector3 destination, List<Vector3> resultPath);
        bool TryGetRandomPointNear(Vector3 center, float radius, out Vector3 result);
        bool IsReachable(Vector3 start, Vector3 destination);
        float GetPathLength(List<Vector3> path);
    }
}
