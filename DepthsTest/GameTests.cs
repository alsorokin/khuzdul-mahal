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

        /// <summary>
        /// Check: GetGemKindAt() returns the correct gem kind
        /// </summary>
        [TestMethod]
        public void GetGemKindAt_ReturnsCorrectGemKind()
        {
            Gems.SetSeed(12345);
            Game game = new();

            Assert.AreEqual(GemKind.Amber, game.GetGemKindAt(0, 0));
            Assert.AreEqual(GemKind.Amethyst, game.GetGemKindAt(1, 0));
            Assert.AreEqual(GemKind.Amethyst, game.GetGemKindAt(2, 0));
            Assert.AreEqual(GemKind.Ruby, game.GetGemKindAt(3, 0));
            Assert.AreEqual(GemKind.Emerald, game.GetGemKindAt(4, 0));
            Assert.AreEqual(GemKind.Ruby, game.GetGemKindAt(5, 0));
            Assert.AreEqual(GemKind.Amethyst, game.GetGemKindAt(6, 0));
            Assert.AreEqual(GemKind.Ruby, game.GetGemKindAt(7, 0));
        }

        /// <summary>
        /// Check: GetClusters() identifies the following types of clusters:
        /// * Simple
        /// * Four
        /// * LargeL
        /// * Supernova
        /// </summary>
        [TestMethod]
        public void GetClusters_IdentifiesClusters()
        {
            Gems.SetSeed(25337);
            Game game = new();

            List<GemCluster> clusters = game.GetClusters();

            Assert.AreEqual(4, clusters.Count);
            Assert.AreEqual(ClusterType.Simple, clusters[0].ClusterType);
            Assert.AreEqual(3, clusters[0].GemPositions.Count);
            Assert.AreEqual(ClusterType.Four, clusters[1].ClusterType);
            Assert.AreEqual(4, clusters[1].GemPositions.Count);
            Assert.AreEqual(ClusterType.LargeL, clusters[2].ClusterType);
            Assert.AreEqual(6, clusters[2].GemPositions.Count);
            Assert.AreEqual(ClusterType.Supernova, clusters[3].ClusterType);
            Assert.AreEqual(6, clusters[3].GemPositions.Count);
        }

        /// <summary>
        /// Check: GetClusters() identifies Hyper cluster
        /// </summary>
        [TestMethod]
        public void GetClusters_IdentifiesHyper()
        {
            Gems.SetSeed(18);
            Game game = new();

            List<GemCluster> clusters = game.GetClusters();

            Assert.AreEqual(5, clusters.Count);
            Assert.AreEqual(ClusterType.Hyper, clusters[2].ClusterType);
            Assert.AreEqual(5, clusters[2].GemPositions.Count);
        }


        /// <summary>
        /// Check: GetClusters() identifies L cluster
        /// </summary>
        [TestMethod]
        public void GetClusters_IdentifiesLargeL()
        {
            Gems.SetSeed(20);
            Game game = new();

            List<GemCluster> clusters = game.GetClusters();
            Assert.AreEqual(3, clusters.Count);
            Assert.AreEqual(ClusterType.L, clusters[0].ClusterType);
            Assert.AreEqual(5, clusters[0].GemPositions.Count);
        }

        /// <summary>
        /// Just some tooling to find desired game seeds
        /// </summary>
        // [TestMethod]
        public void Game_FindSeed()
        {
            int seed = 0;
            List<GemCluster> clusters;
            do
            {
                Gems.SetSeed(++seed);
                Game game = new();

                clusters = game.GetClusters();
            } while (!(clusters.Any(c => c.ClusterType == ClusterType.Hyper)));
            Assert.AreEqual(0, seed);
        }
    }
}
