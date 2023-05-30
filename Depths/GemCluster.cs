namespace Depths
{
    public enum ClusterType
    {
        /// <summary>
        /// Three in a row
        /// </summary>
        Simple,

        /// <summary>
        /// Four in a row, forming a fire gem
        /// </summary>
        Four,

        /// <summary>
        /// Three in a row horizontally and three in a row vertically,
        /// forming a star gem
        /// </summary>
        L,

        /// <summary>
        /// Four in a row, crossed by another line of three or four
        /// </summary>
        LargeL,

        /// <summary>
        /// Five in a row, forming a hypercube
        /// </summary>
        Hyper,

        /// <summary>
        /// Six in a row, forming a supernova
        /// </summary>
        Supernova,
    }

    /// <summary>
    /// A number of matched gems.
    /// </summary>
    public class GemCluster
    {
        public List<(int, int)> Gems = new();
        public ClusterType ClusterType;

        public int Value
        {
            get
            {
                if (scoreTable.ContainsKey(ClusterType))
                {
                    return scoreTable[ClusterType];
                }
                else
                {
#if DEBUG
                    throw new NotImplementedException($"Unknown ClusterType: {ClusterType}");
#else
                    return 0;
#endif
                }
            }
        }

        private readonly Dictionary<ClusterType, int> scoreTable = new()
        {
            { ClusterType.Simple,    50  },
            { ClusterType.Four,      100 },
            { ClusterType.L,         150 },
            { ClusterType.LargeL,    200 },
            { ClusterType.Hyper,     250 },
            { ClusterType.Supernova, 500 }
        };
    }
}
