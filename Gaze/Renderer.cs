using Depths;
using System.Collections.ObjectModel;

namespace Gaze
{
    internal static class Renderer
    {
        private const char GemSymbol       = '@';
        private const char FireSymbol      = 'F';
        private const char StarSymbol      = 'S';
        private const char HypercubeSymbol = 'H';
        private const char SupernovaSymbol = 'N';
        private const char NoneSymbol      = '.';

        private static readonly ReadOnlyDictionary<GemPower, char> GemSymbols = new(new Dictionary<GemPower, char>
        {
            { GemPower.Normal,    GemSymbol       },
            { GemPower.Fire,      FireSymbol      },
            { GemPower.Star,      StarSymbol      },
            { GemPower.Hypercube, HypercubeSymbol },
            { GemPower.Supernova, SupernovaSymbol },
        });

        private static readonly ReadOnlyDictionary<GemKind, ConsoleColor> GemColors = new(new Dictionary<GemKind, ConsoleColor>
        {
            { GemKind.None,      ConsoleColor.Gray        },
            { GemKind.Amber,     ConsoleColor.DarkYellow  },
            { GemKind.Amethyst,  ConsoleColor.Magenta     },
            { GemKind.Diamond,   ConsoleColor.White       },
            { GemKind.Emerald,   ConsoleColor.Green       },
            { GemKind.Ruby,      ConsoleColor.Red         },
            { GemKind.Sapphire,  ConsoleColor.Blue        },
            { GemKind.Topaz,     ConsoleColor.Yellow      },
        });

        public static void Render(Game game)
        {
            List<Move> validMoves = game.GetValidMoves();
            for (int y = 0; y < Game.FieldHeight; y++)
            {
                for (int x = 0; x < Game.FieldWidth; x++)
                {
                    RenderGem(game.GetGemKindAt(x, y), game.GetGemPowerAt(x, y), new Position(x, y), validMoves);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Score: {game.Score}; Tick: {game.Tick}; Clusters: {game.GetClusters().Count}, Valid moves: {game.GetValidMoves().Count}");
        }

        private static void RenderGem(GemKind kind, GemPower power, Position position, List<Move> validMoves)
        {
            // Prepare
            ConsoleColor oldForeground = Console.ForegroundColor;
            ConsoleColor oldBackground = Console.BackgroundColor;

            // Set colors
            Console.ForegroundColor = GemColors[kind];
            Console.BackgroundColor = validMoves.Any(move => move.Start == position || move.End == position) ? ConsoleColor.DarkGray : ConsoleColor.Black;

            // Write
            Console.Write(kind == GemKind.None ? NoneSymbol : GemSymbols[power]);

            // Restore
            Console.ForegroundColor = oldForeground;
            Console.BackgroundColor = oldBackground;
        }
    }
}
