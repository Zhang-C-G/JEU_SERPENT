// Copy RenderConfig from Shared project
namespace Gauniv.Game.Rendering
{
    /// <summary>
    /// Configuration for visual rendering - supports both colors and sprite images
    /// </summary>
    public class RenderConfig
    {
        public bool UseSpriteImages { get; set; } = false;
        public string SpriteBasePath { get; set; } = "Assets/Sprites";
        
        public ColorScheme Snake1Color { get; set; } = new ColorScheme
        {
            Primary = "#FF0000",
            Secondary = "#CC0000",
            Accent = "#FFFFFF"
        };
        
        public ColorScheme Snake2Color { get; set; } = new ColorScheme
        {
            Primary = "#0000FF",
            Secondary = "#0000CC",
            Accent = "#FFFFFF"
        };
        
        public ColorScheme FoodColor { get; set; } = new ColorScheme
        {
            Primary = "#FFD700",
            Secondary = "#FFA500"
        };
        
        public string BackgroundColor { get; set; } = "#2C2C2C";
        public string GridLineColor { get; set; } = "#404040";
        public bool ShowGrid { get; set; } = true;
        
        public int CellSize { get; set; } = 20;
        public int CellPadding { get; set; } = 2;
        public int CornerRadius { get; set; } = 0;
    }
    
    public class ColorScheme
    {
        public string Primary { get; set; } = "#000000";
        public string Secondary { get; set; } = "#000000";
        public string Accent { get; set; } = "#FFFFFF";
    }
    
    public class SpriteMap
    {
        public string SnakeHeadSprite { get; set; } = "snake_head.png";
        public string SnakeBodySprite { get; set; } = "snake_body.png";
        
        public Dictionary<string, string> CollectibleSprites { get; set; } = new()
        {
            { "apple", "apple.png" },
            { "golden_apple", "golden_apple.png" },
            { "lightning", "lightning.png" },
            { "shield", "shield.png" }
        };
        
        public string GetSpritePath(string collectibleId, string basePath)
        {
            if (CollectibleSprites.TryGetValue(collectibleId, out var fileName))
            {
                return Path.Combine(basePath, fileName);
            }
            return Path.Combine(basePath, "default.png");
        }
    }
}
