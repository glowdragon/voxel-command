namespace VoxelCommand.Client
{
    public class RoundStartedEvent
    {
        public int RoundNumber { get; }

        public RoundStartedEvent(int roundNumber)
        {
            RoundNumber = roundNumber;
        }
    }

    public class RoundCompletedEvent
    {
        public int RoundNumber { get; }
        public Team WinningTeam { get; }

        public RoundCompletedEvent(int roundNumber, Team winningTeam)
        {
            RoundNumber = roundNumber;
            WinningTeam = winningTeam;
        }
    }
}
