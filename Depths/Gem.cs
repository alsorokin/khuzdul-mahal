namespace Depths
{
    public enum GemKind
    {
        Amber, Amethyst, Diamond, Emerald, Ruby, Sapphire, Topaz
    }

    public enum GemPower
    {
        Normal, Fire, Star, Hypercube
    }

    public static class Gems
    {
        private static readonly Random random = new();

        public static GemKind GetRandomGemKind()
        {
            Array values = Enum.GetValues(typeof(GemKind));
            return (GemKind)values.GetValue(random.Next(values.Length))!;
        }

    }
}
