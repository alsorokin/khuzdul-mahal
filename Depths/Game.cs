namespace Depths
{
    /// <summary>
    /// Main game logic
    /// </summary>
    public class Game
    {
        public const int FieldWidth = 8;
        public const int FieldHeight = 8;
        private readonly GemKind[,] gemKinds = new GemKind[FieldWidth, FieldHeight];
        private readonly GemPower[,] gemPowers = new GemPower[FieldWidth, FieldHeight];

        public long Score { get; private set; }

        public GemKind GetGemKindAt(int x, int y)
        {
            if (x < 0 || x >= FieldWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"x should be in range of 0-{FieldWidth - 1}");
            }
            if (y < 0 || y >= FieldHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y should be in range of 0-{FieldHeight - 1}");
            }

            return this.gemKinds[x, y];
        }

        public GemPower GetGemPowerAt(int x, int y)
        {
            if (x < 0 || x >= FieldWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"x should be in range of 0-{FieldWidth - 1}");
            }
            if (y < 0 || y >= FieldHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y should be in range of 0-{FieldHeight - 1}");
            }

            return this.gemPowers[x, y];
        }

        public Game()
        {
            this.Score = 0;

            // Fill the field with random gems
            // No checks for pre-existing matches yet
            for (int i = 0; i < FieldWidth; i++)
            {
                for (int j = 0; j < FieldHeight; j++)
                {
                    this.gemKinds[i, j] = Gems.GetRandomGemKind();
                }
            }
        }
    }
}