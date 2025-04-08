using PrimeTween;
using UnityEngine;
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

        [SerializeField]
        private float _spawnHeight = 15f;

        [SerializeField]
        private float _fallDuration = 1.2f;

        [SerializeField]
        private Ease _fallEase = Ease.OutQuad;

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
            string name = _nameGenerator.GetUniqueName(team, _unitManager.Units);

            // Add height to spawn position for air drop
            Vector3 airPosition = spawnPosition + Vector3.up * _spawnHeight;

            // Spawn the unit in the air
            Unit unit = _unitManager.InstantiateUnit(airPosition, Quaternion.identity, team, name);

            // Temporarily disable navigation and collision while falling
            bool hadNavEnabled = false;
            if (unit.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                hadNavEnabled = agent.enabled;
                agent.enabled = false;
            }

            if (unit.TryGetComponent<Collider>(out var collider))
            {
                collider.enabled = false;
            }

            // Animate the fall
            Tween
                .Position(unit.transform, spawnPosition, _fallDuration, _fallEase)
                .OnComplete(
                    () =>
                    {
                        // Re-enable navigation and collision after landing
                        if (unit != null)
                        {
                            if (agent != null)
                                agent.enabled = hadNavEnabled;
                            if (collider != null)
                                collider.enabled = true;
                        }
                    },
                    warnIfTargetDestroyed: false
                );

            return unit;
        }
    }
}
