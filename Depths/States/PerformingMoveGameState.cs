using System.Diagnostics;

namespace Depths.States
{
    internal class PerformingMoveGameState : IGameState
    {
        public void Progress(Game game)
        {
            // If a move was scheduled, perform it
            if (game.currentMove != null)
            {
                ExecuteCurrentMove(game);
                game.State = GameStates.CollectingClusters;
            }
            else
            {
                game.State = GameStates.Idle;
            }
        }

        private static void ExecuteCurrentMove(Game game)
        {
            Debug.Assert(game.currentMove.HasValue, "There must be a current move to execute");
            game.Combo = 0;
            game.Field.SwapGems(game.currentMove.Value.Start, game.currentMove.Value.End);
            game.lastMove = game.currentMove;
            game.currentMove = null;
            game.Tick++;
        }
    }
}
