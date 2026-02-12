using Gauniv.Game.Rendering;

namespace Gauniv.Game
{
    /// <summary>
    /// Main game form - displays the game
    /// </summary>
    public partial class GameForm : Form
    {
        private GameRenderer _renderer;
        private GameStateDto _gameState;
        private System.Windows.Forms.Timer _renderTimer;
        private System.Windows.Forms.Timer _gameTimer;
        private MenuForm.GameMode _gameMode;
        
        // For local game testing
        private LocalGameController? _localController;
        
        public GameForm(MenuForm.GameMode mode)
        {
            _gameMode = mode;
            InitializeComponent();
            InitializeGame();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Snake Duel - 贪吃蛇对决";
            this.ClientSize = new Size(620, 660);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Set up paint event
            this.Paint += GameForm_Paint;
            this.KeyDown += GameForm_KeyDown;
        }
        
        private void InitializeGame()
        {
            // Create renderer with color scheme
            var config = new RenderConfig
            {
                UseSpriteImages = false,
                CellSize = 20,
                CellPadding = 1,
                CornerRadius = 0,
                ShowGrid = true
            };
            
            _renderer = new GameRenderer(config);
            
            // Create local game controller for standalone play
            _localController = new LocalGameController(_gameMode);
            _gameState = _localController.GetGameState();
            
            // Set up render timer (60 FPS)
            _renderTimer = new System.Windows.Forms.Timer();
            _renderTimer.Interval = 16;
            _renderTimer.Tick += (s, e) => this.Invalidate();
            _renderTimer.Start();
            
            // Set up game update timer (10 FPS - matches server tick rate)
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 100;
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();
        }
        
        private void GameTimer_Tick(object? sender, EventArgs e)
        {
            if (_localController != null)
            {
                _localController.Tick();
                _gameState = _localController.GetGameState();
                
                // Check game over
                if (_gameState.IsGameOver)
                {
                    _gameTimer.Stop();
                    ShowGameOver();
                }
            }
        }
        
        private void ShowGameOver()
        {
            string winner = _gameState.WinnerName ?? "平局";
            MessageBox.Show($"游戏结束！\n\n胜者: {winner}\n\nP1死亡: {_gameState.Player1Deaths}\nP2死亡: {_gameState.Player2Deaths}", 
                "游戏结束", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
        
        private void GameForm_Paint(object? sender, PaintEventArgs e)
        {
            if (_gameState != null)
            {
                _renderer.DrawGame(e.Graphics, _gameState);
            }
        }
        
        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_localController == null) return;
            
            // Player 1 controls (WASD)
            switch (e.KeyCode)
            {
                case Keys.W:
                    _localController.SetPlayer1Direction(DirectionDto.Up);
                    break;
                case Keys.S:
                    _localController.SetPlayer1Direction(DirectionDto.Down);
                    break;
                case Keys.A:
                    _localController.SetPlayer1Direction(DirectionDto.Left);
                    break;
                case Keys.D:
                    _localController.SetPlayer1Direction(DirectionDto.Right);
                    break;
                    
                // Player 2 controls (Arrow Keys) - Only works in PvP mode
                case Keys.Up:
                    _localController.SetPlayer2Direction(DirectionDto.Up);
                    break;
                case Keys.Down:
                    _localController.SetPlayer2Direction(DirectionDto.Down);
                    break;
                case Keys.Left:
                    _localController.SetPlayer2Direction(DirectionDto.Left);
                    break;
                case Keys.Right:
                    _localController.SetPlayer2Direction(DirectionDto.Right);
                    break;
                    
                case Keys.Escape:
                    this.Close();
                    break;
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _renderTimer?.Stop();
            _renderTimer?.Dispose();
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
