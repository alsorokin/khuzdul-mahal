﻿using System.Diagnostics;

namespace Depths
{
    public class Field
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
            GetGemKindAt(position.x, position.y);

        public GemPower GetGemPowerAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return gemPowers[x, y];
        }

        public GemPower GetGemPowerAt(Position position) =>
            GetGemPowerAt(position.x, position.y);

        public void SetGemKindAt(Position gemSpawnPoint, GemKind gemKind) =>
            SetGemKindAt(gemSpawnPoint.x, gemSpawnPoint.y, gemKind);

        public void SetGemKindAt(int x, int y, GemKind kind)
        {
            VerifyXYBounds(x, y);
            gemKinds[x, y] = kind;
        }

        public void SetGemPowerAt(Position position, GemPower power) =>
            SetGemPowerAt(position.x, position.y, power);

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
            (gemKinds[pos2.x, pos2.y], gemKinds[pos1.x, pos1.y]) = (gemKinds[pos1.x, pos1.y], gemKinds[pos2.x, pos2.y]);
            (gemPowers[pos2.x, pos2.y], gemPowers[pos1.x, pos1.y]) = (gemPowers[pos1.x, pos1.y], gemPowers[pos2.x, pos2.y]);
        }

        private static void VerifyXYBounds(int x, int y)
        {
            Debug.Assert(x >= 0 && x < Width, "x should be in bounds of game field.");
            Debug.Assert(y >= 0 && y < Height, "y should be in bounds of game field.");
        }

        private static void VerifyXYBounds(Position position)
        {
            VerifyXYBounds(position.x, position.y);
        }
    }
}
