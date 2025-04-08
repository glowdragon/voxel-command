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
        private UnitManager _unitManager;

        [SerializeField, Tooltip("Maximum number of units per team")]
        private int _maxTeamSize = 12;
        public int MaxTeamSize => _maxTeamSize;

        [SerializeField]
        private Transform _team1SpawnPoint;

        public Transform Team1SpawnPoint => _team1SpawnPoint;

        [SerializeField]
        private Transform _team2SpawnPoint;

        public Transform Team2SpawnPoint => _team2SpawnPoint;

        private ReactiveCollection<Unit> _team1Units = new();
        public IReadOnlyReactiveCollection<Unit> Team1Units => _team1Units;

        private ReactiveCollection<Unit> _team2Units = new();
        public IReadOnlyReactiveCollection<Unit> Team2Units => _team2Units;

        public Subject<Team> OnTeamVictory = new Subject<Team>();
        public Subject<bool> OnGameOver = new Subject<bool>();

        private void Awake()
        {
            StartSynchronizingUnits();
        }

        private void StartSynchronizingUnits()
        {
            _unitManager
                .Units.ObserveAdd()
                .Subscribe(evt =>
                {
                    AddUnitToTeam(evt.Value);
                });
            _unitManager
                .Units.ObserveRemove()
                .Subscribe(evt =>
                {
                    RemoveUnitFromTeam(evt.Value);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// Adds a unit to the appropriate team
        /// </summary>
        public void AddUnitToTeam(Unit unit)
        {
            if (unit == null)
                return;

            // Add to appropriate team list
            if (unit.Team == Team.Player)
            {
                _team1Units.Add(unit);
            }
            else
            {
                _team2Units.Add(unit);
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
        }

        /// <summary>
        /// Removes a unit from the appropriate team
        /// </summary>
        public void RemoveUnitFromTeam(Unit unit)
        {
            _team1Units.Remove(unit);
            _team2Units.Remove(unit);
        }

        /// <summary>
        /// Called when a unit is destroyed to update team status
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
        /// Revives all units on a team without repositioning them
        /// </summary>
        public void ReviveTeam(Team team)
        {
            List<Unit> teamUnits = team == Team.Player ? _team1Units.ToList() : _team2Units.ToList();

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
                OnGameOver.OnNext(false);
            }
            else if (aliveTeam2 <= 0 && aliveTeam1 > 0)
            {
                Debug.Log("Battle Over: Player team wins!");
                OnTeamVictory.OnNext(Team.Player);
            }
            else if (aliveTeam1 <= 0 && aliveTeam2 <= 0)
            {
                Debug.Log("Battle Over: Draw - both teams eliminated!");
                // In a roguelike, even a draw counts as a loss
                OnGameOver.OnNext(false);
            }
        }

        public List<Unit> GetTeamUnits(Team team)
        {
            return team == Team.Player ? _team1Units.ToList() : _team2Units.ToList();
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
        /// Makes the winning team perform victory animations
        /// </summary>
        public void CelebrateVictory(Team winningTeam)
        {
            List<Unit> winningTeamUnits = GetTeamUnits(winningTeam);

            foreach (Unit unit in winningTeamUnits)
            {
                if (unit != null && unit.State.Health.Value > 0)
                {
                    // Make the unit dance
                    unit.Controller.PlayVictoryAnimation();
                }
            }
        }

        /// <summary>
        /// Calculate how many units each team should have for the current round
        /// </summary>
        public (int allyCount, int enemyCount) CalculateTeamSizes(int maxTeamSize, int allyBaseCount, int enemyBaseCount, int currentRound)
        {
            // Calculate how many units to spawn
            int alliesToHave = Mathf.Min(maxTeamSize, allyBaseCount + (currentRound - 1));
            int enemiesToHave = Mathf.Min(maxTeamSize, enemyBaseCount + (currentRound - 1));

            return (alliesToHave, enemiesToHave);
        }

        public Transform GetSpawnPoint(Team team)
        {
            return team == Team.Player ? _team1SpawnPoint : _team2SpawnPoint;
        }
    }
}
