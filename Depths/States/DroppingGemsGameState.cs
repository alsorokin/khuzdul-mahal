namespace Depths.States
{
    internal class DroppingGemsGameState : IGameState
    {
        public void Progress(Game game)
        {
            // Only collect clusters when gems are settled,
            // This is mainly for Gaze convenience
            bool gemsFell = Fall(game);
            if (gemsFell)
            {
                game.lastMove = null;
                game.Tick++;
            }
            game.State = GameStates.CollectingClusters;
        }

        private static bool Fall(Game game)
        {
            bool fell = false;
            // Fill empty space
            for (int x = GameField.Width - 1; x >= 0; x--)
            {
                int? lastEmptyY = null;
                for (int y = GameField.Height - 1; y >= 0; y--)
                {
                    GemKind kind = game.Field.GetGemKindAt(x, y);
                    if (kind == GemKind.None)
                    {
                        lastEmptyY ??= y;
                        continue;
                    }

                    // Found a non-empty cell but previous ones were empty.
                    if (lastEmptyY != null)
                    {
                        fell = true;
                        game.Field.SwapGems(new Position(x, y), new Position(x, (int)lastEmptyY));
                        // As we filled the lastEmptyY cell, now it will be one row above
                        lastEmptyY--;
                    }
                }
                // Now, if any empty cells remain, they should be filled with random gems
                if (lastEmptyY != null)
                {
                    for (int y = 0; y <= lastEmptyY; y++)
                    {
                        game.Field.SetGemKindAt(x, y, Gems.GetRandomGemKind());
                    }
                }
            }
            return fell;
        }
    }
}
