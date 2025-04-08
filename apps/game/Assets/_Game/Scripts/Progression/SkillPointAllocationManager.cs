using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class SkillPointAllocationManager : MonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        [Inject]
        private UnitManager _unitManager;

        [SerializeField]
        private SkillSelectionUI _skillSelectionMenu;

        private Queue<Unit> _pendingUnits = new Queue<Unit>();

        public void Start()
        {
            _messageBroker
                .Receive<RoundCompletedEvent>()
                .Subscribe(_ => Observable.Timer(TimeSpan.FromSeconds(5f)).Subscribe(_ => StartSkillPointAllocation()).AddTo(this));
        }

        /// <summary>
        /// Starts processing the queue of units waiting for skill selection
        /// </summary>
        private void StartSkillPointAllocation()
        {
            foreach (var unit in _unitManager.Units)
            {
                if (unit.Team == Team.Player)
                {
                    for (int i = 0; i < unit.State.AvailableSkillPoints.Value; i++)
                    {
                        _pendingUnits.Enqueue(unit);
                    }
                }
                else if (unit.Team == Team.Enemy)
                {
                    SpendSkillPointsRandomly(unit, unit.State.AvailableSkillPoints.Value);
                }
            }

            Debug.Log($"Starting skill point allocation for {_pendingUnits.Count} units");

            if (_pendingUnits.Count == 0)
            {
                CompleteSkillPointAllocation();
                return;
            }

            // Publish event that skill point allocation has started
            _messageBroker.Publish(new SkillPointAllocationStartedEvent(_pendingUnits.Count));

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
                CompleteSkillPointAllocation();
                return;
            }

            Unit nextUnit = _pendingUnits.Dequeue();

            // Show the menu for this unit
            _skillSelectionMenu.Show(nextUnit);

            // Subscribe to when the menu is closed so we can process the next unit
            _skillSelectionMenu.OnMenuClosed.Take(1).Subscribe(_ => ProcessNextUnit());
        }

        /// <summary>
        /// Finishes processing the queue and cleans up
        /// </summary>
        private void CompleteSkillPointAllocation()
        {
            _skillSelectionMenu.FadeOutBackground();
            _messageBroker.Publish(new SkillPointAllocationCompletedEvent());
        }

        /// <summary>
        /// Automatically allocates a skill point to a random stat for the given unit
        /// </summary>
        private void SpendSkillPointsRandomly(Unit unit, int limit = -1)
        {
            int pointsToSpend =
                limit == -1 ? unit.State.AvailableSkillPoints.Value : Mathf.Min(limit, unit.State.AvailableSkillPoints.Value);

            // Keep allocating skills until we've spent the desired number of points
            while (pointsToSpend > 0 && unit.State.AvailableSkillPoints.Value > 0)
            {
                var statTypes = unit.State.Skills.Keys.ToList();
                var randomStatType = statTypes[UnityEngine.Random.Range(0, statTypes.Count)];
                unit.State.Skills[randomStatType].Value++;
                unit.State.AvailableSkillPoints.Value--;
                pointsToSpend--;
                Debug.Log($"Spent skill point on {randomStatType} for {unit.Name}");
            }
        }
    }
}
