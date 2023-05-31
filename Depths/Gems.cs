namespace Depths
{
    public enum GemKind
    {
        None = 0, Hypercube = 999,
        Amber = 1, Amethyst = 2, Diamond = 3, Emerald = 4, Ruby = 5, Sapphire = 6, Topaz = 7
    }

    public enum GemPower
    {
        Normal, Fire, Star, Hypercube, Supernova
    }

    public struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public static class Gems
    {
        public const int GemWorth = 10;

        private static Random random = new();

        public static void SetSeed(int seed)
        {
            random = new Random(seed);
        }

        public static GemKind GetRandomGemKind()
        {
            Array values = Enum.GetValues(typeof(GemKind));
            return (GemKind)values.GetValue(random.Next(1, values.Length - 1))!;
        }
    }
}
