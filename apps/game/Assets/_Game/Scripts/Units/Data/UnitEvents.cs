using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Event fired when a unit is spawned in the game
    /// </summary>
    public class UnitSpawnedEvent
    {
        public Unit Unit { get; }
        public Vector3 Position { get; }

        public UnitSpawnedEvent(Unit unit, Vector3 position)
        {
            Unit = unit;
            Position = position;
        }
    }

    /// <summary>
    /// Event fired when a unit takes damage
    /// </summary>
    public class UnitDamagedEvent
    {
        public Unit TargetUnit { get; }
        public Unit SourceUnit { get; }
        public float DamageAmount { get; }

        public UnitDamagedEvent(Unit targetUnit, Unit sourceUnit, float damageAmount)
        {
            TargetUnit = targetUnit;
            SourceUnit = sourceUnit;
            DamageAmount = damageAmount;
        }
    }

    /// <summary>
    /// Event fired when a unit dies
    /// </summary>
    public class UnitDeathEvent
    {
        public Unit Victim { get; }
        public Unit Killer { get; }

        public UnitDeathEvent(Unit victim, Unit killer)
        {
            Victim = victim;
            Killer = killer;
        }
    }
}
