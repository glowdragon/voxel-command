using System;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class RoundManager : MonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        [SerializeField]
        private bool _startRoundOnStart = true;

        [Header("Config")]
        [SerializeField, Tooltip("The delay before the first round starts")]
        private float _roundStartDelay = 1f;

        [SerializeField, Tooltip("The delay before the next round starts")]
        private float _roundTransitionDelay = 3f;

        [Header("State")]
        [SerializeField]
        private IntReactiveProperty _currentRound = new(0);
        public IntReactiveProperty CurrentRound => _currentRound;

        private void Start()
        {
            if (_startRoundOnStart)
            {
                StartFirstRound();
            }
        }

        public void StartFirstRound()
        {
            _currentRound.Value = 1;
            StartRoundIn(_currentRound.Value, _roundStartDelay);
        }

        public void StartNextRound()
        {
            _currentRound.Value++;
            StartRoundIn(_currentRound.Value, _roundTransitionDelay);
        }

        private void StartRoundIn(int round, float delay)
        {
            Observable.Timer(TimeSpan.FromSeconds(delay)).Subscribe(_ => _messageBroker.Publish(new RoundStartedEvent(round)));
        }

        public void CompleteRound(Team winningTeam)
        {
            _messageBroker.Receive<SkillPointAllocationCompletedEvent>().Take(1).Subscribe(_ => StartNextRound());
            _messageBroker.Publish(new RoundCompletedEvent(_currentRound.Value, winningTeam));
        }
    }
}
