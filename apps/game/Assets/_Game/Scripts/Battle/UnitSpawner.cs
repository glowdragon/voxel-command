using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitSpawner : MonoBehaviour
    {
        [Inject]
        private INameGenerator _nameGenerator;

        [Inject]
        private UnitManager _unitManager;

        [Inject]
        private TeamManager _teamManager;

        [Inject]
        private IPathfindingService _pathfindingService;

        public Unit SpawnUnit(Team team)
        {
            // Generate a random spawn position near the spawn point
            int currentTeamSize = _teamManager.GetTeamUnits(team).Count;
            float randomOffset = Random.Range(1f, Mathf.Max(5f, 3f + currentTeamSize * 1.5f));
            Vector3 spawnPosition = _teamManager.GetSpawnPoint(team).position;
            if (_pathfindingService.TryGetRandomPointNear(spawnPosition, randomOffset, out Vector3 validPosition))
            {
                spawnPosition = validPosition;
            }

            // Generate a unique name for the unit
            string name = _nameGenerator.GetUniqueName(team);

            // Spawn the unit
            Unit unit = _unitManager.CreateUnit(spawnPosition, Quaternion.identity, team, name);

            // TOOD: add to team

            return unit;
        }
    }
}
