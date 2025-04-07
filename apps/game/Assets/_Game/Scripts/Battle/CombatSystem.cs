using System.Collections.Generic;
using UnityEngine;
using UniRx;

namespace VoxelCommand.Client
{
    public class CombatSystem : MonoBehaviour
    {
        [SerializeField]
        private float _aiUpdateInterval = 1f; // Time between AI updates
        
        [SerializeField]
        private float _engageDistance = 15f; // Distance at which enemies will engage
        
        [SerializeField]
        private float _retargetProbability = 0.1f; // Chance per AI update to reconsider target
        
        [SerializeField]
        private bool _aiEnabled = true; // Toggle for AI

        private BattleManager _battleManager;
        private float _lastAiUpdateTime;
        
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
            _lastAiUpdateTime = 0f;
        }
        
        private void Update()
        {
            if (!_aiEnabled) return;
            
            // Only update AI at specified intervals to save performance
            if (Time.time - _lastAiUpdateTime < _aiUpdateInterval) return;
            _lastAiUpdateTime = Time.time;
            
            UpdateAI();
        }
        
        /// <summary>
        /// Updates AI for all units
        /// </summary>
        private void UpdateAI()
        {
            // Update enemy AI
            UpdateTeamAI(_battleManager.GetEnemyUnits(), _battleManager.GetPlayerUnits());
            
            // If you want AI for player team too (for automatic battles)
            UpdateTeamAI(_battleManager.GetPlayerUnits(), _battleManager.GetEnemyUnits());
        }
        
        /// <summary>
        /// Updates AI for a specific team, targeting units from the opposing team
        /// </summary>
        private void UpdateTeamAI(List<Unit> teamUnits, List<Unit> opposingTeamUnits)
        {
            if (teamUnits == null || opposingTeamUnits == null)
                return;
                
            foreach (Unit unit in teamUnits)
            {
                if (unit == null || unit.State.Health.Value <= 0 || unit.Controller == null)
                    continue;
                    
                UnitController controller = unit.Controller;
                
                // Always find the nearest enemy
                Unit nearestEnemy = controller.FindNearestEnemy(opposingTeamUnits);

                // --- Logic based on whether an enemy is found --- 
                if (nearestEnemy != null)
                {
                    float distanceToEnemy = Vector3.Distance(
                        unit.transform.position, 
                        nearestEnemy.transform.position
                    );
                    
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
                        Vector3 randomPosition = unit.transform.position + new Vector3(
                            Random.Range(-10f, 10f),
                            0f,
                            Random.Range(-10f, 10f)
                        );
                        
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