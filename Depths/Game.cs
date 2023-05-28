namespace Depths
{
    /// <summary>
    /// Main game logic
    /// </summary>
    public class Game
    {
        public const int FieldWidth = 32;
        public const int FieldHeight = 16;
        private readonly GemKind[,] gemKinds = new GemKind[FieldWidth, FieldHeight];
        private readonly GemPower[,] gemPowers = new GemPower[FieldWidth, FieldHeight];

        public long Score { get; private set; }

        public GemKind GetGemKindAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return this.gemKinds[x, y];
        }

        public GemPower GetGemPowerAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return this.gemPowers[x, y];
        }

        public Game()
        {
            this.Score = 0;

            // Fill the field with random gems
            // No checks for pre-existing matches yet
            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    this.gemKinds[x, y] = Gems.GetRandomGemKind();
                }
            }
            
            List<GemCluster> clusters = GetClusters();
            foreach (GemCluster cluster in clusters)
            {
                Console.WriteLine($"{cluster.ClusterType}: {cluster.Points.First().Item1}:{cluster.Points.First().Item2}");
            }
        }

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

            List<GemCluster> clusters = new List<GemCluster>();
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
                            if (width >= 5 || height >= 5)
                                type = ClusterType.Hyper;
                            else if (width >= 3 && height >= 3)
                                type = ClusterType.L;
                            else if (width == 4 || height == 4)
                                type = ClusterType.Four;
                            else if ((width == 3 && height == 1) || (width == 1 && height == 3))
                                type = ClusterType.Simple;

                            GemCluster cluster = new() { Points = clusterPoints, ClusterType = type };
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