namespace Depths
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Main game logic
    /// </summary>
    public class Game
    {
        public Field Field { get; } = new();
        private Move? currentMove = null;
        private Move? lastMove = null;
        private (long, List<GemCluster>) cachedClusters = (-1, new());
        private (long, List<Move>) cachedMoves = (-1, new());

        public long Score { get; private set; } = 0;
        public long Tick { get; private set; } = 0;
        public int Combo { get; private set; } = 0;

        public Game()
        {
            // Fill the field with random gems
            // No checks for pre-existing matches yet
            for (int x = 0; x < Field.Width; x++)
            {
                for (int y = 0; y < Field.Height; y++)
                {
                    Field.SetGemKindAt(x, y, Gems.GetRandomGemKind());
                }
            }
        }

        /// <summary>
        /// Checks if the specified move is valid and if so, executes it.
        /// </summary>
        public void MakeMove(Move move)
        {
            // Verify that the move results in a cluster
            if (IsMoveValid(move))
            {
                // Save the move for processing
                currentMove = move;
            }
        }

        private bool IsMoveValid(Move move)
        {
            // Move constructor already checks for bounds, only check for empty cell and that the move is resultant in a cluster
            if (Field.GetGemKindAt(move.Start) == GemKind.None || Field.GetGemKindAt(move.End) == GemKind.None)
            {
                // Can't move from or to an empty cell
                return false;
            }
            else
            {
                // Check if the move results in a cluster
                // This is done by checking if the move is in the list of moves that result in a cluster
                // TODO: Rewrite to a more efficient algorithm, for example by doing a DFS from the start and the end of the move
                List<Move> validMoves = GetValidMoves();
                return validMoves.Contains(move);
            }
        }

        /// <summary>
        /// Progresses the game by one step. This includes falling of gems and collecting clusters.
        /// </summary>
        public void Progress()
        {
            // Only collect clusters when gems are settled,
            // This is mainly for Gaze convenience
            bool gemsFell = Fall();
            if (gemsFell)
            {
                Tick++;
                return;
            }

            // Find and collect clusters
            List<GemCluster> clusters = GetClusters();
            CollectClusters(clusters);
            // Only increment game tick if clusters were found
            // This way we can reuse cached clusters and moves when nothing happens
            if (clusters.Count > 0)
            {
                Tick++;
                return;
            }

            // Execute outstanding move
            if (currentMove != null)
            {
                ExecuteCurrentMove();
                Tick++;
                return;
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
        public List<GemCluster> GetClusters(bool forceRecalculation = false)
        {
            // Return cached clusters if possible
            if (cachedClusters.Item1 == Tick && !forceRecalculation)
            {
                return cachedClusters.Item2;
            }

            bool[,] horizontalLines = new bool[Field.Width, Field.Height];
            bool[,] verticalLines = new bool[Field.Width, Field.Height];

            for (int x = 0; x < Field.Width; x++)
            {
                for (int y = 0; y < Field.Height; y++)
                {
                    List<Position> horizontalLine = FindLine(x, y, 1, 0);
                    foreach (Position point in horizontalLine)
                    {
                        horizontalLines[point.x, point.y] = true;
                    }

                    List<Position> verticalLine = FindLine(x, y, 0, 1);
                    foreach (Position point in verticalLine)
                    {
                        verticalLines[point.x, point.y] = true;
                    }
                }
            }

            List<GemCluster> clusters = new();
            bool[,] visited = new bool[Field.Width, Field.Height];

            for (int x = 0; x < Field.Width; x++)
            {
                for (int y = 0; y < Field.Height; y++)
                {
                    if (!visited[x, y] && (horizontalLines[x, y] || verticalLines[x, y]))
                    {
                        // Find all gems in this cluster
                        List<Position> clusterPoints = new();
                        Dfs(x, y, visited, clusterPoints, Field.GetGemKindAt(x, y), horizontalLines, verticalLines);

                        if (clusterPoints.Count >= 3)
                        {
                            // Determine the cluster type
                            int minX = clusterPoints.Min(gem => gem.x);
                            int maxX = clusterPoints.Max(gem => gem.x);
                            int minY = clusterPoints.Min(gem => gem.y);
                            int maxY = clusterPoints.Max(gem => gem.y);

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

                            GemCluster cluster = new() { GemPositions = clusterPoints, ClusterType = type };
                            clusters.Add(cluster);
                        }
                    }
                }
            }

            if (!forceRecalculation)
            {
                cachedClusters = (Tick, clusters);
            }
            return clusters;
        }

        /// <summary>
        /// Computes and returns a list of all possible <see cref="Move">moves</see> that can be made on the field.
        /// A move is considered possible if it leads to the formation of a cluster.
        /// </summary>
        /// <returns>
        /// A list of all possible <see cref="Move">moves</see>.
        /// Empty list if there are none or there are some clusters formed already.
        /// </returns>
        public List<Move> GetValidMoves()
        {
            // Retirn cached moves if possible
            if (cachedMoves.Item1 == Tick)
            {
                return cachedMoves.Item2;
            }

            cachedMoves = (Tick, new());
            List<GemCluster> clusters = GetClusters();
            // If there are clusters formed already, no moves are possible until the clusters are collected
            if (clusters.Count > 0)
            {
                return cachedMoves.Item2;
            }
            // If there are empty cells, no moves are possible until they are filled
            if (Field.HasEmptyCells)
            {
                return cachedMoves.Item2;
            }

            // Temporarily swap gems, then call GetClusters to see if any clusters are formed
            // If so, then this move is possible
            for (int x = 0; x < Field.Width; x++)
            {
                for (int y = 0; y < Field.Height; y++)
                {
                    List<Move> potentialMoves = new()
                    {
                        new Move (new Position(x, y), dx: 1), // Right move
                        new Move (new Position(x, y), dy: 1), // Down move
                    };

                    foreach (Move move in potentialMoves)
                    {
                        int newX = move.Start.x + move.DX;
                        int newY = move.Start.y + move.DY;
                        if (newX < 0 || newX >= Field.Width || newY < 0 || newY >= Field.Height)
                        {
                            continue; // Skip invalid moves
                        }

                        Field.SwapGems(new Position(newX, newY), new Position(x, y));
                        if (GetClusters(true).Count > 0)
                        {
                            cachedMoves.Item2.Add(move); // This move leads to at least one cluster
                        }
                        Field.SwapGems(new Position(newX, newY), new Position(x, y));
                    }
                }
            }

            return cachedMoves.Item2;
        }

        private bool Fall()
        {
            bool fell = false;
            // Fill empty space
            for (int x = Field.Width - 1; x >= 0; x--)
            {
                for (int y = Field.Height - 1; y >= 0; y--)
                {
                    if (Field.GetGemKindAt(x, y) == GemKind.None)
                    {
                        fell = true;
                        // Go from bottom to top, swapping any non-empty cells with last known empty one
                        int lastEmptyY = y;
                        for (int yy = y - 1; yy >= 0; yy--)
                        {
                            GemKind kind = Field.GetGemKindAt(x, yy);
                            GemPower power = Field.GetGemPowerAt(x, yy);
                            if (kind == GemKind.None)
                            {
                                continue;
                            }
                            else
                            {
                                Field.SetGemKindAt(x, lastEmptyY, kind);
                                Field.SetGemPowerAt(x, lastEmptyY, power);
                                Field.SetGemKindAt(x, yy, GemKind.None);
                                Field.SetGemPowerAt(x, yy, GemPower.Normal);
                                lastEmptyY--;
                            }
                        }
                        // Fill all empty space that's left with random gems
                        for (int yy = 0; yy <= lastEmptyY; yy++)
                        {
                            Field.SetGemKindAt(x, yy, Gems.GetRandomGemKind());
                        }
                        // Since all work is done in one go, skip checking the remainder of the column
                        break;
                    }
                }
            }
            return fell;
        }

        private void CollectClusters(List<GemCluster> clusters)
        {
            foreach (GemCluster cluster in clusters)
            {
                GemKind clusterGemKind = Field.GetGemKindAt(cluster.GemPositions.First());
                Debug.Assert(clusterGemKind != GemKind.None, "Cluster gem kind must not be None");
                Debug.Assert(cluster.GemPositions.Count > 2, "Cluster must have at least three gems in it");

                Combo++;
                Score += cluster.WorthBonus * Combo;
                foreach (Position gem in cluster.GemPositions)
                {
                    CollectGem(gem.x, gem.y, Field.GetGemKindAt(cluster.GemPositions.First()));
                }

                // Create new power gems for non-simple clusters
                // If the cluster is a part of a move, spawn the power gem at the start or end of the move
                // Otherwise, spawn it in the middle of the cluster
                if (cluster.ClusterType == ClusterType.Simple)
                {
                    continue;
                }
                Position gemSpawnPoint = cluster.GemPositions.Any(p => p == lastMove?.Start || p == lastMove?.End) ?
                    cluster.GemPositions.FirstOrDefault(p => p == lastMove?.Start || p == lastMove?.End) :
                    cluster.GemPositions[(int)Math.Round((float)cluster.GemPositions.Count / 2.0, MidpointRounding.ToPositiveInfinity)];

                Field.SetGemKindAt(gemSpawnPoint, cluster.ClusterType == ClusterType.Hyper ? GemKind.Hypercube : clusterGemKind);
                Field.SetGemPowerAt(gemSpawnPoint, cluster.ClusterType switch
                {
                    ClusterType.Four => GemPower.Fire,
                    ClusterType.L => GemPower.Star, // TODO: Gem should spawn at the intersection
                    ClusterType.LargeL => GemPower.Star, // TODO: This should spawn a star gem and a fire gem
                    ClusterType.Hyper => GemPower.Hypercube,
                    ClusterType.Supernova => GemPower.Supernova,
                    _ => throw new Exception("Invalid cluster type")
                });
            }
        }

        private void CollectGem(int x, int y, GemKind kind)
        {
            if (Field.GetGemKindAt(x, y) == GemKind.None)
            {
                return;
            }
            GemKind thisGemKind = Field.GetGemKindAt(x, y);
            // Collect the gem
            Field.SetGemKindAt(x, y, GemKind.None);
            Score += Gems.GemWorth * Combo;

            // If the gem had a power, activate it
            switch (Field.GetGemPowerAt(x, y))
            {
                case GemPower.Fire:
                    // handle fire power: 3x3 explosion
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx < 0 || xx >= Field.Width || yy < 0 || yy >= Field.Height)
                                continue;
                            CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    break;
                case GemPower.Star:
                    // handle star power: collect all gems in one vertical and one horizontal line
                    for (int xx = 0; xx < Field.Width; xx++)
                    {
                        CollectGem(xx, y, thisGemKind);
                    }
                    for (int yy = 0; yy < Field.Height; yy++)
                    {
                        CollectGem(x, yy, thisGemKind);
                    }
                    break;
                case GemPower.Hypercube:
                    // handle hypercube power: collect all gems of the same kind
                    for (int xx = 0; xx < Field.Width; xx++)
                    {
                        for (int yy = 0; yy < Field.Height; yy++)
                        {
                            CollectGem(xx, yy, kind);
                        }
                    }
                    break;
                case GemPower.Supernova:
                    // handle supernova power: collect all gems in three horizontal and three vertical lines
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = 0; yy < Field.Height; yy++)
                        {
                            CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    for (int yy = y - 1; yy <= y + 1; yy++)
                    {
                        for (int xx = 0; xx < Field.Width; xx++)
                        {
                            CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    break;
                default:
                    break;
            }
            Field.SetGemPowerAt(x, y, GemPower.Normal);
        }

        private void ExecuteCurrentMove()
        {
            Debug.Assert(currentMove.HasValue, "There must be a current move to execute");
            Combo = 0;
            // TODO: Handle hypercube and double hypercube
            Field.SwapGems(currentMove.Value.Start, currentMove.Value.End);
            lastMove = currentMove;
            currentMove = null;
        }

        private void Dfs(int x, int y, bool[,] visited, List<Position> cluster, GemKind kind, bool[,] horizontalLines, bool[,] verticalLines)
        {
            if (x < 0 || x >= Field.Width || y < 0 || y >= Field.Height)
                return; // Out of bounds

            if (visited[x, y] || Field.GetGemKindAt(x, y) != kind)
                return; // Either already visited or not the right kind

            visited[x, y] = true; // Mark as visited
            cluster.Add(new Position(x, y)); // Add to the cluster

            // Visit all neighbors in a line of matched gems
            if (x > 0 && horizontalLines[x - 1, y])
                Dfs(x - 1, y, visited, cluster, kind, horizontalLines, verticalLines); // Left
            if (x < Field.Width - 1 && horizontalLines[x, y])
                Dfs(x + 1, y, visited, cluster, kind, horizontalLines, verticalLines); // Right
            if (y > 0 && verticalLines[x, y - 1])
                Dfs(x, y - 1, visited, cluster, kind, horizontalLines, verticalLines); // Up
            if (y < Field.Height - 1 && verticalLines[x, y])
                Dfs(x, y + 1, visited, cluster, kind, horizontalLines, verticalLines); // Down
        }

        private List<Position> FindLine(int x, int y, int dx, int dy)
        {
            List<Position> line = new();
            GemKind kind = Field.GetGemKindAt(x, y);
            // Hypercubes and non-gems don't form lines
            if (kind == GemKind.None || kind == GemKind.Hypercube)
                return line;

            while (x >= 0 && x < Field.Width && y >= 0 && y < Field.Height && Field.GetGemKindAt(x, y) == kind)
            {
                line.Add(new Position(x, y));
                x += dx;
                y += dy;
            }

            return line.Count >= 3 ? line : new List<Position>();
        }
    }
}