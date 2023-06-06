using Depths;
using Gaze;

Random random = new();
long lastTick;

Gems.SetSeed(18);
Game game = new();
Renderer renderer = new();
renderer.HighlightCustomMove = true;
renderer.Render(game);
Console.WriteLine();
while (Console.ReadLine() != "q")
{
    List<Move> moves = game.GetValidMoves();
    renderer.HighlightedMove = null;
    if (moves.Any())
    {
        Move randomMove = moves[random.Next(0, moves.Count - 1)];
        game.MakeMove(randomMove);
        renderer.HighlightedMove = randomMove;
    }
    lastTick = game.Tick;
    game.Progress();
    if (game.Tick == lastTick)
    {
        // continue;
    }
    renderer.Render(game);
}
