using Depths;

namespace Gaze
{
    internal static class Renderer
    {
        internal const char GemSymbol = '@';
        internal const char NoneSymbol = ' ';

        internal static void Render(Game game)
        {
            List<Game.Move> moves = game.GetPossibleMoves();
            for (int y = 0; y < Game.FieldHeight; y++)
            {
                for (int x = 0; x < Game.FieldWidth; x++)
                {
                    RenderGem(game.GetGemKindAt(x, y), new Position(x, y), moves);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        private static void RenderGem(GemKind kind, Position position, List<Game.Move> moves)
        {
            // Prepare
            ConsoleColor oldForeground = Console.ForegroundColor;
            ConsoleColor oldBackground = Console.BackgroundColor;

            // Set colors
            Console.ForegroundColor = GetGemColor(kind);
            Console.BackgroundColor = moves.Any(move => move.Start == position || move.End == position) ? ConsoleColor.DarkGray : ConsoleColor.Black;

            // Write
            Console.Write(kind == GemKind.None ? NoneSymbol : GemSymbol);

            // Restore
            Console.ForegroundColor = oldForeground;
            Console.BackgroundColor = oldBackground;
        }

        private static ConsoleColor GetGemColor(GemKind kind)
        {
            switch (kind)
            {
                case GemKind.Amber:
                    return ConsoleColor.DarkYellow;
                case GemKind.Amethyst:
                    return ConsoleColor.Magenta;
                case GemKind.None:
                case GemKind.Diamond:
                    return ConsoleColor.White;
                case GemKind.Emerald:
                    return ConsoleColor.Green;
                case GemKind.Ruby:
                    return ConsoleColor.Red;
                case GemKind.Sapphire:
                    return ConsoleColor.DarkBlue;
                case GemKind.Topaz:
                    return ConsoleColor.Yellow;
                case GemKind.Hypercube:
                    return ConsoleColor.DarkGray;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown gem kind: {kind}");
            }
        }
    }
}
