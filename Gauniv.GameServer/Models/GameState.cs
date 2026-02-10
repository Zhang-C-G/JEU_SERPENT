using Gauniv.GameServer.Collectibles;

namespace Gauniv.GameServer.Models
{
    /// <summary>
    /// Complete game state for a match
    /// </summary>
    public class GameState
    {
        public const int MAP_SIZE = 30;  // 30x30 grid
        public const int INITIAL_FOOD_COUNT = 5;
        
        public string MatchId { get; set; } = Guid.NewGuid().ToString();
        public Player? Player1 { get; set; }
        public Player? Player2 { get; set; }
        public List<ICollectible> Collectibles { get; set; } = new();
        
        // Game timing
        public DateTime StartTime { get; set; }
        public int RemainingSeconds { get; set; } = 120;  // 2 minutes
        public bool IsGameOver { get; set; } = false;
        public string? WinnerId { get; set; }
        
        // Game mode
        public GameMode Mode { get; set; } = GameMode.PvP;
        
        /// <summary>
        /// Check if position is occupied by any snake
        /// </summary>
        public bool IsPositionOccupied(Position pos)
        {
            if (Player1 != null && !Player1.Snake.IsDead)
            {
                if (Player1.Snake.Body.Contains(pos))
                    return true;
            }
            
            if (Player2 != null && !Player2.Snake.IsDead)
            {
                if (Player2.Snake.Body.Contains(pos))
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if position is within map bounds
        /// </summary>
        public bool IsPositionValid(Position pos)
        {
            return pos.X >= 0 && pos.X < MAP_SIZE && 
                   pos.Y >= 0 && pos.Y < MAP_SIZE;
        }
        
        /// <summary>
        /// Get a random empty position
        /// </summary>
        public Position GetRandomEmptyPosition(Random random)
        {
            int maxAttempts = 100;
            for (int i = 0; i < maxAttempts; i++)
            {
                var pos = new Position(
                    random.Next(MAP_SIZE),
                    random.Next(MAP_SIZE)
                );
                
                if (!IsPositionOccupied(pos) && 
                    !Collectibles.Any(c => c.Position == pos))
                {
                    return pos;
                }
            }
            
            // Fallback to center if no empty position found
            return new Position(MAP_SIZE / 2, MAP_SIZE / 2);
        }
        
        /// <summary>
        /// Determine winner based on rules
        /// </summary>
        public void DetermineWinner()
        {
            if (Player1 == null || Player2 == null)
            {
                IsGameOver = true;
                return;
            }
            
            // Rule 1: If one player has no lives left
            if (!Player1.IsAlive && Player2.IsAlive)
            {
                WinnerId = Player2.PlayerId;
                IsGameOver = true;
                return;
            }
            if (Player1.IsAlive && !Player2.IsAlive)
            {
                WinnerId = Player1.PlayerId;
                IsGameOver = true;
                return;
            }
            
            // Rule 2 & 3: Time ran out, compare deaths then length
            if (RemainingSeconds <= 0)
            {
                if (Player1.DeathCount < Player2.DeathCount)
                    WinnerId = Player1.PlayerId;
                else if (Player2.DeathCount < Player1.DeathCount)
                    WinnerId = Player2.PlayerId;
                else if (Player1.Snake.Length > Player2.Snake.Length)
                    WinnerId = Player1.PlayerId;
                else if (Player2.Snake.Length > Player1.Snake.Length)
                    WinnerId = Player2.PlayerId;
                // else: draw, WinnerId remains null
                
                IsGameOver = true;
            }
        }
    }
    
    public enum GameMode
    {
        PvP,      // Player vs Player
        AITraining // Player vs AI
    }
}
