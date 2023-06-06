namespace Depths.States
{
    internal class IdleGameState : IGameState
    {
        public void Progress(Game game)
        {
            // If a move was scheduled, perform it
            if (game.currentMove != null)
            {
                game.State = GameStates.PerformingMove;
                game.Tick++;
            }
        }
    }
}
