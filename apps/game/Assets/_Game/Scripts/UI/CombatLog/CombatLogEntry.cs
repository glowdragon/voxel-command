using System;
using UnityEngine;

namespace VoxelCommand.Client
{
    [Serializable]
    public class CombatLogEntry
    {
        public DateTime Timestamp { get; }
        public string Message { get; }
        public Color Color { get; }

        public CombatLogEntry(string message, Color color)
        {
            Message = message;
            Color = color;
            Timestamp = DateTime.Now;
        }

        /// <summary>
        /// Format the entry for display in the UI, including timestamp
        /// </summary>
        public string GetFormattedText()
        {
            return $"[{Timestamp:HH:mm}] {Message}";
        }
    }
}
