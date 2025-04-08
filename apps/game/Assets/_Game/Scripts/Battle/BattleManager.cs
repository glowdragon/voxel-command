using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace VoxelCommand.Client
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _navMeshSurfaceObject;

        [SerializeField]
        private int _allyBaseCount = 2;

        [SerializeField]
        private int _enemyBaseCount = 1;

        [Inject]
        private UnitManager _unitManager;

        [Inject]
        private RoundManager _roundManager;

        [Inject]
        private TeamManager _teamManager;

        [Inject]
        private UnitSpawner _unitSpawner;

        [Inject]
        private ExperienceManager _experienceManager;

        [SerializeField]
        private CombatSystem _combatSystem;

        public void Start()
        {
            _roundManager.OnRoundStarted.Subscribe(OnRoundStarted).AddTo(this);
            _teamManager.OnTeamVictory.Subscribe(OnTeamVictory).AddTo(this);
            _teamManager.OnGameOver.Subscribe(OnGameOver).AddTo(this);

            // Start first round
            Observable.NextFrame().Subscribe(_ => _roundManager.StartNextRound()).AddTo(this);
        }

        /// <summary>
        /// Handles the start of a new round
        /// </summary>
        private void OnRoundStarted(int round)
        {
            // Calculate how many units to spawn
            (int alliesToHave, int enemiesToHave) = _teamManager.CalculateTeamSizes(
                _teamManager.MaxTeamSize,
                _allyBaseCount,
                _enemyBaseCount,
                round
            );

            Debug.Log($"Starting Round {round}: Target {alliesToHave} allies and {enemiesToHave} enemies");

            // Revive existing player units without repositioning
            _teamManager.ReviveTeam(Team.Player);

            // Spawn additional player units if needed
            int currentAllies = _teamManager.Team1Units.Count;
            int alliesToSpawn = Mathf.Max(0, alliesToHave - currentAllies);

            Debug.Log($"Reviving {currentAllies} allies, spawning {alliesToSpawn} allies");

            // Spawn any additional allies needed at spawn points
            for (int i = 0; i < alliesToSpawn; i++)
            {
                _unitSpawner.SpawnUnit(Team.Player);
            }

            // Instead of clearing enemies, revive existing ones without repositioning
            _teamManager.ReviveTeam(Team.Enemy);

            // Determine how many additional enemies to spawn
            int currentEnemies = _teamManager.Team2Units.Count;
            int enemiesToSpawn = Mathf.Max(0, enemiesToHave - currentEnemies);

            Debug.Log($"Reviving {currentEnemies} enemies, spawning {enemiesToSpawn} enemies");

            // Spawn additional enemies as needed at spawn points
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                _unitSpawner.SpawnUnit(Team.Enemy);
            }

            
            // Wait 1 second before starting combat
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                // Use the target positions if they're set, otherwise use opposing spawn positions
                Vector3 team1Target = _teamManager.Team2SpawnPoint.position;
                Vector3 team2Target = _teamManager.Team1SpawnPoint.position;

                // Order teams to move toward each other
                _teamManager.MoveTeamTo(Team.Player, team1Target);
                _teamManager.MoveTeamTo(Team.Enemy, team2Target);

                Debug.Log($"Round {_roundManager.CurrentRound.Value} - Battle started - teams engaging!");
            }).AddTo(this);
        }

        /// <summary>
        /// Handles team victory
        /// </summary>
        private void OnTeamVictory(Team team)
        {
            StartCoroutine(HandleVictory_Co(team));
        }

        private IEnumerator HandleVictory_Co(Team team)
        {
            yield return new WaitForSeconds(2.5f);

            // Have winning team celebrate
            _teamManager.CelebrateVictory(team);

            yield return new WaitForSeconds(0.2f);

            // Award XP and prepare for next round
            _experienceManager.AwardExperience();

            // Start the round transition process
            StartCoroutine(_roundManager.HandleRoundOver_Co());
        }

        /// <summary>
        /// Handles game over state
        /// </summary>
        /// <param name="victory">True if player won the game, false if defeated</param>
        private void OnGameOver(bool victory)
        {
            if (victory)
            {
                // For now, just log the message
                Debug.Log("Congratulations! You completed the run!");
            }
            else
            {
                Debug.Log("Your army was defeated! Game Over! You survived {_roundManager.CurrentRound.Value} rounds");

                // Restart the game after a short delay
                Observable.Timer(TimeSpan.FromSeconds(3)).Subscribe(_ => RestartGame()).AddTo(this);
            }
        }

        /// <summary>
        /// Restarts the current scene
        /// </summary>
        private void RestartGame()
        {
            Debug.Log("Restarting game...");
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}
