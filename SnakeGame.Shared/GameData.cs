using System.Text.Json.Serialization;

namespace SnakeGame.Shared
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public FoodItem Food { get; set; } = new FoodItem();
        public PowerUp PowerUp { get; set; } = new PowerUp();
    }

    public class Player
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public List<Position> Snake { get; set; } = new List<Position>();
        public string Direction { get; set; } = "Right";
        public int Score { get; set; }
        public bool IsObserver { get; set; }
        public bool IsInvincible { get; set; }
        public DateTime InvincibleUntil { get; set; }
    }

    public class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
        
        public override bool Equals(object obj)
        {
            if (obj is Position other)
                return X == other.X && Y == other.Y;
            return false;
        }
        
        public override int GetHashCode() => HashCode.Combine(X, Y);
    }

    public class FoodItem
    {
        public Position Position { get; set; } = new Position();
        public bool Exists { get; set; } = true;
    }

    public class PowerUp
    {
        public Position Position { get; set; } = new Position();
        public bool Exists { get; set; }
    }
}
