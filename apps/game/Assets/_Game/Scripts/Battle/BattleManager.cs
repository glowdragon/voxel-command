using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace VoxelCommand.Client
{
    public class BattleManager : MonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        [Inject]
        private RoundManager _roundManager;

        [Inject]
        private TeamManager _teamManager;

        [Inject]
        private UnitSpawner _unitSpawner;

        [SerializeField]


        private int _allyBaseCount = 2;

        [SerializeField]
        private int _enemyBaseCount = 1;

        public void Awake()
        {
            _messageBroker.Receive<RoundStartedEvent>().Subscribe(OnRoundStarted).AddTo(this);
            _messageBroker.Receive<UnitDeathEvent>().Subscribe(OnUnitDeath).AddTo(this);
            _messageBroker.Receive<RoundCompletedEvent>().Subscribe(OnRoundCompleted).AddTo(this);
        }

        /// <summary>
        /// Handles the start of a new round
        /// </summary>
        private void OnRoundStarted(RoundStartedEvent e)
        {
            // Calculate how many units to spawn
            (int alliesToHave, int enemiesToHave) = _teamManager.CalculateTeamSizes(
                _teamManager.MaxTeamSize,
                _allyBaseCount,
                _enemyBaseCount,
                e.RoundNumber
            );

            // Revive existing player units
            _teamManager.ReviveTeam(Team.Player);

            // Spawn additional player units if needed
            int currentAllies = _teamManager.PlayerUnits.Count;
            int alliesToSpawn = Mathf.Max(0, alliesToHave - currentAllies);
            for (int i = 0; i < alliesToSpawn; i++)
            {
                _unitSpawner.SpawnUnit(Team.Player);
            }

            // Revive existing enemy units
            _teamManager.ReviveTeam(Team.Enemy);

            // Determine how many additional enemies to spawn
            int currentEnemies = _teamManager.EnemyUnits.Count;
            int enemiesToSpawn = Mathf.Max(0, enemiesToHave - currentEnemies);
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                _unitSpawner.SpawnUnit(Team.Enemy);
            }

            // Move teams to each other's spawn points
            Observable
                .Timer(TimeSpan.FromSeconds(2))
                .Subscribe(_ =>
                {
                    _teamManager.MoveTeamTo(Team.Player, _teamManager.EnemySpawn.position);
                    _teamManager.MoveTeamTo(Team.Enemy, _teamManager.PlayerSpawn.position);
                })
                .AddTo(this);
        }

        /// <summary>
        /// Checks if one team has won
        /// </summary>
        private void OnUnitDeath(UnitDeathEvent e)
        {
            // Count alive units
            int aliveTeam1 = _teamManager.PlayerUnits.Count(unit => unit.State.IsAlive);
            int aliveTeam2 = _teamManager.EnemyUnits.Count(unit => unit.State.IsAlive);

            // Declare winner if one team is eliminated
            if (aliveTeam1 <= 0)
            {
                Observable.NextFrame().Subscribe(_ => _roundManager.CompleteRound(Team.Enemy));
            }
            else if (aliveTeam2 <= 0)
            {
                Observable.NextFrame().Subscribe(_ => _roundManager.CompleteRound(Team.Player));
            }
        }

        private void OnRoundCompleted(RoundCompletedEvent e)
        {
            if (e.WinningTeam == Team.Enemy)
            {
                Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ => RestartGame()).AddTo(this);
            }
        }

        /// <summary>
        /// Restarts the current scene
        /// </summary>
        private void RestartGame()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
