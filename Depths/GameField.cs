using System.Diagnostics;

namespace Depths
{
    public class GameField
    {
        public const int Width = 8;
        public const int Height = 8;
        private readonly GemKind[,] gemKinds = new GemKind[Width, Height];
        private readonly GemPower[,] gemPowers = new GemPower[Width, Height];

        public bool HasEmptyCells
        {
            get
            {
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (gemKinds[x, y] == GemKind.None)
                            return true;
                return false;
            }
        }

        public GemKind GetGemKindAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return gemKinds[x, y];
        }

        public GemKind GetGemKindAt(Position position) =>
            GetGemKindAt(position.X, position.Y);

        public GemPower GetGemPowerAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return gemPowers[x, y];
        }

        public GemPower GetGemPowerAt(Position position) =>
            GetGemPowerAt(position.X, position.Y);

        public void SetGemKindAt(Position gemSpawnPoint, GemKind gemKind) =>
            SetGemKindAt(gemSpawnPoint.X, gemSpawnPoint.Y, gemKind);

        public void SetGemKindAt(int x, int y, GemKind kind)
        {
            VerifyXYBounds(x, y);
            gemKinds[x, y] = kind;
        }

        public void SetGemPowerAt(Position position, GemPower power) =>
            SetGemPowerAt(position.X, position.Y, power);

        public void SetGemPowerAt(int x, int y, GemPower power)
        {
            VerifyXYBounds(x, y);
            gemPowers[x, y] = power;
        }

        public void SetGemKindAndPowerAt(int x, int y, GemKind kind, GemPower power)
        {
            VerifyXYBounds(x, y);
            gemKinds[x, y] = kind;
            gemPowers[x, y] = power;
        }

        public void SwapGems(Position pos1, Position pos2)
        {
            VerifyXYBounds(pos1);
            VerifyXYBounds(pos2);
            (gemKinds[pos2.X, pos2.Y], gemKinds[pos1.X, pos1.Y]) = (gemKinds[pos1.X, pos1.Y], gemKinds[pos2.X, pos2.Y]);
            (gemPowers[pos2.X, pos2.Y], gemPowers[pos1.X, pos1.Y]) = (gemPowers[pos1.X, pos1.Y], gemPowers[pos2.X, pos2.Y]);
        }

        /// <summary>
        /// Executes action for each gem on the game field.
        /// </summary>
        public void ForEachGem(Action<Position, GemKind, GemPower> action)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    action(new Position(x, y), gemKinds[x, y], gemPowers[x, y]);
        }

        private static void VerifyXYBounds(int x, int y)
        {
            Debug.Assert(x >= 0 && x < Width, "x should be in bounds of game field.");
            Debug.Assert(y >= 0 && y < Height, "y should be in bounds of game field.");
        }

        private static void VerifyXYBounds(Position position)
        {
            VerifyXYBounds(position.X, position.Y);
        }
    }
}
