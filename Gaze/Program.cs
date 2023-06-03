using Depths;
using Gaze;

Random random = new();

// Gems.SetSeed(25337);
Game game = new();
Renderer.Render(game);
Console.WriteLine();
while (Console.ReadLine() != "q")
{
    List<Move> moves = game.GetValidMoves();
    if (moves.Any())
    {
        Move randomMove = moves[random.Next(0, moves.Count - 1)];
        game.MakeMove(randomMove);
    }
    game.Progress();
    Renderer.Render(game);
    Console.WriteLine();
}
