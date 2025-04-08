using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class CombatSystem : DisposableComponent
    {
        [SerializeField, Tooltip("Time between AI updates")]
        private float _aiUpdateInterval = 1f;

        [SerializeField, Tooltip("Distance at which enemies will engage")]
        private float _engageDistance = 15f;

        [SerializeField, Tooltip("Chance per AI update to reconsider target")]
        private float _retargetProbability = 0.1f;

        [SerializeField, Tooltip("Toggle for AI")]
        private bool _aiEnabled = true;

        private BattleManager _battleManager;

        private void Awake()
        {
            _battleManager = GetComponent<BattleManager>();
            if (_battleManager == null)
            {
                Debug.LogError("BattleManager not found on CombatSystem's GameObject");
                enabled = false;
            }
        }

        private void Start()
        {
            SetupAiUpdateInterval();
        }

        private void SetupAiUpdateInterval()
        {
            // Dispose any existing subscriptions
            _disposables.Clear();

            if (_aiEnabled)
            {
                // Create an Observable that emits every _aiUpdateInterval seconds
                Observable
                    .Interval(System.TimeSpan.FromSeconds(_aiUpdateInterval))
                    .Where(_ => _aiEnabled) // Only proceed if AI is enabled
                    .Subscribe(_ => UpdateAI())
                    .AddTo(_disposables);
            }
        }

        private void OnEnable()
        {
            SetupAiUpdateInterval();
        }

        private void OnDisable()
        {
            _disposables.Clear();
        }

        /// <summary>
        /// Updates AI for all units
        /// </summary>
        private void UpdateAI()
        {
            UpdateTeamAI(_battleManager.GetPlayerUnits(), _battleManager.GetEnemyUnits());
            UpdateTeamAI(_battleManager.GetEnemyUnits(), _battleManager.GetPlayerUnits());
        }

        /// <summary>
        /// Updates AI for a specific team, targeting units from the opposing team
        /// </summary>
        private void UpdateTeamAI(List<Unit> teamUnits, List<Unit> opposingTeamUnits)
        {
            foreach (Unit unit in teamUnits)
            {
                // Skip dead units or units without controllers
                if (unit == null || unit.Controller == null || unit.State.Health.Value <= 0)
                    continue;

                UnitController controller = unit.Controller;

                // --- Skip AI if unit is under manual control ---
                if (controller.IsUnderManualControl)
                {
                    continue; // Let the manual command complete
                }

                // Always find the nearest enemy
                Unit nearestEnemy = controller.FindNearestEnemy(opposingTeamUnits);

                // --- Logic based on whether an enemy is found ---
                if (nearestEnemy != null)
                {
                    float distanceToEnemy = Vector3.Distance(unit.transform.position, nearestEnemy.transform.position);

                    bool isEngaged = IsUnitEngaged(controller);

                    // --- Logic if unit is currently engaged ---
                    if (isEngaged)
                    {
                        // Periodically check if a closer target is available
                        if (Random.value < _retargetProbability)
                        {
                            // Check if the nearest enemy is different from the current one and within range
                            // Assuming UnitController has a CurrentTarget property or similar
                            if (nearestEnemy != controller.CurrentTarget && distanceToEnemy <= _engageDistance)
                            {
                                controller.EngageTarget(nearestEnemy);
                            }
                            // Optional: Could add logic here if the current target moved out of range
                        }
                    }
                    // --- Logic if unit is NOT currently engaged ---
                    else
                    {
                        // If enemy is within engage distance, engage them
                        if (distanceToEnemy <= _engageDistance)
                        {
                            controller.EngageTarget(nearestEnemy);
                        }
                        // Otherwise, consider moving towards the enemy
                        else if (Random.value < 0.2f) // 20% chance to move toward enemy
                        {
                            // Move closer to enemy position
                            controller.MoveToRandomPositionNear(nearestEnemy.transform.position, 10f);
                        }
                    }
                }
                // --- Logic if NO enemy is found ---
                else
                {
                    // If the unit was engaged, maybe disengage explicitly?
                    // For now, just apply patrol behavior

                    // No enemies found, could implement patrol behavior here
                    if (Random.value < 0.1f) // 10% chance to move randomly
                    {
                        Vector3 randomPosition =
                            unit.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));

                        controller.MoveToRandomPositionNear(randomPosition, 5f);
                    }
                }
            }
        }

        /// <summary>
        /// Check if a unit is already engaged in combat
        /// </summary>
        private bool IsUnitEngaged(UnitController controller)
        {
            // This is a simple implementation
            // In a more complex system, you might have a state machine for the unit
            return controller.IsInCombat;
        }
    }
}
