namespace Depths
{
    public enum GemKind
    {
        None = 0,
        Amber, Amethyst, Diamond, Emerald, Ruby, Sapphire, Topaz
    }

    public enum GemPower
    {
        Normal, Fire, Star, Hypercube
    }

    public static class Gems
    {
        private static Random random = new();

        public static void SetSeed(int seed)
        {
            random = new Random(seed);
        }

        public static GemKind GetRandomGemKind()
        {
            Array values = Enum.GetValues(typeof(GemKind));
            return (GemKind)values.GetValue(random.Next(1, values.Length))!;
        }
    }
}
