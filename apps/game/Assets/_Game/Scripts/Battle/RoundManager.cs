using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class RoundManager : MonoBehaviour
    {
        [Inject]
        private SkillPointAllocationManager _skillPointAllocationManager;

        [Header("Config")]
        [SerializeField]
        private float _roundStartDelay = 1f;

        [SerializeField]
        private float _roundTransitionDelay = 3f;

        [Header("State")]
        [SerializeField]
        private IntReactiveProperty _currentRound = new(0);
        public IntReactiveProperty CurrentRound => _currentRound;

        public Subject<int> OnRoundStarted { get; } = new();
        public Subject<int> OnRoundCompleted { get; } = new();

        /// <summary>
        /// Starts the next round
        /// </summary>
        public void StartNextRound()
        {
            _currentRound.Value++;
            if (_currentRound.Value == 1)
            {
                Observable.Timer(TimeSpan.FromSeconds(_roundStartDelay)).Subscribe(_ => OnRoundStarted.OnNext(_currentRound.Value));
            }
            else
            {
                Observable.Timer(TimeSpan.FromSeconds(_roundTransitionDelay)).Subscribe(_ => OnRoundStarted.OnNext(_currentRound.Value));
            }
        }

        /// <summary>
        /// Called when a battle round is completed
        /// TODO: Use UniRx
        /// </summary>
        public IEnumerator HandleRoundOver_Co()
        {
            yield return new WaitForSeconds(0.2f);

            // Notify subscribers that round is complete
            OnRoundCompleted.OnNext(_currentRound.Value);

            // Wait for skills to be allocated if needed
            if (_skillPointAllocationManager.IsBusy.Value)
            {
                Debug.Log("Delaying round restart until skill allocation completes");
                yield return new WaitUntil(() => !_skillPointAllocationManager.IsBusy.Value);
                Debug.Log("Skill allocation complete, continuing to next round");
            }

            StartNextRound();
        }
    }
}
