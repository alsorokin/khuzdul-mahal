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
        /// Five in a row, forming a hypercube
        /// </summary>
        Hyper
    }

    /// <summary>
    /// A number of matched gems.
    /// </summary>
    public class GemCluster
    {
        public List<(int, int)> Points = new();
        public ClusterType ClusterType;
    }
}
