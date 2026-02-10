namespace Gauniv.Shared.Rendering
{
    /// <summary>
    /// Configuration for visual rendering - supports both colors and sprite images
    /// This allows switching between color-based rendering and image-based rendering
    /// </summary>
    public class RenderConfig
    {
        /// <summary>
        /// Use sprite images instead of colors (false = use colors, true = use sprites)
        /// </summary>
        public bool UseSpriteImages { get; set; } = false;
        
        /// <summary>
        /// Base directory for sprite images (if UseSpriteImages = true)
        /// </summary>
        public string SpriteBasePath { get; set; } = "Assets/Sprites";
        
        // Snake Colors
        public ColorScheme Snake1Color { get; set; } = new ColorScheme
        {
            Primary = "#FF0000",   // Red
            Secondary = "#CC0000",
            Accent = "#FFFFFF"     // Eyes
        };
        
        public ColorScheme Snake2Color { get; set; } = new ColorScheme
        {
            Primary = "#0000FF",   // Blue
            Secondary = "#0000CC",
            Accent = "#FFFFFF"
        };
        
        // Collectibles
        public ColorScheme FoodColor { get; set; } = new ColorScheme
        {
            Primary = "#FFD700",   // Golden
            Secondary = "#FFA500"
        };
        
        // Background and Grid
        public string BackgroundColor { get; set; } = "#2C2C2C";  // Dark gray
        public string GridLineColor { get; set; } = "#404040";    // Light gray
        public bool ShowGrid { get; set; } = true;
        
        // Cell rendering
        public int CellSize { get; set; } = 20;  // pixels
        public int CellPadding { get; set; } = 2; // pixels between cells
        public int CornerRadius { get; set; } = 0; // 0 = square, >0 = rounded
    }
    
    public class ColorScheme
    {
        public string Primary { get; set; } = "#000000";
        public string Secondary { get; set; } = "#000000";
        public string Accent { get; set; } = "#FFFFFF";
    }
    
    /// <summary>
    /// Sprite mapping - maps collectible IDs to sprite file names
    /// </summary>
    public class SpriteMap
    {
        // Snake sprites
        public string SnakeHeadSprite { get; set; } = "snake_head.png";
        public string SnakeBodySprite { get; set; } = "snake_body.png";
        
        // Collectible sprites (maps to collectible IDs)
        public Dictionary<string, string> CollectibleSprites { get; set; } = new()
        {
            { "food_apple", "apple.png" },
            { "food_golden_apple", "golden_apple.png" },
            { "powerup_speed", "lightning.png" },
            { "powerup_shield", "shield.png" },
            { "trap_poison", "poison_apple.png" },
            { "trap_slow", "ice_crystal.png" }
        };
        
        /// <summary>
        /// Get sprite path for a collectible ID
        /// </summary>
        public string GetSpritePath(string collectibleId, string basePath)
        {
            if (CollectibleSprites.TryGetValue(collectibleId, out var fileName))
            {
                return Path.Combine(basePath, fileName);
            }
            return Path.Combine(basePath, "default.png");
        }
    }
    
    /// <summary>
    /// Factory to create default render configurations
    /// </summary>
    public static class RenderConfigFactory
    {
        public static RenderConfig PixelArt()
        {
            return new RenderConfig
            {
                UseSpriteImages = true,
                SpriteBasePath = "Assets/Sprites/PixelArt",
                CellSize = 20,
                CellPadding = 1,
                CornerRadius = 0,
                BackgroundColor = "#2C2C2C",
                ShowGrid = true
            };
        }
        
        public static RenderConfig ModernFlat()
        {
            return new RenderConfig
            {
                UseSpriteImages = false,
                CellSize = 24,
                CellPadding = 2,
                CornerRadius = 4,  // Rounded corners
                Snake1Color = new ColorScheme
                {
                    Primary = "#4CAF50",   // Material Green
                    Secondary = "#388E3C",
                    Accent = "#FFFFFF"
                },
                Snake2Color = new ColorScheme
                {
                    Primary = "#2196F3",   // Material Blue
                    Secondary = "#1976D2",
                    Accent = "#FFFFFF"
                },
                FoodColor = new ColorScheme
                {
                    Primary = "#FF5722",   // Material Deep Orange
                    Secondary = "#E64A19"
                },
                BackgroundColor = "#FAFAFA",
                GridLineColor = "#E0E0E0",
                ShowGrid = true
            };
        }
        
        public static RenderConfig NeonGlow()
        {
            return new RenderConfig
            {
                UseSpriteImages = false,
                CellSize = 22,
                CellPadding = 3,
                CornerRadius = 0,
                Snake1Color = new ColorScheme
                {
                    Primary = "#00FFFF",   // Cyan neon
                    Secondary = "#00CCCC",
                    Accent = "#FFFFFF"
                },
                Snake2Color = new ColorScheme
                {
                    Primary = "#FF00FF",   // Magenta neon
                    Secondary = "#CC00CC",
                    Accent = "#FFFFFF"
                },
                FoodColor = new ColorScheme
                {
                    Primary = "#FFFF00",   // Yellow glow
                    Secondary = "#CCCC00"
                },
                BackgroundColor = "#000000",  // Pure black
                GridLineColor = "#111111",
                ShowGrid = false
            };
        }
        
        public static RenderConfig Minimalist()
        {
            return new RenderConfig
            {
                UseSpriteImages = false,
                CellSize = 20,
                CellPadding = 1,
                CornerRadius = 0,
                Snake1Color = new ColorScheme
                {
                    Primary = "#000000",   // Black
                    Secondary = "#1A1A1A",
                    Accent = "#FFFFFF"
                },
                Snake2Color = new ColorScheme
                {
                    Primary = "#666666",   // Dark gray
                    Secondary = "#4D4D4D",
                    Accent = "#FFFFFF"
                },
                FoodColor = new ColorScheme
                {
                    Primary = "#000000",   // Black outline
                    Secondary = "#FFFFFF"  // White fill
                },
                BackgroundColor = "#FFFFFF",  // Pure white
                GridLineColor = "#DDDDDD",
                ShowGrid = true
            };
        }
    }
}
