namespace DepthsTest
{
    [TestClass]
    public class GameTests
    {
        [TestMethod]
        public void GetGemKindAt_ThrowsOutOfRangeException()
        {
            Game game = new();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => game.GetGemKindAt(Game.FieldWidth, 0), "Should throw ArgumentOutOfRangeException when x is too large.");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => game.GetGemKindAt(0, Game.FieldHeight), "Should throw ArgumentOutOfRangeException when y is too large.");
        }

        [TestMethod]
        public void GetClusters_IdentifiesAllClusters()
        {
            Gems.SetSeed(25337); // To make the test predictable
            Game game = new();

            List<GemCluster> clusters = game.GetClusters();

            Assert.AreEqual(4, clusters.Count);
            Assert.AreEqual(ClusterType.Simple, clusters[0].ClusterType);
            Assert.AreEqual(3, clusters[0].Points.Count);
            Assert.AreEqual(ClusterType.Four, clusters[1].ClusterType);
            Assert.AreEqual(4, clusters[1].Points.Count);
            Assert.AreEqual(ClusterType.L, clusters[2].ClusterType);
            Assert.AreEqual(6, clusters[2].Points.Count);
            Assert.AreEqual(ClusterType.Hyper, clusters[3].ClusterType);
            Assert.AreEqual(6, clusters[3].Points.Count);
        }
    }
}
