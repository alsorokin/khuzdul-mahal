using Depths;

namespace Sanctum
{
    /// <summary>
    /// A stonesinger is an AI that makes moves for the player.
    /// </summary>
    public interface IStonesinger
    {
        Move? MakeMove(Game game);
    }
}