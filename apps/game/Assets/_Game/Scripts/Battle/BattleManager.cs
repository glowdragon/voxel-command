using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.AI;
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
        
        private readonly List<Unit> _team1Units = new();
        private readonly List<Unit> _team2Units = new();
        
        [Inject]
        private IPathfindingService _pathfindingService;
        
        [Inject]
        private UnitManager _unitManager;
        
        [Inject]
        private INameGenerator _nameGenerator;
        
        private CombatSystem _combatSystem;

        public void Start()
        {
            // Check if NavMesh is already baked for the scene
            if (!NavMesh.SamplePosition(transform.position, out _, 1.0f, NavMesh.AllAreas))
            {
                Debug.LogWarning("No NavMesh detected in scene. Please bake a NavMesh or add NavMeshComponents to generate one at runtime.");
            }
            
            // Create 5 units on both sides
            for (int i = 0; i < 10; i++)
            {
                SpawnUnit(null, Team.Player);
                SpawnUnit(null, Team.Enemy);
            }
            
            // Get or add combat system
            _combatSystem = GetComponent<CombatSystem>();
            if (_combatSystem == null && _autoBattleEnabled)
            {
                _combatSystem = gameObject.AddComponent<CombatSystem>();
            }
            
            // If auto battle is enabled, start the battle after a delay
            if (_autoBattleEnabled)
            {
                Invoke(nameof(StartBattle), _battleStartDelay);
            }
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
            
            Debug.Log("Battle started - teams engaging!");
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
            Vector2 randomOffset = Random.insideUnitCircle * 15f;
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
            unit.State.Health
                .Subscribe(health => {
                    if (health <= 0) {
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
            if (unit == null) return;
            
            Debug.Log($"{unit.name} has been defeated!");
            
            if (unit.Team == Team.Player)
            {
                _team1Units.Remove(unit);
            }
            else
            {
                _team2Units.Remove(unit);
            }
            
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
                // Could trigger game over or other events here
            }
            else if (aliveTeam2 <= 0 && aliveTeam1 > 0)
            {
                Debug.Log("Battle Over: Player team wins!");
                // Could trigger level completion or rewards here
            }
            else if (aliveTeam1 <= 0 && aliveTeam2 <= 0)
            {
                Debug.Log("Battle Over: Draw - both teams eliminated!");
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
    }
}
