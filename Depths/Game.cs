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

        /// <summary>
        /// Retrieves the type of gem at the specified position in the game field.
        /// </summary>
        /// <param name="x">The x-coordinate of the gem position.</param>
        /// <param name="y">The y-coordinate of the gem position.</param>
        /// <returns>
        /// The type (<see cref="GemKind"/>) of the gem at the specified position.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided x or y coordinates are out of the game field bounds.
        /// </exception>
        public GemKind GetGemKindAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return this.gemKinds[x, y];
        }

        /// <summary>
        /// Retrieves the power of the gem at the specified position in the game field.
        /// </summary>
        /// <param name="x">The x-coordinate of the gem position.</param>
        /// <param name="y">The y-coordinate of the gem position.</param>
        /// <returns>
        /// The power (<see cref="GemPower"/>) of the gem at the specified position.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the provided x or y coordinates are out of the game field bounds.
        /// </exception>
        public GemPower GetGemPowerAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return this.gemPowers[x, y];
        }

        public Game()
        {
            // Fill the field with random gems
            // No checks for pre-existing matches yet
            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    this.gemKinds[x, y] = Gems.GetRandomGemKind();
                }
            }
        }

        /// <summary>
        /// Progresses the game by one step. This includes falling of gems and collecting clusters.
        /// </summary>
        public void Progress()
        {
            bool gemsFell = false;
            // Fill empty space
            for (int x = FieldWidth - 1; x >= 0; x--)
            {
                for (int y = FieldHeight - 1; y >= 0; y--)
                {
                    if (this.gemKinds[x, y] == GemKind.None)
                    {
                        gemsFell = true;
                        // Go from bottom to top, swapping any non-empty cells with last known empty one
                        int lastEmptyY = y;
                        for (int yy = y - 1; yy >= 0; yy--)
                        {
                            if (this.gemKinds[x, yy] == GemKind.None)
                            {
                                continue;
                            }
                            else
                            {
                                this.gemKinds[x, lastEmptyY] = this.gemKinds[x, yy];
                                this.gemPowers[x, lastEmptyY] = this.gemPowers[x, yy];
                                this.gemKinds[x, yy] = GemKind.None;
                                this.gemPowers[x, yy] = GemPower.Normal;
                                lastEmptyY--;
                            }
                        }
                        // Fill all empty space that's left with random gems
                        for (int yy = 0; yy <= lastEmptyY; yy++)
                        {
                            this.gemKinds[x, yy] = Gems.GetRandomGemKind();
                        }
                        // Since all work is done in one go, skip checking the remainder of the column
                        break;
                    }
                }
            }

            // Only collect clusters when gems are settled,
            // This is mainly for Gaze convenience
            if (gemsFell) return;

            // Find and collect clusters
            List<GemCluster> clusters = GetClusters();
            foreach (GemCluster cluster in clusters)
            {
                Score += cluster.Value;
                foreach ((int, int) gem in cluster.Gems)
                {
                    gemKinds[gem.Item1, gem.Item2] = GemKind.None;
                }
                // TODO: Explode power gems
                // TODO: Create new power gems for non-simple clusters
            }
        }

        /// <summary>
        /// Scans the game field to detect and return all clusters of gems. A cluster is a group of
        /// three or more gems of the same kind that form a straight line or multiple intersecting lines.
        /// This function also determines the type of each cluster (Simple, Four, L, Hyper) based on the
        /// size and shape of the cluster.
        /// </summary>
        /// <returns>
        /// A list of all clusters found on the field. Each cluster is represented by a GemCluster object
        /// that includes the coordinates of the gems in the cluster and the type of the cluster.
        /// </returns>
        public List<GemCluster> GetClusters()
        {
            bool[,] horizontalLines = new bool[FieldWidth, FieldHeight];
            bool[,] verticalLines = new bool[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    List<(int, int)> horizontalLine = FindLine(x, y, 1, 0);
                    foreach ((int, int) point in horizontalLine)
                    {
                        horizontalLines[point.Item1, point.Item2] = true;
                    }

                    List<(int, int)> verticalLine = FindLine(x, y, 0, 1);
                    foreach ((int, int) point in verticalLine)
                    {
                        verticalLines[point.Item1, point.Item2] = true;
                    }
                }
            }

            List<GemCluster> clusters = new();
            bool[,] visited = new bool[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    if (!visited[x, y] && (horizontalLines[x, y] || verticalLines[x, y]))
                    {
                        // Find all gems in this cluster
                        List<(int, int)> clusterPoints = new();
                        Dfs(x, y, visited, clusterPoints, gemKinds[x, y], horizontalLines, verticalLines);

                        if (clusterPoints.Count >= 3)
                        {
                            // Determine the cluster type
                            int minX = clusterPoints.Min(gem => gem.Item1);
                            int maxX = clusterPoints.Max(gem => gem.Item1);
                            int minY = clusterPoints.Min(gem => gem.Item2);
                            int maxY = clusterPoints.Max(gem => gem.Item2);

                            int width = maxX - minX + 1;
                            int height = maxY - minY + 1;

                            ClusterType type = ClusterType.Simple;
                            if (width >= 6 || height >= 6)
                                type = ClusterType.Supernova;
                            else if (width >= 5 || height >= 5)
                                type = ClusterType.Hyper;
                            else if ((width >= 4 && height >= 3) || (width >= 3 && height >= 4))
                                type = ClusterType.LargeL;
                            else if (width == 3 && height == 3)
                                type = ClusterType.L;
                            else if (width == 4 || height == 4)
                                type = ClusterType.Four;
                            else if ((width == 3 && height == 1) || (width == 1 && height == 3))
                                type = ClusterType.Simple;

                            GemCluster cluster = new() { Gems = clusterPoints, ClusterType = type };
                            clusters.Add(cluster);
                        }
                    }
                }
            }

            return clusters;
        }

        private void Dfs(int x, int y, bool[,] visited, List<(int, int)> cluster, GemKind kind, bool[,] horizontalLines, bool[,] verticalLines)
        {
            if (x < 0 || x >= FieldWidth || y < 0 || y >= FieldHeight)
                return; // Out of bounds

            if (visited[x, y] || gemKinds[x, y] != kind)
                return; // Either already visited or not the right kind

            visited[x, y] = true; // Mark as visited
            cluster.Add((x, y)); // Add to the cluster

            // Visit all neighbors in a line of matched gems
            if (x > 0 && horizontalLines[x - 1, y])
                Dfs(x - 1, y, visited, cluster, kind, horizontalLines, verticalLines); // Left
            if (x < FieldWidth - 1 && horizontalLines[x, y])
                Dfs(x + 1, y, visited, cluster, kind, horizontalLines, verticalLines); // Right
            if (y > 0 && verticalLines[x, y - 1])
                Dfs(x, y - 1, visited, cluster, kind, horizontalLines, verticalLines); // Up
            if (y < FieldHeight - 1 && verticalLines[x, y])
                Dfs(x, y + 1, visited, cluster, kind, horizontalLines, verticalLines); // Down
        }

        private void VerifyXYBounds(int x, int y)
        {
            if (x < 0 || x >= FieldWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"x should be in range of 0-{FieldWidth - 1}");
            }
            if (y < 0 || y >= FieldHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y should be in range of 0-{FieldHeight - 1}");
            }
        }

        private List<(int, int)> FindLine(int x, int y, int dx, int dy)
        {
            List<(int, int)> line = new();
            GemKind kind = gemKinds[x, y];

            while (x >= 0 && x < FieldWidth && y >= 0 && y < FieldHeight && gemKinds[x, y] == kind)
            {
                line.Add((x, y));
                x += dx;
                y += dy;
            }

            return line.Count >= 3 ? line : new List<(int, int)>();
        }
    }
}