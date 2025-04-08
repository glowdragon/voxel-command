using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Note: This class references NavMeshSurface which is part of NavMeshComponents.
    /// You may need to import it from Unity's GitHub repository:
    /// https://github.com/Unity-Technologies/NavMeshComponents
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        // Reference to a manual NavMesh surface if using runtime NavMesh generation
        [SerializeField]
        private Transform _navMeshSurfaceObject;

        [SerializeField]
        private Transform _team1SpawnPoint;

        [SerializeField]
        private Transform _team2SpawnPoint;

        [SerializeField]
        private bool _autoBattleEnabled = true;

        [SerializeField]
        private float _battleStartDelay = 2f;

        [SerializeField]
        private float _roundTransitionDelay = 3f;

        // Maximum number of units per team
        [SerializeField]
        private int _maxTeamSize = 12;

        // Reference to the fast forward controller
        [SerializeField]
        private VoxelCommand.Client.FastForwardController _fastForwardController;

        private readonly List<Unit> _team1Units = new();
        private readonly List<Unit> _team2Units = new();

        private int _currentRound = 0;
        private int _allyBaseCount = 2;
        private int _enemyBaseCount = 1;

        [Inject]
        private IPathfindingService _pathfindingService;

        [Inject]
        private UnitManager _unitManager;

        [Inject]
        private INameGenerator _nameGenerator;

        private CombatSystem _combatSystem;
        private bool _isWaitingForSkillAllocation = false;

        // Event for when battle is paused/resumed
        public Subject<bool> OnBattlePauseStateChanged = new Subject<bool>();

        public void Start()
        {
            // Check if NavMesh is already baked for the scene
            if (!NavMesh.SamplePosition(transform.position, out _, 1.0f, NavMesh.AllAreas))
            {
                Debug.LogWarning(
                    "No NavMesh detected in scene. Please bake a NavMesh or add NavMeshComponents to generate one at runtime."
                );
            }

            // Initialize round counter (will be incremented in StartNextRound)
            _currentRound = 0;

            // Add FastForwardController if not assigned
            if (_fastForwardController == null)
            {
                _fastForwardController = GetComponent<VoxelCommand.Client.FastForwardController>();
                if (_fastForwardController == null)
                {
                    _fastForwardController = gameObject.AddComponent<VoxelCommand.Client.FastForwardController>();
                    Debug.Log("FastForwardController was not assigned. Created new component.");
                }
            }

            // Start first round
            StartNextRound();

            // Get or add combat system
            _combatSystem = GetComponent<CombatSystem>();
            if (_combatSystem == null && _autoBattleEnabled)
            {
                _combatSystem = gameObject.AddComponent<CombatSystem>();
            }
        }

        /// <summary>
        /// Handles game over state
        /// </summary>
        /// <param name="victory">True if player won the game, false if defeated</param>
        private void GameOver(bool victory)
        {
            Debug.Log($"Game Over! Victory: {victory}");

            // Restore normal time scale
            if (_fastForwardController != null)
            {
                _fastForwardController.ResetToNormalSpeed();
            }

            // Stop any ongoing battles
            if (_combatSystem != null)
            {
                _combatSystem.enabled = false;
            }

            // Disable additional round spawning
            CancelInvoke(nameof(StartNextRound));

            // Here you would typically display a game over UI
            // and provide options to restart or return to main menu

            // For now, let's just log the message
            if (victory)
            {
                Debug.Log("Congratulations! You completed the run!");

                // Optionally restart after a victory too
                // Invoke(nameof(RestartGame), 5f); // Restart after 5 seconds
            }
            else
            {
                Debug.Log("Your army was defeated! Game Over!");

                // Optional: You could display stats like rounds survived, enemies defeated, etc.
                Debug.Log($"Survived {_currentRound} rounds");

                // Restart the game after a short delay
                Invoke(nameof(RestartGame), 3f);
            }
        }

        /// <summary>
        /// Starts the next round with appropriate unit counts
        /// </summary>
        private void StartNextRound()
        {
            _currentRound++;

            // Calculate how many units to spawn
            int alliesToHave = Mathf.Min(_maxTeamSize, _allyBaseCount + (_currentRound - 1));
            int enemiesToHave = Mathf.Min(_maxTeamSize, _enemyBaseCount + (_currentRound - 1));

            // Every 3rd round, add one more ally
            // if (_currentRound > 1 && _currentRound % 3 == 0)
            // {
            //     alliesToHave++;
            // }

            Debug.Log(
                $"Starting Round {_currentRound}: Target {alliesToHave} allies and {enemiesToHave} enemies (capped at {_maxTeamSize})"
            );

            // Revive existing player units without repositioning
            ReviveTeam(Team.Player);

            // Spawn additional player units if needed
            int currentAllies = _team1Units.Count;
            int alliesToSpawn = Mathf.Max(0, alliesToHave - currentAllies);

            Debug.Log($"Reviving {currentAllies} existing allies, spawning {alliesToSpawn} additional allies");

            // Spawn any additional allies needed at spawn points
            for (int i = 0; i < alliesToSpawn; i++)
            {
                SpawnUnit(null, Team.Player);
            }

            // Instead of clearing enemies, revive existing ones without repositioning
            ReviveTeam(Team.Enemy);

            // Determine how many additional enemies to spawn
            int currentEnemies = _team2Units.Count;
            int enemiesToSpawn = Mathf.Max(0, enemiesToHave - currentEnemies);

            Debug.Log($"Reviving {currentEnemies} existing enemies, spawning {enemiesToSpawn} additional enemies");

            // Spawn additional enemies as needed at spawn points
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                SpawnUnit(null, Team.Enemy);
            }

            // If auto battle is enabled, start the battle after a delay
            if (_autoBattleEnabled)
            {
                Invoke(nameof(StartBattle), _battleStartDelay);
            }
        }

        /// <summary>
        /// Awards experience to all units based on their performance
        /// </summary>
        private void AwardExperience()
        {
            Debug.Log("Awarding experience to units based on performance");

            // Award experience to player units
            AwardExperienceToTeam(_team1Units);

            // Award experience to enemy units - they also progress between rounds
            AwardExperienceToTeam(_team2Units);
        }

        /// <summary>
        /// Awards experience to all units in a team based on their performance
        /// </summary>
        private void AwardExperienceToTeam(List<Unit> units)
        {
            foreach (Unit unit in units)
            {
                if (unit != null)
                {
                    // Calculate XP based on damage dealt and kills
                    int damageXp = Mathf.RoundToInt(unit.State.DamageDealt.Value * 0.5f);
                    int killXp = unit.State.Kills.Value * 100;
                    int totalXp = damageXp + killXp;

                    // Award XP
                    unit.AddExperience(totalXp);

                    Debug.Log($"{unit.name} earned {totalXp} XP (Damage: {unit.State.DamageDealt.Value}, Kills: {unit.State.Kills.Value})");

                    // Reset stats for next round
                    unit.ResetBattleStats();
                }
            }
        }

        /// <summary>
        /// Spawns a unit for the specified team at their spawn point
        /// </summary>
        public Unit SpawnUnit(UnitConfig config, Team team)
        {
            // Determine spawn point based on team
            Transform spawnPoint = team == Team.Player ? _team1SpawnPoint : _team2SpawnPoint;

            if (spawnPoint == null)
            {
                Debug.LogError($"No spawn point set for {team}");
                return null;
            }

            // Calculate a random offset to prevent units from overlapping
            // Scale the offset radius based on team size
            int currentTeamSize = team == Team.Player ? _team1Units.Count : _team2Units.Count;
            float offsetScale = Mathf.Max(5f, 3f + currentTeamSize * 1.5f);
            Vector2 randomOffset = Random.insideUnitCircle * offsetScale;
            Vector3 spawnPosition = spawnPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);

            // Sample the navmesh to ensure the spawn position is valid
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                spawnPosition = hit.position;
            }
            else
            {
                // Fall back to spawn point if navmesh sampling fails
                spawnPosition = spawnPoint.position;
                Debug.LogWarning($"Failed to find valid NavMesh position for unit spawn. Using default spawn point.");
            }

            // Use UnitManager to create unit at the spawn position
            Unit unit = _unitManager.CreateUnit(spawnPosition, Quaternion.identity, team, config);

            if (unit == null)
            {
                Debug.LogError("Failed to create unit using UnitManager");
                return null;
            }

            // Add to appropriate team list
            if (team == Team.Player)
            {
                _team1Units.Add(unit);
                unit.name = _nameGenerator.GetUniqueName(team);
            }
            else
            {
                _team2Units.Add(unit);
                unit.name = _nameGenerator.GetUniqueName(team);
            }

            // Subscribe to unit's health for death detection
            unit.State.Health.Subscribe(health =>
                {
                    if (health <= 0)
                    {
                        OnUnitDestroyed(unit);
                    }
                })
                .AddTo(unit);

            return unit;
        }

        /// <summary>
        /// Orders all units of a team to move to a position near a target
        /// </summary>
        public void MoveTeamTo(Team team, Vector3 target, float spreadRadius = 5f)
        {
            List<Unit> teamUnits = team == Team.Player ? _team1Units : _team2Units;

            foreach (Unit unit in teamUnits)
            {
                UnitController controller = unit.Controller;
                if (controller != null)
                {
                    controller.MoveToRandomPositionNear(target, spreadRadius);
                }
            }
        }

        /// <summary>
        /// Called when a unit is destroyed to remove it from team lists
        /// </summary>
        public void OnUnitDestroyed(Unit unit)
        {
            if (unit == null)
                return;

            Debug.Log($"{unit.name} has been defeated!");

            // Don't remove units from the team list, just mark them as defeated
            // We'll keep them in the list so they can be revived in the next round

            // Check if battle is over
            CheckBattleStatus();
        }

        /// <summary>
        /// Checks if one team has won
        /// </summary>
        private void CheckBattleStatus()
        {
            // Count alive units
            int aliveTeam1 = 0;
            int aliveTeam2 = 0;

            foreach (Unit unit in _team1Units)
            {
                if (unit != null && unit.State.Health.Value > 0)
                {
                    aliveTeam1++;
                }
            }

            foreach (Unit unit in _team2Units)
            {
                if (unit != null && unit.State.Health.Value > 0)
                {
                    aliveTeam2++;
                }
            }

            // Declare winner if one team is eliminated
            if (aliveTeam1 <= 0 && aliveTeam2 > 0)
            {
                Debug.Log("Battle Over: Enemy team wins!");
                // Game over - the player lost (roguelike style)
                GameOver(false);
            }
            else if (aliveTeam2 <= 0 && aliveTeam1 > 0)
            {
                Debug.Log("Battle Over: Player team wins!");

                StartCoroutine(BattleOver_Co());
            }
            else if (aliveTeam1 <= 0 && aliveTeam2 <= 0)
            {
                Debug.Log("Battle Over: Draw - both teams eliminated!");
                // In a roguelike, even a draw counts as a loss
                GameOver(false);
            }
        }

        private IEnumerator BattleOver_Co()
        {
            yield return new WaitForSeconds(0.2f);
            // Have winning team celebrate
            CelebrateVictory(Team.Player);
            yield return new WaitForSeconds(0.2f);

            // Award XP and prepare for next round
            AwardExperience();
            
            // Wait for skills to be allocated if needed
            if (_isWaitingForSkillAllocation)
            {
                Debug.Log("Delaying round restart until skill allocation completes");
                yield return new WaitUntil(() => !_isWaitingForSkillAllocation);
                Debug.Log("Skill allocation complete, continuing to next round");
            }
            
            yield return new WaitForSeconds(_roundTransitionDelay);
            StartNextRound();
        }

        /// <summary>
        /// Makes the winning team perform victory dances
        /// </summary>
        private void CelebrateVictory(Team winningTeam)
        {
            List<Unit> winningTeamUnits = winningTeam == Team.Player ? _team1Units : _team2Units;

            foreach (Unit unit in winningTeamUnits)
            {
                if (unit != null && unit.State.Health.Value > 0 && unit.Controller != null)
                {
                    // Make the unit dance
                    unit.Controller.PlayVictoryAnimation();
                }
            }

            Debug.Log($"The {winningTeam} team celebrates their victory!");
        }

        /// <summary>
        /// Restarts the current scene
        /// </summary>
        private void RestartGame()
        {
            Debug.Log("Restarting game...");

            // Get the current scene index or name
            Scene currentScene = SceneManager.GetActiveScene();

            // Reload the current scene
            SceneManager.LoadScene(currentScene.name);
        }

        /// <summary>
        /// Initiates the battle, making teams engage each other
        /// </summary>
        public void StartBattle()
        {
            // For demonstration, we'll move each team toward the other's spawn point
            Vector3 team1Target = _team2SpawnPoint.position;
            Vector3 team2Target = _team1SpawnPoint.position;

            // Order teams to move toward each other
            MoveTeamTo(Team.Player, team1Target);
            MoveTeamTo(Team.Enemy, team2Target);

            Debug.Log($"Round {_currentRound} - Battle started - teams engaging!");
        }

        /// <summary>
        /// Revives all units on a team without repositioning them
        /// </summary>
        private void ReviveTeam(Team team)
        {
            List<Unit> teamUnits = team == Team.Player ? _team1Units : _team2Units;

            foreach (Unit unit in teamUnits)
            {
                if (unit != null)
                {
                    // Make sure the unit is active and enabled
                    unit.gameObject.SetActive(true);

                    // Reset unit health to full
                    unit.State.Health.Value = unit.State.MaxHealth.Value;

                    // Reset combat state (keep position unchanged)
                    if (unit.Controller != null)
                    {
                        unit.Controller.ResetCombatState();
                    }
                }
            }
        }

        /// <summary>
        /// Returns all player units
        /// </summary>
        public List<Unit> GetPlayerUnits()
        {
            return _team1Units;
        }

        /// <summary>
        /// Returns all enemy units
        /// </summary>
        public List<Unit> GetEnemyUnits()
        {
            return _team2Units;
        }

        /// <summary>
        /// Sets a flag to indicate that skill points are being allocated
        /// </summary>
        public void SetWaitingForSkillAllocation(bool waiting)
        {
            _isWaitingForSkillAllocation = waiting;
            Debug.Log($"Battle {(waiting ? "paused" : "resumed")} for skill allocation");
        }
    }
}
