using Depths;
using System.Collections.ObjectModel;

namespace Gaze
{
    internal class Renderer
    {
        public bool HighlightValidMoves { get; set; } = false;
        public bool HighlightCustomMove { get; set; } = false;

        internal Move? HighlightedMove { get; set; }

        private const char GemSymbol       = '@';
        private const char FireSymbol      = 'F';
        private const char StarSymbol      = 'S';
        private const char HypercubeSymbol = 'H';
        private const char SupernovaSymbol = 'N';
        private const char NoneSymbol      = '.';

        private readonly ConsoleColor defaultBackgroundColor;
        private readonly ConsoleColor defaultForegroundColor;

        internal Renderer()
        {
            defaultBackgroundColor = Console.BackgroundColor;
            defaultForegroundColor = Console.ForegroundColor;
        }

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
            { GemKind.Hypercube, ConsoleColor.Gray        },
            { GemKind.Amber,     ConsoleColor.DarkYellow  },
            { GemKind.Amethyst,  ConsoleColor.Magenta     },
            { GemKind.Diamond,   ConsoleColor.White       },
            { GemKind.Emerald,   ConsoleColor.Green       },
            { GemKind.Ruby,      ConsoleColor.Red         },
            { GemKind.Sapphire,  ConsoleColor.Blue        },
            { GemKind.Topaz,     ConsoleColor.Yellow      },
        });

        internal void Render(Game game)
        {
            List<Move> validMoves = game.GetValidMoves();
            for (int y = 0; y < Field.Height; y++)
            {
                for (int x = 0; x < Field.Width; x++)
                {
                    RenderGem(game.Field.GetGemKindAt(x, y), game.Field.GetGemPowerAt(x, y), new Position(x, y), validMoves);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Score: {game.Score}; Combo: {game.Combo}; Clusters: {game.GetClusters().Count};");
            Console.WriteLine($"Tick:{game.Tick}; Valid moves: {game.GetValidMoves().Count}");
        }

        private void RenderGem(GemKind kind, GemPower power, Position position, List<Move> validMoves)
        {
            // Set colors
            Console.ForegroundColor = GemColors[kind];
            Console.BackgroundColor = HighlightValidMoves && validMoves.Any(move => move.Start == position || move.End == position) ?
                ConsoleColor.DarkGray : defaultBackgroundColor;
            Console.BackgroundColor = HighlightCustomMove && HighlightedMove.HasValue &&
                (HighlightedMove.Value.Start == position || HighlightedMove.Value.End == position) ?
                ConsoleColor.DarkGray : Console.BackgroundColor;

            // Write
            Console.Write(kind == GemKind.None ? NoneSymbol : GemSymbols[power]);

            // Restore
            bool isInMiddleOfMove = false;
            if (HighlightValidMoves)
            {
                foreach (Move move in validMoves)
                {
                    // Determine if we're in the middle of a move
                    if ((move.Start == position && move.End == new Position(position.x + 1, position.y)) ||
                        (move.End == position && move.Start == new Position(position.x + 1, position.y)))
                    {
                        isInMiddleOfMove = true;
                        break;
                    }
                }
            }
            else if (HighlightCustomMove && HighlightedMove.HasValue &&
                ((HighlightedMove.Value.Start == position && HighlightedMove.Value.End == new Position(position.x + 1, position.y)) ||
                (HighlightedMove.Value.End == position && HighlightedMove.Value.Start == new Position(position.x + 1, position.y))))
            {
                isInMiddleOfMove = true;
            }
            Console.ForegroundColor = defaultForegroundColor;
            Console.BackgroundColor = isInMiddleOfMove ? Console.BackgroundColor : defaultBackgroundColor;
        }
    }
}
