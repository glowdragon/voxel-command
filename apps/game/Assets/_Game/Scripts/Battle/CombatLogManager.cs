using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Manages combat log entries by subscribing to game events and formatting them for UI display
    /// </summary>
    public class CombatLogManager : MonoBehaviour
    {
        [Inject]
        private IMessageBroker _messageBroker;

        private ReactiveCollection<CombatLogEntry> _logEntries = new ReactiveCollection<CombatLogEntry>();
        public IReadOnlyReactiveCollection<CombatLogEntry> LogEntries => _logEntries;

        [SerializeField]
        private int _maxEntries = 100;

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            // Unit spawn event
            _messageBroker
                .Receive<UnitSpawnedEvent>()
                .Subscribe(e => AddLogEntry($"{e.Unit.name} has entered the battle", new Color32(180, 180, 180, 255)))
                .AddTo(this);

            // Unit death event
            _messageBroker
                .Receive<UnitDeathEvent>()
                .Subscribe(e =>
                    AddLogEntry(
                        e.Killer != null ? $"{e.Killer.name} eliminated {e.Victim.name}" : $"{e.Victim.name} has been eliminated",
                        e.Victim.Team == Team.Player ? new Color32(255, 140, 0, 255) : new Color32(100, 220, 100, 255)
                    )
                )
                .AddTo(this);

            // Round events
            _messageBroker
                .Receive<RoundCompletedEvent>()
                .Subscribe(e =>
                    AddLogEntry(
                        e.WinningTeam == Team.Player ? $"Round {e.RoundNumber} completed" : $"Round {e.RoundNumber} failed :(",
                        e.WinningTeam == Team.Player ? new Color32(120, 255, 120, 255) : new Color32(255, 100, 100, 255)
                    )
                )
                .AddTo(this);
        }

        /// <summary>
        /// Adds a new entry to the combat log, maintaining the maximum size
        /// </summary>
        private void AddLogEntry(string message, Color color)
        {
            // Add new entry
            _logEntries.Add(new CombatLogEntry(message, color));

            // Trim list if needed
            while (_logEntries.Count > _maxEntries)
            {
                _logEntries.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears all entries from the log
        /// </summary>
        public void ClearLog()
        {
            _logEntries.Clear();
        }
    }
}
