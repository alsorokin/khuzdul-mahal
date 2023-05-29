using Depths;

namespace Gaze
{
    internal static class Renderer
    {
        internal const char GemSymbol = '@';
        internal const char NoneSymbol = ' ';

        internal static void Render(Game game)
        {
            for (int y = 0; y < Game.FieldHeight; y++)
            {
                for (int x = 0; x < Game.FieldWidth; x++)
                {
                    RenderGem(game.GetGemKindAt(x, y));
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
        }

        private static void RenderGem(GemKind kind)
        {
            ConsoleColor oldForeground = Console.ForegroundColor;
            Console.ForegroundColor = GetGemColor(kind);
            Console.Write(kind == GemKind.None ? NoneSymbol : GemSymbol);
            Console.ForegroundColor = oldForeground;
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
                default:
                    throw new ArgumentOutOfRangeException($"Unknown gem kind: {kind}");
            }
        }
    }
}
