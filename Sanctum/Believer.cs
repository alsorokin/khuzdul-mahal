using Depths;

namespace Sanctum
{
    /// <summary>
    /// A stonesinger that makes random moves.
    /// </summary>
    public class Believer : IStonesinger
    {
        private static Random random = new();
        public Move? MakeMove(Game game)
        {
            List<Move> moves = game.GetValidMoves();

            if (moves.Any())
            {
                Move randomMove = moves[random.Next(0, moves.Count - 1)];
                return randomMove;
            }
            return null;
        }
    }
}
