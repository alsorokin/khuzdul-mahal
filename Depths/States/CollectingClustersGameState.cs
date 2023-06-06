using System.Diagnostics;

namespace Depths.States
{
    internal class CollectingClustersGameState : IGameState
    {
        public void Progress(Game game)
        {
            List<GemCluster> clusters = game.GetClusters();
            bool hypercubesCollected = CollectHypercubes(game);
            bool clustersCollected = CollectClusters(game, clusters);
            game.Tick++;
            // If gems were collected, either by hypercubes or clusters, drop gems
            if (hypercubesCollected || clustersCollected)
            {
                game.State = GameStates.DroppingGems;
            }
            else
            {
                game.State = GameStates.Idle;
            }
        }

        private static bool CollectHypercubes(Game game)
        {
            if (!game.lastMove.HasValue)
            {
                return false;
            }
            // If only one end of the last move is a hypercube, collect all gems of the other kind and the hypercube itself
            // If both are hypercubes, collect all gems of any kind (this is handled in CollectGem)
            if (game.Field.GetGemKindAt(game.lastMove.Value.Start) == GemKind.Hypercube ||
                game.Field.GetGemKindAt(game.lastMove.Value.End) == GemKind.Hypercube)
            {
                GemKind otherGemKind = game.Field.GetGemKindAt(game.lastMove.Value.Start) == GemKind.Hypercube ?
                    game.Field.GetGemKindAt(game.lastMove.Value.End) : game.Field.GetGemKindAt(game.lastMove.Value.Start);
                CollectGem(game, game.lastMove.Value.Start.X, game.lastMove.Value.Start.Y, otherGemKind);
                CollectGem(game, game.lastMove.Value.End.X, game.lastMove.Value.End.Y, otherGemKind);
                game.Tick++;
                return true;
            }
            return false;
        }

        private static bool CollectClusters(Game game, List<GemCluster> clusters)
        {
            foreach (GemCluster cluster in clusters)
            {
                GemKind clusterGemKind = game.Field.GetGemKindAt(cluster.GemPositions.First());
                Debug.Assert(clusterGemKind != GemKind.None, "Cluster gem kind must not be None");
                Debug.Assert(cluster.GemPositions.Count > 2, "Cluster must have at least three gems in it");

                game.Combo++;
                game.Score += cluster.WorthBonus * game.Combo;
                foreach (Position gem in cluster.GemPositions)
                {
                    CollectGem(game, gem.X, gem.Y, game.Field.GetGemKindAt(cluster.GemPositions.First()));
                }

                // Create new power gems for non-simple clusters
                // If the cluster is a part of a move, spawn the power gem at the start or end of the move
                // Otherwise, spawn it in the middle of the cluster
                if (cluster.ClusterType == ClusterType.Simple)
                {
                    continue;
                }
                // TODO: Bad logic, refactor
                Position gemSpawnPoint = cluster.GemPositions.Any(p => p == game.lastMove?.Start || p == game.lastMove?.End) ?
                    cluster.GemPositions.FirstOrDefault(p => p == game.lastMove?.Start || p == game.lastMove?.End) :
                    cluster.GemPositions[(int)Math.Round(cluster.GemPositions.Count / 2f, MidpointRounding.ToPositiveInfinity)];

                game.Field.SetGemKindAt(gemSpawnPoint, cluster.ClusterType == ClusterType.Hyper ? GemKind.Hypercube : clusterGemKind);
                game.Field.SetGemPowerAt(gemSpawnPoint, cluster.ClusterType switch
                {
                    ClusterType.Four => GemPower.Fire,
                    ClusterType.L => GemPower.Star, // TODO: Gem should spawn at the intersection
                    ClusterType.LargeL => GemPower.Star, // TODO: This should spawn a star gem and a fire gem
                    ClusterType.Hyper => GemPower.Hypercube,
                    ClusterType.Supernova => GemPower.Supernova,
                    _ => throw new Exception("Invalid cluster type")
                });
            }
            return clusters.Count > 0;
        }

        private static void CollectGem(Game game, int x, int y, GemKind otherKind)
        {
            if (game.Field.GetGemKindAt(x, y) == GemKind.None)
            {
                return;
            }
            GemKind thisKind = game.Field.GetGemKindAt(x, y);
            // Collect the gem
            game.Field.SetGemKindAt(x, y, GemKind.None);
            game.Score += Gems.GemWorth * game.Combo;

            // If the gem had a power, activate it
            switch (game.Field.GetGemPowerAt(x, y))
            {
                case GemPower.Fire:
                    // handle fire power: 3x3 explosion
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = y - 1; yy <= y + 1; yy++)
                        {
                            if (xx < 0 || xx >= GameField.Width || yy < 0 || yy >= GameField.Height)
                                continue;
                            CollectGem(game, xx, yy, thisKind);
                        }
                    }
                    break;
                case GemPower.Star:
                    // handle star power: collect all gems in one vertical and one horizontal line
                    for (int xx = 0; xx < GameField.Width; xx++)
                    {
                        CollectGem(game, xx, y, thisKind);
                    }
                    for (int yy = 0; yy < GameField.Height; yy++)
                    {
                        CollectGem(game, x, yy, thisKind);
                    }
                    break;
                case GemPower.Hypercube:
                    // handle hypercube power: collect all gems of the same kind
                    // if the other gem is also a hypercube, collect all gems
                    game.Field.ForEachGem((pos, k, _) =>
                    {
                        if (k == otherKind || otherKind == GemKind.Hypercube)
                        {
                            CollectGem(game, pos.X, pos.Y, otherKind);
                        }
                    });
                    break;
                case GemPower.Supernova:
                    // handle supernova power: collect all gems in three horizontal and three vertical lines
                    for (int xx = x - 1; xx <= x + 1; xx++)
                    {
                        for (int yy = 0; yy < GameField.Height; yy++)
                        {
                            CollectGem(game, xx, yy, thisKind);
                        }
                    }
                    for (int yy = y - 1; yy <= y + 1; yy++)
                    {
                        for (int xx = 0; xx < GameField.Width; xx++)
                        {
                            CollectGem(game, xx, yy, thisKind);
                        }
                    }
                    break;
                default:
                    break;
            }
            game.Field.SetGemPowerAt(x, y, GemPower.Normal);
        }
    }
}
