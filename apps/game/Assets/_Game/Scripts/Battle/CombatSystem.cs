using System.Collections.Generic;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Manages combat interactions between units, including AI behavior for targeting and movement.
    /// This system operates at a high level, making strategic decisions for groups of units.
    /// </summary>
    public class CombatSystem : DisposableMonoBehaviour
    {
        [Inject]
        private TeamManager _teamManager;

        [SerializeField, Tooltip("Time between AI updates in seconds. Controls how frequently units make new decisions.")]
        private float _aiUpdateInterval = 1f;

        [SerializeField, Tooltip("Distance at which units will engage enemies")]
        private float _engageDistance = 15f;

        [SerializeField, Tooltip("Probability per AI update that a unit will reconsider its current target")]
        private float _retargetProbability = 0.1f;

        /// <summary>
        [SerializeField, Tooltip("Probability to move toward enemy when not engaged in combat")]
        private float _moveTowardEnemyChance = 0.2f;

        [SerializeField, Tooltip("Maximum distance from target for random movement. When moving toward enemies, units will stay within this range.")]
        private float _randomMovementRange = 10f;

        [SerializeField, Tooltip("Probability to move randomly when no enemies are detected")]
        private float _randomScoutChance = 0.1f;

        /// <summary>
        /// Maximum distance for random patrol movements.
        /// Units will patrol within this range of their current position.
        /// </summary>
        [SerializeField, Tooltip("Maximum distance for random patrol movements")]
        private float _scoutMovementRange = 5f;

        /// <summary>
        /// Called when the component is enabled. Starts the AI behavior update loop.
        /// </summary>
        private void OnEnable()
        {
            StartAIBehaviour();
        }

        /// <summary>
        /// Called when the component is disabled. Cleans up any active subscriptions.
        /// </summary>
        private void OnDisable()
        {
            ClearDisposables();
        }

        /// <summary>
        /// Initializes the AI behavior update loop using UniRx's Observable system.
        /// Creates a timer that triggers AI updates at regular intervals.
        /// </summary>
        private void StartAIBehaviour()
        {
            Observable
                .Interval(System.TimeSpan.FromSeconds(_aiUpdateInterval))
                .Subscribe(_ =>
                {
                    UpdateTeamAI(_teamManager.PlayerUnits, _teamManager.EnemyUnits);
                    UpdateTeamAI(_teamManager.EnemyUnits, _teamManager.PlayerUnits);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// Updates AI decision-making for all units in a team.
        /// </summary>
        /// <param name="teamUnits">Units to update AI for</param>
        /// <param name="opposingTeamUnits">Enemy units that can be targeted</param>
        /// <remarks>
        /// This method implements the core AI decision-making logic:
        /// 1. Finds nearest enemy for each unit
        /// 2. Makes engagement decisions based on distance
        /// 3. Handles target switching
        /// 4. Manages scouting behavior when no enemies are present
        /// </remarks>
        private void UpdateTeamAI(IEnumerable<Unit> teamUnits, IEnumerable<Unit> opposingTeamUnits)
        {
            foreach (Unit unit in teamUnits)
            {
                UnitController controller = unit.Controller;

                // Skip dead and player-controlled units
                if (unit.State.IsDead || controller.IsCrowdControlled || controller.IsUnderManualControl)
                    continue;

                // Find nearest enemy for decision-making
                Unit nearestEnemy = controller.FindNearestEnemy(opposingTeamUnits);

                // =============================================
                // ENEMY FOUND - COMBAT DECISION LOGIC
                // =============================================
                if (nearestEnemy != null)
                {
                    float distanceToEnemy = Vector3.Distance(unit.transform.position, nearestEnemy.transform.position);

                    if (controller.IsInCombat)
                    {
                        // Potentially switch targets if a better one is available
                        if (Random.value < _retargetProbability)
                        {
                            if (nearestEnemy != controller.Opponent && distanceToEnemy <= _engageDistance)
                            {
                                controller.EngageTarget(nearestEnemy);
                            }
                        }
                    }
                    else
                    {
                        // Engage if enemy is within range
                        if (distanceToEnemy <= _engageDistance)
                        {
                            controller.EngageTarget(nearestEnemy);
                        }
                        // Otherwise, consider moving closer to the enemy
                        else if (Random.value < _moveTowardEnemyChance)
                        {
                            controller.MoveToRandomPositionNear(nearestEnemy.transform.position, _randomMovementRange);
                        }
                    }
                }
                // =============================================
                // NO ENEMIES - SCOUTING BEHAVIOR
                // =============================================
                else
                {
                    // controller.TransitionToState(controller.ScoutingState);

                    // Random patrol movement when no enemies are detected
                    if (controller.CurrentState != controller.VictoryState && Random.value < _randomScoutChance)
                    {
                        Vector3 randomPosition =
                            unit.transform.position
                            + new Vector3(
                                Random.Range(-_scoutMovementRange, _scoutMovementRange),
                                0f,
                                Random.Range(-_scoutMovementRange, _scoutMovementRange)
                            );

                        controller.MoveToRandomPositionNear(randomPosition, _scoutMovementRange);
                    }
                }
            }
        }
    }
}
