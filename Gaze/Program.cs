using Depths;
using Gaze;

// Gems.SetSeed(25337);
Game game = new();
Renderer.Render(game);
Console.WriteLine();
Console.WriteLine("---------------");
while (Console.ReadLine() != "q")
{
    game.Progress();
    Renderer.Render(game);
    Console.WriteLine();
    Console.WriteLine("---------------");
}
