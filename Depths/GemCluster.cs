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

        public int WorthBonus
        {
            get
            {
                if (bonuses.ContainsKey(ClusterType))
                {
                    return bonuses[ClusterType];
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

        private readonly Dictionary<ClusterType, int> bonuses = new()
        {
            { ClusterType.Simple,    0   },
            { ClusterType.Four,      10  },
            { ClusterType.L,         30  },
            { ClusterType.LargeL,    50  },
            { ClusterType.Hyper,     100 },
            { ClusterType.Supernova, 250 }
        };
    }
}
