using Depths;

namespace Gaze
{
    internal static class Renderer
    {
        internal const char GemSymbol = '@';
        internal static void Render(Game game)
        {
            for (int i = 0; i < Game.FieldWidth; i++)
            {
                for (int j = 0; j < Game.FieldHeight; j++)
                {
                    RenderGem(game.GetGemKindAt(i, j));
                    Console.Write(' ');
                }
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
