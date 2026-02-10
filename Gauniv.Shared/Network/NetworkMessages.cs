using MessagePack;

namespace Gauniv.Shared.Network
{
    /// <summary>
    /// Network message types for client-server communication
    /// </summary>
    
    // Client -> Server messages
    [MessagePackObject]
    public class PlayerJoinMessage
    {
        [Key(0)]
        public string PlayerName { get; set; } = string.Empty;
        
        [Key(1)]
        public GameMode Mode { get; set; }
    }
    
    [MessagePackObject]
    public class PlayerInputMessage
    {
        [Key(0)]
        public string PlayerId { get; set; } = string.Empty;
        
        [Key(1)]
        public DirectionCommand Direction { get; set; }
    }
    
    // Server -> Client messages
    [MessagePackObject]
    public class GameStateMessage
    {
        [Key(0)]
        public string MatchId { get; set; } = string.Empty;
        
        [Key(1)]
        public List<SnakeState> Snakes { get; set; } = new();
        
        [Key(2)]
        public List<CollectibleState> Collectibles { get; set; } = new();
        
        [Key(3)]
        public int RemainingSeconds { get; set; }
        
        [Key(4)]
        public Dictionary<string, PlayerStats> PlayerStats { get; set; } = new();
        
        [Key(5)]
        public bool IsGameOver { get; set; }
        
        [Key(6)]
        public string? WinnerId { get; set; }
    }
    
    [MessagePackObject]
    public class SnakeState
    {
        [Key(0)]
        public string PlayerId { get; set; } = string.Empty;
        
        [Key(1)]
        public List<Position> Body { get; set; } = new();
        
        [Key(2)]
        public DirectionCommand Direction { get; set; }
        
        [Key(3)]
        public bool IsDead { get; set; }
        
        [Key(4)]
        public bool IsRespawning { get; set; }
    }
    
    [MessagePackObject]
    public class CollectibleState
    {
        [Key(0)]
        public Position Position { get; set; }
        
        [Key(1)]
        public string RenderVisualId { get; set; } = string.Empty;
    }
    
    [MessagePackObject]
    public class PlayerStats
    {
        [Key(0)]
        public int Lives { get; set; }
        
        [Key(1)]
        public int Deaths { get; set; }
        
        [Key(2)]
        public int Score { get; set; }
    }
    
    [MessagePackObject]
    public struct Position
    {
        [Key(0)]
        public int X { get; set; }
        
        [Key(1)]
        public int Y { get; set; }
    }
    
    public enum DirectionCommand
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }
    
    public enum GameMode
    {
        PvP = 0,
        AITraining = 1
    }
}
