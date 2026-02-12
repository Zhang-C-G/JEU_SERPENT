namespace Gauniv.GameServer.Models
{
    /// <summary>
    /// 2D position on the game grid
    /// </summary>
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Position a, Position b) => !(a == b);
        
        public override bool Equals(object? obj) => obj is Position pos && this == pos;
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }
    
    /// <summary>
    /// Snake movementdirection
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }
    
    /// <summary>
    /// Player state in the game
    /// </summary>
    public class Player
    {
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public Snake Snake { get; set; } = new();
        public int LivesRemaining { get; set; } = 3;  // 3 lives per player
        public int DeathCount { get; set; } = 0;
        public int Score { get; set; } = 0;
        public int MaxLength { get; set; } = 3;  // Track maximum snake length achieved
        public bool IsAlive => LivesRemaining > 0;
        public bool IsRespawning { get; set; } = false;
        public DateTime? RespawnTime { get; set; }
    }
    
    /// <summary>
    /// Snake entity
    /// </summary>
    public class Snake
    {
        public List<Position> Body { get; set; } = new();
        public Direction CurrentDirection { get; set; } = Direction.Right;
        public Direction NextDirection { get; set; } = Direction.Right;
        public bool IsDead { get; set; } = false;
        
        public Position Head => Body.Count > 0 ? Body[0] : new Position(0, 0);
        public int Length => Body.Count;
        
        /// <summary>
        /// Initialize snake at starting position
        /// </summary>
        public void Initialize(Position startPos, Direction direction)
        {
            Body.Clear();
            Body.Add(startPos);
            Body.Add(new Position(startPos.X - 1, startPos.Y));
            Body.Add(new Position(startPos.X - 2, startPos.Y));
            CurrentDirection = direction;
            NextDirection = direction;
            IsDead = false;
        }
        
        /// <summary>
        /// Move snake in current direction
        /// </summary>
        public void Move(bool grow = false)
        {
            CurrentDirection = NextDirection;
            
            // Calculate new head position
            Position newHead = CurrentDirection switch
            {
                Direction.Up => new Position(Head.X, Head.Y - 1),
                Direction.Down => new Position(Head.X, Head.Y + 1),
                Direction.Left => new Position(Head.X - 1, Head.Y),
                Direction.Right => new Position(Head.X + 1, Head.Y),
                _ => Head
            };
            
            // Add new head
            Body.Insert(0, newHead);
            
            // Remove tail if not growing
            if (!grow && Body.Count > 0)
            {
                Body.RemoveAt(Body.Count - 1);
            }
        }
        
        /// <summary>
        /// Change direction (prevent 180-degree turns)
        /// </summary>
        public void SetDirection(Direction newDirection)
        {
            // Prevent reversing direction
            bool isOpposite = (CurrentDirection == Direction.Up && newDirection == Direction.Down) ||
                            (CurrentDirection == Direction.Down && newDirection == Direction.Up) ||
                            (CurrentDirection == Direction.Left && newDirection == Direction.Right) ||
                            (CurrentDirection == Direction.Right && newDirection == Direction.Left);
                            
            if (!isOpposite)
            {
                NextDirection = newDirection;
            }
        }
    }
}
