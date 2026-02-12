using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.Collectibles
{
    /// <summary>
    /// Base interface for all collectible items - extensible design
    /// Supporting future additions of new collectibles without modifying core code
    /// </summary>
    public interface ICollectible
    {
        string Id { get; }
        CollectibleType Type { get; }
        Position Position { get; set; }
        
        /// <summary>
        /// Visual render ID - can map to color scheme key OR sprite file name
        /// Examples: "apple" (sprite), "food_basic" (color scheme)
        /// </summary>
        string RenderVisualId { get; }
        
        int SpawnWeight { get; }
        
        /// <summary>
        /// Called when snake collects this item
        /// </summary>
        CollectResult OnCollected(Snake snake, GameState state);
    }
    
    public enum CollectibleType
    {
        Food,
        PowerUp,
        Trap,
        Bonus,
        Special
    }
    
    /// <summary>
    /// Result of collecting an item
    /// </summary>
    public class CollectResult
    {
        public bool GrowSnake { get; set; } = false;
        public int GrowthAmount { get; set; } = 1;
        public int ScoreBonus { get; set; } = 0;
        public Effect? AppliedEffect { get; set; }
        public bool RemoveCollectible { get; set; } = true;
        public string? Message { get; set; }
    }
    
    public class Effect
    {
        public EffectType Type { get; set; }
        public int DurationMs { get; set; }
        public float Multiplier { get; set; } = 1.0f;
    }
    
    public enum EffectType
    {
        Speed,
        Slow,
        Invincibility,
        Magnet
    }
    
    // ========== Built-in Collectible Types ==========
    
    /// <summary>
    /// Basic food - standard apple
    /// </summary>
    public class BasicFood : ICollectible
    {
        public string Id => "food_apple";
        public CollectibleType Type => CollectibleType.Food;
        public Position Position { get; set; }
        public string RenderVisualId => "apple";  // Maps to sprite or color scheme
        public int SpawnWeight => 100;
        
        public CollectResult OnCollected(Snake snake, GameState state)
        {
            return new CollectResult
            {
                GrowSnake = true,
                GrowthAmount = 1,
                ScoreBonus = 10,
                RemoveCollectible = true
            };
        }
    }
    
    /// <summary>
    /// Super food - golden apple, grows +3 segments
    /// </summary>
    public class SuperFood : ICollectible
    {
        public string Id => "food_golden_apple";
        public CollectibleType Type => CollectibleType.Food;
        public Position Position { get; set; }
        public string RenderVisualId => "golden_apple";
        public int SpawnWeight => 10;  // Rare
        
        public CollectResult OnCollected(Snake snake, GameState state)
        {
            return new CollectResult
            {
                GrowSnake = true,
                GrowthAmount = 3,
                ScoreBonus = 50,
                RemoveCollectible = true,
                Message = "🌟 Super Food! +3"
            };
        }
    }
    
    /// <summary>
    /// Speed boost power-up
    /// </summary>
    public class SpeedBoost : ICollectible
    {
        public string Id => "powerup_speed";
        public CollectibleType Type => CollectibleType.PowerUp;
        public Position Position { get; set; }
        public string RenderVisualId => "lightning";
        public int SpawnWeight => 20;
        
        public CollectResult OnCollected(Snake snake, GameState state)
        {
            return new CollectResult
            {
                ScoreBonus = 5,
                AppliedEffect = new Effect
                {
                    Type = EffectType.Speed,
                    DurationMs = 3000,  // 3 seconds
                    Multiplier = 1.5f    // 50% faster
                },
                RemoveCollectible = true,
                Message = "⚡ Speed Boost!"
            };
        }
    }
    
    /// <summary>
    /// Shield/invincibility power-up
    /// </summary>
    public class Shield : ICollectible
    {
        public string Id => "powerup_shield";
        public CollectibleType Type => CollectibleType.PowerUp;
        public Position Position { get; set; }
        public string RenderVisualId => "shield";
        public int SpawnWeight => 15;
        
        public CollectResult OnCollected(Snake snake, GameState state)
        {
            return new CollectResult
            {
                AppliedEffect = new Effect
                {
                    Type = EffectType.Invincibility,
                    DurationMs = 2000,   // 2 seconds invincible
                    Multiplier = 1.0f
                },
                RemoveCollectible = true,
                Message = "🛡️ Shield!"
            };
        }
    }
}
