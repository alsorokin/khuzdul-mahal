using Depths;

namespace Gaze
{
    internal static class Renderer
    {
        internal const char GemSymbol = '@';
        internal static void Render(Game game)
        {
            ConsoleColor oldColor = Console.BackgroundColor;
            var clusters = game.GetClusters();
            for (int y = 0; y < Game.FieldHeight; y++)
            {
                for (int x = 0; x < Game.FieldWidth; x++)
                {
                    Console.BackgroundColor = clusters.Any(c => c.Points.Any(p => p.Item1 == x && p.Item2 == y)) ? ConsoleColor.DarkGray : ConsoleColor.Black;
                    RenderGem(game.GetGemKindAt(x, y));
                    Console.Write(' ');
                }
                Console.BackgroundColor = oldColor;
                Console.WriteLine();
            }
        }

        private static void RenderGem(GemKind kind)
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            Console.ForegroundColor = GetGemColor(kind);
            Console.Write(GemSymbol);
            Console.ForegroundColor = oldColor;
        }

        private static ConsoleColor GetGemColor(GemKind kind)
        {
            switch (kind)
            {
                case GemKind.Amber:
                    return ConsoleColor.DarkYellow;
                case GemKind.Amethyst:
                    return ConsoleColor.Magenta;
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
                default:
                    throw new ArgumentOutOfRangeException($"Unknown gem kind: {kind}");
            }
        }
    }
}
