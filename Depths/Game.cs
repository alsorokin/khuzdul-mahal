namespace Depths
{
    using System.Collections.Generic;

    /// <summary>
    /// Main game logic
    /// </summary>
    public class Game
    {
        public const int FieldWidth = 8;
        public const int FieldHeight = 8;
        private readonly GemKind[,] gemKinds = new GemKind[FieldWidth, FieldHeight];
        private readonly GemPower[,] gemPowers = new GemPower[FieldWidth, FieldHeight];
        private Move? currentMove = null;
        private long gameTick = 0;
        private (long, List<GemCluster>) cachedClusters = (-1, new());
        private (long, List<Move>) cachedMoves = (-1, new());

        public long Score { get; private set; }
        public long Tick => gameTick;

        /// <summary>
        /// Retrieves the type of gem at the specified position in the game field.
        /// </summary>
        /// <param name="x">The x-coordinate of the gem position.</param>
        /// <param name="y">The y-coordinate of the gem position.</param>
        /// <returns>The type (<see cref="GemKind"/>) of the gem at the specified position.</returns>
        public GemKind GetGemKindAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return gemKinds[x, y];
        }

        /// <summary>
        /// Retrieves the type of gem at the specified position in the game field.
        /// </summary>
        /// <param name="position">The position of the gem.</param>
        /// <returns>The type (<see cref="GemKind"/>) of the gem at the specified position.</returns>
        public GemKind GetGemKindAt(Position position)
        {
            return GetGemKindAt(position.x, position.y);
        }

        /// <summary>
        /// Retrieves the power of the gem at the specified position in the game field.
        /// </summary>
        /// <param name="x">The x-coordinate of the gem position.</param>
        /// <param name="y">The y-coordinate of the gem position.</param>
        /// <returns>The power (<see cref="GemPower"/>) of the gem at the specified position.</returns>
        public GemPower GetGemPowerAt(int x, int y)
        {
            VerifyXYBounds(x, y);
            return gemPowers[x, y];
        }

        /// <summary>
        /// Retrieves the power of the gem at the specified position in the game field.
        /// </summary>
        /// <param name="position">The position of the gem.</param>
        /// <returns>The power (<see cref="GemPower"/>) of the gem at the specified position.</returns>
        public GemPower GetGemPowerAt(Position position)
        {
            return GetGemPowerAt(position.x, position.y);
        }

        public Game()
        {
            // Fill the field with random gems
            // No checks for pre-existing matches yet
            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    gemKinds[x, y] = Gems.GetRandomGemKind();
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
            if (gemKinds[move.Start.x, move.Start.y] == GemKind.None || gemKinds[move.End.x, move.End.y] == GemKind.None)
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
                gameTick++;
                return;
            }

            // Find and collect clusters
            List<GemCluster> clusters = GetClusters();
            CollectClusters(clusters);
            // Only increment game tick if clusters were found
            // This way we can reuse cached clusters and moves when nothing happens
            if (clusters.Count > 0)
            {
                gameTick++;
                return;
            }

            // Execute outstanding move
            if (currentMove != null)
            {
                SwapGems(currentMove.Value.Start, currentMove.Value.End);
                currentMove = null;
                gameTick++;
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
            if (cachedClusters.Item1 == gameTick && !forceRecalculation)
            {
                return cachedClusters.Item2;
            }

            bool[,] horizontalLines = new bool[FieldWidth, FieldHeight];
            bool[,] verticalLines = new bool[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
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
            bool[,] visited = new bool[FieldWidth, FieldHeight];

            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
                {
                    if (!visited[x, y] && (horizontalLines[x, y] || verticalLines[x, y]))
                    {
                        // Find all gems in this cluster
                        List<Position> clusterPoints = new();
                        Dfs(x, y, visited, clusterPoints, gemKinds[x, y], horizontalLines, verticalLines);

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

                            GemCluster cluster = new() { Gems = clusterPoints, ClusterType = type };
                            clusters.Add(cluster);
                        }
                    }
                }
            }

            if (!forceRecalculation)
            {
                cachedClusters = (gameTick, clusters);
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
            if (cachedMoves.Item1 == gameTick)
            {
                return cachedMoves.Item2;
            }

            cachedMoves = (gameTick, new());
            List<GemCluster> clusters = GetClusters();
            // If there are clusters formed already, no moves are possible until the clusters are collected
            if (clusters.Count > 0)
                return cachedMoves.Item2;
            // If there are empty cells, no moves are possible until they are filled
            if (gemKinds.Cast<GemKind>().Any((kind) => kind == GemKind.None))
                return cachedMoves.Item2;

            // Temporarily swap gems, then call GetClusters to see if any clusters are formed
            // If so, then this move is possible
            for (int x = 0; x < FieldWidth; x++)
            {
                for (int y = 0; y < FieldHeight; y++)
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
                        if (newX < 0 || newX >= FieldWidth || newY < 0 || newY >= FieldHeight)
                            continue; // Skip invalid moves

                        // Swap gems
                        SwapGems(new Position(newX, newY), new Position(x, y));
                        if (GetClusters(true).Count > 0)
                            cachedMoves.Item2.Add(move); // This move leads to at least one cluster

                        // Swap back
                        SwapGems(new Position(newX, newY), new Position(x, y));
                    }
                }
            }

            return cachedMoves.Item2;
        }

        private void SwapGems(Position pos1, Position pos2)
        {
            (gemKinds[pos2.x, pos2.y], gemKinds[pos1.x, pos1.y]) = (gemKinds[pos1.x, pos1.y], gemKinds[pos2.x, pos2.y]);
        }

        private bool Fall()
        {
            bool fell = false;
            // Fill empty space
            for (int x = FieldWidth - 1; x >= 0; x--)
            {
                for (int y = FieldHeight - 1; y >= 0; y--)
                {
                    if (gemKinds[x, y] == GemKind.None)
                    {
                        fell = true;
                        // Go from bottom to top, swapping any non-empty cells with last known empty one
                        int lastEmptyY = y;
                        for (int yy = y - 1; yy >= 0; yy--)
                        {
                            if (gemKinds[x, yy] == GemKind.None)
                            {
                                continue;
                            }
                            else
                            {
                                gemKinds[x, lastEmptyY] = gemKinds[x, yy];
                                gemPowers[x, lastEmptyY] = gemPowers[x, yy];
                                gemKinds[x, yy] = GemKind.None;
                                gemPowers[x, yy] = GemPower.Normal;
                                lastEmptyY--;
                            }
                        }
                        // Fill all empty space that's left with random gems
                        for (int yy = 0; yy <= lastEmptyY; yy++)
                        {
                            gemKinds[x, yy] = Gems.GetRandomGemKind();
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
                Score += cluster.WorthBonus;
                foreach (Position gem in cluster.Gems)
                {
                    CollectGem(gem.x, gem.y, gemKinds[cluster.Gems.First().x, cluster.Gems.First().y]);
                }
                // TODO: Create new power gems for non-simple clusters
            }
        }

        private void CollectGem(int x, int y, GemKind kind)
        {
            GemKind thisGemKind = gemKinds[x, y];
            // Collect the gem
            gemKinds[x, y] = GemKind.None;
            Score += Gems.GemWorth;

            // If the gem had a power, activate it
            switch (gemPowers[x, y])
            {
                case GemPower.Fire:
                    // handle fire power: 3x3 explosion
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx < 0 || xx >= FieldWidth || yy < 0 || yy >= FieldHeight)
                                continue;
                            if (gemKinds[xx, yy] != GemKind.None)
                                CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    break;
                case GemPower.Star:
                    // handle star power: collect all gems horizontally and vertically
                    for (int xx = 0; xx < FieldWidth; xx++)
                    {
                        for (int yy = 0; yy < FieldHeight; yy++)
                        {
                            if (gemKinds[xx, yy] != GemKind.None)
                                CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    break;
                case GemPower.Hypercube:
                    // handle hypercube power: collect all gems of the same kind
                    // TODO: correctly handle when a hypercube is matched with another hypercube (collect the whole field)
                    for (int xx = 0; xx < FieldWidth; xx++)
                    {
                        for (int yy = 0; yy < FieldHeight; yy++)
                        {
                            if (gemKinds[xx, yy] == kind)
                                CollectGem(xx, yy, kind);
                        }
                    }
                    break;
                case GemPower.Supernova:
                    // handle supernova power: collect all gems in three horizontal and three vertical lines
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx < 0 || xx >= FieldWidth || yy < 0 || yy >= FieldHeight)
                                continue;
                            if (gemKinds[xx, yy] != GemKind.None)
                                CollectGem(xx, yy, thisGemKind);
                        }
                    }
                    break;
                default:
                    break;
            }
            gemPowers[x, y] = GemPower.Normal;
        }

        private void Dfs(int x, int y, bool[,] visited, List<Position> cluster, GemKind kind, bool[,] horizontalLines, bool[,] verticalLines)
        {
            if (x < 0 || x >= FieldWidth || y < 0 || y >= FieldHeight)
                return; // Out of bounds

            if (visited[x, y] || gemKinds[x, y] != kind)
                return; // Either already visited or not the right kind

            visited[x, y] = true; // Mark as visited
            cluster.Add(new Position(x, y)); // Add to the cluster

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

        private List<Position> FindLine(int x, int y, int dx, int dy)
        {
            List<Position> line = new();
            GemKind kind = gemKinds[x, y];
            // Hypercubes and non-gems don't form lines
            if (kind == GemKind.None || kind == GemKind.Hypercube)
                return line;

            while (x >= 0 && x < FieldWidth && y >= 0 && y < FieldHeight && gemKinds[x, y] == kind)
            {
                line.Add(new Position(x, y));
                x += dx;
                y += dy;
            }

            return line.Count >= 3 ? line : new List<Position>();
        }
    }
}