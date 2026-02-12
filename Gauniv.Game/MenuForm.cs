namespace Gauniv.Game
{
    /// <summary>
    /// Main menu form for mode selection
    /// </summary>
    public partial class MenuForm : Form
    {
        public MenuForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Snake Duel - 贪吃蛇对决";
            this.ClientSize = new Size(400, 300);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Title label
            var titleLabel = new Label
            {
                Text = "🐍 Snake Duel",
                Font = new Font("Arial", 24, FontStyle.Bold),
                ForeColor = Color.DarkGreen,
                AutoSize = false,
                Size = new Size(380, 60),
                Location = new Point(10, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(titleLabel);
            
            // Subtitle
            var subtitleLabel = new Label
            {
                Text = "双人对战贪吃蛇",
                Font = new Font("Arial", 12),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(380, 30),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(subtitleLabel);
            
            // AI Training button
            var aiButton = new Button
            {
                Text = "🤖 AI训练模式\n(单人 vs 渐进AI)",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(280, 60),
                Location = new Point(60, 130),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            aiButton.FlatAppearance.BorderSize = 0;
            aiButton.Click += (s, e) => StartGame(GameMode.AITraining);
            this.Controls.Add(aiButton);
            
            // PvP button
            var pvpButton = new Button
            {
                Text = "⚔️ 双人对战模式\n(P1: WASD | P2: ↑↓←→)",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Size = new Size(280, 60),
                Location = new Point(60, 200),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            pvpButton.FlatAppearance.BorderSize = 0;
            pvpButton.Click += (s, e) => StartGame(GameMode.PvP);
            this.Controls.Add(pvpButton);
        }
        
        private void StartGame(GameMode mode)
        {
            this.Hide();
            var gameForm = new GameForm(mode);
            gameForm.FormClosed += (s, e) => this.Close();
            gameForm.Show();
        }
        
        public enum GameMode
        {
            AITraining,
            PvP
        }
    }
}
