namespace Depths
{
    /// <summary>
    /// Represents a potential move that the player can make. A move is defined by the starting
    /// position of the gem and the direction in which it is moved.
    /// </summary>
    public struct Move
    {
        public Move(Position start, int dx = 0, int dy = 0)
        {
            if (dx < -1 || dx > 1)
                throw new ArgumentOutOfRangeException(nameof(dx), dx, "Move must be at most 1 unit in either direction");
            if (dy < -1 || dy > 1)
                throw new ArgumentOutOfRangeException(nameof(dy), dy, "Move must be at most 1 unit in either direction");
            if (dx == 0 && dy == 0)
                throw new ArgumentException($"Move must be non-zero. Set either {nameof(dx)} or {nameof(dy)}", nameof(dx));
            if (Math.Abs(dx) == 1 && Math.Abs(dy) == 1)
                throw new ArgumentException($"Move must not be diagonal. Set either {nameof(dx)} or {nameof(dy)}", nameof(dx));
            Start = start;
            DX = dx;
            DY = dy;
            Processed = false;
        }

        public Position Start;
        public int DX;
        public int DY;
        public bool Processed;
        public Position End => new() { X = Start.X + DX, Y = Start.Y + DY };

        public static bool operator ==(Move a, Move b)
        {
            if (a.DX == b.DX && a.DY == b.DY && a.Start == b.Start)
            {
                return true;
            }
            else if (a.DX == -b.DX && a.DY == -b.DY && a.End == b.Start)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Move a, Move b) => !(a == b);

        public override bool Equals(object? obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is Move move)
            {
                return this == move;
            }
            return false;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
