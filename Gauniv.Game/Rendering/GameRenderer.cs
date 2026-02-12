namespace Gauniv.Game.Rendering
{
    /// <summary>
    /// Game renderer with flexible visual system
    /// Supports both color schemes and sprite images
    /// </summary>
    public class GameRenderer
    {
        private readonly RenderConfig _config;
        private readonly SpriteMap _spriteMap;
        private readonly Dictionary<string, Image> _loadedSprites = new();
        private readonly Dictionary<string, Brush> _brushCache = new();
        
        public GameRenderer(RenderConfig config)
        {
            _config = config;
            _spriteMap = new SpriteMap();
            
            // Load sprites if using sprite mode
            if (_config.UseSpriteImages)
            {
                LoadSprites();
            }
        }
        
        /// <summary>
        /// Draw entire game state
        /// </summary>
        public void DrawGame(Graphics g, GameStateDto state)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Draw background
            DrawBackground(g);
            
            // Draw grid (optional)
            if (_config.ShowGrid)
            {
                DrawGrid(g);
            }
            
            // Draw collectibles
            foreach (var collectible in state.Collectibles)
            {
                DrawCollectible(g, collectible);
            }
            
            // Draw snakes
            DrawSnake(g, state.Player1Snake, _config.Snake1Color, 1);
            DrawSnake(g, state.Player2Snake, _config.Snake2Color, 2);
            
            // Draw UI overlay
            DrawUI(g, state);
        }
        
        private void DrawBackground(Graphics g)
        {
            var bgColor = ColorFromHex(_config.BackgroundColor);
            g.Clear(bgColor);
        }
        
        private void DrawGrid(Graphics g)
        {
            var gridColor = ColorFromHex(_config.GridLineColor);
            using var pen = new Pen(gridColor, 1);
            
            int mapSize = 30;  // From GameState.MAP_SIZE
            int totalWidth = mapSize * _config.CellSize;
            int totalHeight = mapSize * _config.CellSize;
            
            // Vertical lines
            for (int x = 0; x <= mapSize; x++)
            {
                int px = x * _config.CellSize;
                g.DrawLine(pen, px, 0, px, totalHeight);
            }
            
            // Horizontal lines
            for (int y = 0; y <= mapSize; y++)
            {
                int py = y * _config.CellSize;
                g.DrawLine(pen, 0, py, totalWidth, py);
            }
        }
        
        private void DrawSnake(Graphics g, SnakeDto? snake, ColorScheme colorScheme, int playerNumber)
        {
            if (snake == null || snake.IsDead) return;
            
            for (int i = 0; i < snake.Body.Count; i++)
            {
                var segment = snake.Body[i];
                var rect = GetCellRect(segment);
                
                if (i == 0)  // Head
                {
                    DrawSnakeHead(g, rect, colorScheme, snake.Direction);
                }
                else  // Body
                {
                    DrawSnakeBody(g, rect, colorScheme);
                }
            }
        }
        
        private void DrawSnakeHead(Graphics g, Rectangle rect, ColorScheme colorScheme, DirectionDto direction)
        {
            if (_config.UseSpriteImages)
            {
                // Draw sprite if available
                if (_loadedSprites.TryGetValue(_spriteMap.SnakeHeadSprite, out var sprite))
                {
                    g.DrawImage(sprite, rect);
                    return;
                }
            }
            
            // Draw colored head
            var color = ColorFromHex(colorScheme.Primary);
            
            if (_config.CornerRadius > 0)
            {
                using var path = GetRoundedRectPath(rect, _config.CornerRadius);
                using var brush = new SolidBrush(color);
                g.FillPath(brush, path);
            }
            else
            {
                g.FillEllipse(new SolidBrush(color), rect);
            }
            
            // Draw eyes
            var eyeColor = ColorFromHex(colorScheme.Accent);
            int eyeSize = rect.Width / 5;
            
            if (direction == DirectionDto.Right || direction == DirectionDto.Left)
            {
                int eyeY = rect.Y + rect.Height / 4;
                int eyeX1 = rect.X + rect.Width / 4;
                int eyeX2 = rect.X + 3 * rect.Width / 4 - eyeSize;
                
                g.FillEllipse(new SolidBrush(eyeColor), eyeX1, eyeY, eyeSize, eyeSize);
                g.FillEllipse(new SolidBrush(eyeColor), eyeX2, eyeY, eyeSize, eyeSize);
            }
        }
        
        private void DrawSnakeBody(Graphics g, Rectangle rect, ColorScheme colorScheme)
        {
            if (_config.UseSpriteImages)
            {
                if (_loadedSprites.TryGetValue(_spriteMap.SnakeBodySprite, out var sprite))
                {
                    g.DrawImage(sprite, rect);
                    return;
                }
            }
            
            // Draw colored body
            var color = ColorFromHex(colorScheme.Secondary);
            
            if (_config.CornerRadius > 0)
            {
                using var path = GetRoundedRectPath(rect, _config.CornerRadius);
                using var brush = new SolidBrush(color);
                g.FillPath(brush, path);
            }
            else
            {
                g.FillRectangle(new SolidBrush(color), rect);
            }
        }
        
        private void DrawCollectible(Graphics g, CollectibleDto collectible)
        {
            var rect = GetCellRect(collectible.Position);
            
           if (_config.UseSpriteImages)
            {
                // Try to load sprite for this collectible
                string spritePath = _spriteMap.GetSpritePath(collectible.RenderVisualId, _config.SpriteBasePath);
                
                if (_loadedSprites.TryGetValue(spritePath, out var sprite))
                {
                    g.DrawImage(sprite, rect);
                    return;
                }
            }
            
            // Fallback: draw colored blocks based on type
            Color color;
            if (collectible.RenderVisualId == "golden_apple")
            {
                // Rare collectible - golden block
                color = Color.Gold;
            }
            else
            {
                // Common collectible - red block
                color = Color.FromArgb(220, 50, 50);
            }
            
            g.FillEllipse(new SolidBrush(color), rect);
        }
        
        private void DrawUI(Graphics g, GameStateDto state)
        {
            Font font = new Font("Arial", 12, FontStyle.Bold);
            Font smallFont = new Font("Arial", 10);
            
            // Player 1 lives and deaths
            string p1Info = $"P1: {GetHeartString(state.Player1Lives)} Deaths: {state.Player1Deaths}";
            g.DrawString(p1Info, font, Brushes.Red, 10, 10);
            
            // Player 2 lives and deaths
            string p2Info = $"P2: {GetHeartString(state.Player2Lives)} Deaths: {state.Player2Deaths}";
            g.DrawString(p2Info, font, Brushes.Blue, 10, 35);
            
            // Timer
            int minutes = state.RemainingSeconds / 60;
            int seconds = state.RemainingSeconds % 60;
            string timerText = $"⏱️ {minutes:D2}:{seconds:D2}";
            g.DrawString(timerText, font, Brushes.Yellow, 500, 10);
            
            // Lengths
            string lengthText = $"Lengths: P1={state.Player1Snake?.Body?.Count ?? 0} P2={state.Player2Snake?.Body?.Count ?? 0}";
            g.DrawString(lengthText, smallFont, Brushes.White, 10, 60);
        }
        
        private string GetHeartString(int lives)
        {
            string hearts = "";
            for (int i = 0; i < lives; i++) hearts += "❤";
            for (int i = 0; i < (3 - lives); i++) hearts += "🖤";
            return hearts;
        }
        
        private Rectangle GetCellRect(PositionDto pos)
        {
            int x = pos.X * _config.CellSize + _config.CellPadding;
            int y = pos.Y * _config.CellSize + _config.CellPadding;
            int size = _config.CellSize - (2 * _config.CellPadding);
            
            return new Rectangle(x, y, size, size);
        }
        
        private System.Drawing.Drawing2D.GraphicsPath GetRoundedRectPath(Rectangle rect, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
        
        private Color ColorFromHex(string hex)
        {
            hex = hex.TrimStart('#');
            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16)
            );
        }
        
        private void LoadSprites()
        {
            // Load snake sprites
            TryLoadSprite(_spriteMap.SnakeHeadSprite);
            TryLoadSprite(_spriteMap.SnakeBodySprite);
            
            // Load collectible sprites
            foreach (var kvp in _spriteMap.CollectibleSprites)
            {
                string path = Path.Combine(_config.SpriteBasePath, kvp.Value);
                TryLoadSprite(path);
            }
        }
        
        private void TryLoadSprite(string fileName)
        {
            string fullPath = Path.Combine(_config.SpriteBasePath, fileName);
            
            if (File.Exists(fullPath) && !_loadedSprites.ContainsKey(fullPath))
            {
                try
                {
                    _loadedSprites[fullPath] = Image.FromFile(fullPath);
                }
                catch
                {
                    // Sprite loading failed, will use color fallback
                }
            }
        }
    }
    
    // DTO classes for rendering
    public class GameStateDto
    {
        public SnakeDto? Player1Snake { get; set; }
        public SnakeDto? Player2Snake { get; set; }
        public List<CollectibleDto> Collectibles { get; set; } = new();
        public int Player1Lives { get; set; }
        public int Player2Lives { get; set; }
        public int Player1Deaths { get; set; }
        public int Player2Deaths { get; set; }
        public int RemainingSeconds { get; set; }
        public bool IsGameOver { get; set; }
        public string? WinnerName { get; set; }
    }
    
    public class SnakeDto
    {
        public List<PositionDto> Body { get; set; } = new();
        public DirectionDto Direction { get; set; }
        public bool IsDead { get; set; }
    }
    
    public class CollectibleDto
    {
        public PositionDto Position { get; set; }
        public string RenderVisualId { get; set; } = string.Empty;
    }
    
    public struct PositionDto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
    
    public enum DirectionDto
    {
        Up, Down, Left, Right
    }
}
