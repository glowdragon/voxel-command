namespace VoxelCommand.Client
{
    public interface IUnitStatsCalculator
    {
        int CalculateExperienceForLevel(UnitConfig config, UnitState state);
        int CalculateExperienceForLevel(UnitConfig config, int level);
        int CalculateExperienceForNextLevel(UnitConfig config, UnitState state);
        int CalculateExperienceForNextLevel(UnitConfig config, int level);
        int CalculateLevelFromExperience(UnitConfig config, UnitState state);
        float CalculateMaxHealth(UnitConfig config, UnitState state);
        float CalculateMaxHealth(UnitConfig config, int healthRank);
        float CalculateDamageOutput(UnitConfig config, UnitState state);
        float CalculateDamageOutput(UnitConfig config, int damageRank);
        float CalculateIncomingDamageReduction(UnitConfig config, UnitState state);
        float CalculateIncomingDamageReduction(UnitConfig config, int defenseRank);
        float CalculateMovementSpeed(UnitConfig config, UnitState state);
        float CalculateMovementSpeed(UnitConfig config, int speedRank);
    }
}
