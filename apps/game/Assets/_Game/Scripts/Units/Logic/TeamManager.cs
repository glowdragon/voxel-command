using System;
using System.Collections.Generic;
using System.Linq;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class TeamManager : DisposableMonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        [Inject]
        private UnitManager _unitManager;

        [SerializeField, Tooltip("Maximum number of units per team")]
        private int _maxTeamSize = 12;
        public int MaxTeamSize => _maxTeamSize;

        [SerializeField]
        private Transform _playerSpawn;
        public Transform PlayerSpawn => _playerSpawn;

        [SerializeField]
        private Transform _enemySpawn;
        public Transform EnemySpawn => _enemySpawn;

        private ReactiveCollection<Unit> _playerUnits = new();
        public IReadOnlyReactiveCollection<Unit> PlayerUnits => _playerUnits;

        private ReactiveCollection<Unit> _enemyUnits = new();
        public IReadOnlyReactiveCollection<Unit> EnemyUnits => _enemyUnits;

        private void Start()
        {
            StartSynchronizingUnits();

            _messageBroker.Receive<RoundCompletedEvent>().Subscribe(OnRoundCompleted).AddTo(_disposables);
        }

        private void StartSynchronizingUnits()
        {
            _unitManager
                .Units.ObserveAdd()
                .Subscribe(evt =>
                {
                    AddUnitToTeam(evt.Value);
                })
                .AddTo(_disposables);

            _unitManager
                .Units.ObserveRemove()
                .Subscribe(evt =>
                {
                    RemoveUnitFromTeam(evt.Value);
                })
                .AddTo(_disposables);
        }

        private void OnRoundCompleted(RoundCompletedEvent e)
        {
            // Let the winning units dance
            List<Unit> winningUnits = GetTeamUnits(e.WinningTeam);
            foreach (Unit unit in winningUnits)
            {
                if (unit.State.IsAlive)
                {
                    unit.Controller.TransitionToState(unit.Controller.VictoryState);
                }
            }
        }

        /// <summary>
        /// Adds a unit to the appropriate team
        /// </summary>
        public void AddUnitToTeam(Unit unit)
        {
            if (unit == null)
                return;

            if (unit.Team == Team.Player)
                _playerUnits.Add(unit);
            else
                _enemyUnits.Add(unit);
        }

        /// <summary>
        /// Removes a unit from the appropriate team
        /// </summary>
        public void RemoveUnitFromTeam(Unit unit)
        {
            _playerUnits.Remove(unit);
            _enemyUnits.Remove(unit);
        }

        /// <summary>
        /// Revives all units on a team without repositioning them
        /// </summary>
        public void ReviveTeam(Team team)
        {
            List<Unit> teamUnits = team == Team.Player ? _playerUnits.ToList() : _enemyUnits.ToList();

            foreach (Unit unit in teamUnits)
            {
                _unitManager.ReviveUnit(unit);
            }
        }

        public List<Unit> GetTeamUnits(Team team)
        {
            return team == Team.Player ? _playerUnits.ToList() : _enemyUnits.ToList();
        }

        /// <summary>
        /// Orders all units of a team to move to a position near a target
        /// </summary>
        public void MoveTeamTo(Team team, Vector3 target, float spreadRadius = 5f)
        {
            List<Unit> teamUnits = GetTeamUnits(team);

            foreach (Unit unit in teamUnits)
            {
                if (unit == null || unit.State.Health.Value <= 0)
                    continue;

                unit.Controller.MoveToRandomPositionNear(target, spreadRadius);
            }
        }

        /// <summary>
        /// Calculate how many units each team should have for the current round.
        /// New units are added every 2 rounds, alternating between teams:
        /// - No additional units in round 1
        /// - Player team gets a new unit in rounds 2, 6, 10, 14, etc.
        /// - Enemy team gets a new unit in rounds 4, 8, 12, 16, etc.
        /// </summary>
        public (int allyCount, int enemyCount) CalculateTeamSizes(int maxTeamSize, int allyBaseCount, int enemyBaseCount, int currentRound)
        {
            int additionalAllies = Mathf.Max(0, (currentRound + 2) / 4);
            int additionalEnemies = Mathf.Max(0, currentRound / 4);

            int alliesToHave = Mathf.Min(maxTeamSize, allyBaseCount + additionalAllies);
            int enemiesToHave = Mathf.Min(maxTeamSize, enemyBaseCount + additionalEnemies);

            return (alliesToHave, enemiesToHave);
        }

        public Transform GetSpawnPoint(Team team)
        {
            return team == Team.Player ? _playerSpawn : _enemySpawn;
        }
    }
}
