namespace Depths.States
{
    internal static class GameStates
    {
        public static readonly IGameState DroppingGems = new DroppingGemsGameState();
        public static readonly IGameState CollectingClusters = new CollectingClustersGameState();
        public static readonly IGameState Idle = new IdleGameState();
        public static readonly IGameState PerformingMove = new PerformingMoveGameState();
    }
}
