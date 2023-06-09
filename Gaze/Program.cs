using Depths;
using Gaze;
using Sanctum;

Gems.SetSeed(18);
Game game = new();
IStonesinger stonesinger = new Believer();
Renderer renderer = new();
renderer.HighlightCustomMove = true;
renderer.Render(game);
Console.WriteLine();
while (Console.ReadLine() != "q")
{
    renderer.HighlightedMove = null;
    Move? move = stonesinger.MakeMove(game);
    if (move.HasValue)
    {
        game.MakeMove(move.Value);
        renderer.HighlightedMove = move;
    }
    game.Progress();
    renderer.Render(game);
}
