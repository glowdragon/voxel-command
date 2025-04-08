using UniRx;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class ProgressionManager : MonoBehaviour
    {
        [Inject]
        private UnitManager _unitManager;

        [Inject]
        private BattleManager _battleManager;

        [SerializeField]
        private SkillSelectionMenu _skillSelectionMenu;
        
        private Queue<Unit> _pendingUnits = new Queue<Unit>();
        private bool _isProcessingQueue = false;
        
        public void Awake()
        {
            _unitManager.OnAnyUnitSkillPointGained.Subscribe(OnAnyUnitSkillPointGained);
        }

        private void OnAnyUnitSkillPointGained(Unit unit) 
        {
            // If the unit is on the enemy team, assign random skills automatically
            if (unit.Team == Team.Enemy)
            {
                AllocateRandomSkillsForEnemy(unit);
                return;
            }
            
            // For player units, add them to the queue for player selection
            _pendingUnits.Enqueue(unit);
            
            // If we're not already showing a menu, start processing the queue
            if (!_isProcessingQueue)
            {
                StartProcessingQueue();
            }
        }
        
        /// <summary>
        /// Automatically allocates random skills for enemy units
        /// </summary>
        private void AllocateRandomSkillsForEnemy(Unit unit)
        {
            // Keep allocating skills until the unit has no more skill points to spend
            while (unit.State.AvailableStatPoints.Value > 0)
            {
                // Random number between 0 and 3 to pick a skill
                int randomSkill = UnityEngine.Random.Range(0, 4);
                
                // Assign the skill based on the random number
                switch (randomSkill)
                {
                    case 0:
                        unit.AllocatePointToHealth();
                        break;
                    case 1:
                        unit.AllocatePointToDamage();
                        break;
                    case 2:
                        unit.AllocatePointToDefense();
                        break;
                    case 3:
                        unit.AllocatePointToSpeed();
                        break;
                }
            }
            
            // Debug log to show what skills were allocated (can be removed in production)
            Debug.Log($"Enemy {unit.name} automatically allocated skills - Health: {unit.State.HealthRank.Value}, " +
                     $"Damage: {unit.State.DamageRank.Value}, Defense: {unit.State.DefenseRank.Value}, " +
                     $"Speed: {unit.State.SpeedRank.Value}");
        }
        
        /// <summary>
        /// Starts processing the queue of units waiting for skill selection
        /// </summary>
        private void StartProcessingQueue()
        {
            if (_pendingUnits.Count == 0)
                return;
                
            _isProcessingQueue = true;
            
            // Fade in the background at the start of processing
            _skillSelectionMenu.FadeInBackground();
            
            // Process the first unit
            ProcessNextUnit();
        }
        
        private void ProcessNextUnit()
        {
            // If there are no more units to process, we're done
            if (_pendingUnits.Count == 0)
            {
                FinishProcessingQueue();
                return;
            }
            
            // Signal that skill allocation is in progress to delay round restart
            _battleManager.SetWaitingForSkillAllocation(true);
            
            Unit nextUnit = _pendingUnits.Dequeue();
            
            // Show the menu for this unit
            _skillSelectionMenu.Show(nextUnit);
            
            // Subscribe to when the menu is closed so we can process the next unit
            _skillSelectionMenu.OnMenuClosed.Take(1).Subscribe(_ => ProcessNextUnit());
        }
        
        /// <summary>
        /// Finishes processing the queue and cleans up
        /// </summary>
        private void FinishProcessingQueue()
        {
            _isProcessingQueue = false;
            
            // Fade out the background when we're done with all units
            _skillSelectionMenu.FadeOutBackground();
            
            // Signal that skill allocation is complete
            _battleManager.SetWaitingForSkillAllocation(false);
        }
    }
}
