namespace VoxelCommand.Client
{
    /// <summary>
    /// Event fired when skill point allocation begins for player units
    /// </summary>
    public class SkillPointAllocationStartedEvent
    {
        public int PendingUnitsCount { get; }

        public SkillPointAllocationStartedEvent(int pendingUnitsCount)
        {
            PendingUnitsCount = pendingUnitsCount;
        }
    }

    /// <summary>
    /// Event fired when skill point allocation is completed for all units
    /// </summary>
    public class SkillPointAllocationCompletedEvent
    {
    }
}
