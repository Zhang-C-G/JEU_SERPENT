using System.Net.Sockets;
using System.Text.Json;
using SnakeGame.Shared;

namespace SnakeGame.Client
{
    public partial class MainForm : Form
    {
        private TcpClient tcpClient;
        private string playerId = "";
        private string playerName = "Joueur";
        private bool isObserver = false;
        private GameState gameState = new GameState();
        private System.Windows.Forms.Timer gameTimer;
        private Bitmap gameBuffer;
        private const int TileSize = 20;
        private const int GridWidth = 30;
        private const int GridHeight = 20;
        
        // UI Controls
        private Panel gamePanel;
        private Panel controlPanel;
        private TextBox nameTextBox;
        private CheckBox observerCheckBox;
        private Button connectButton;
        private Label statusLabel;
        private Label scoreLabel;
        private Label playersLabel;
        
        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
            this.KeyPreview = true;
        }
        
        private void InitializeComponent()
        {
            this.Text = "Snake Multijoueur";
            this.ClientSize = new Size(800, 650);
            this.DoubleBuffered = true;
            this.KeyDown += MainForm_KeyDown;
            this.FormClosing += MainForm_FormClosing;
            
            // Control Panel (en haut)
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 120;
            controlPanel.BackColor = Color.LightGray;
            controlPanel.BorderStyle = BorderStyle.FixedSingle;
            
            // Name label and textbox
            var nameLabel = new Label();
            nameLabel.Text = "Nom:";
            nameLabel.Location = new Point(10, 15);
            nameLabel.Size = new Size(40, 20);
            
            nameTextBox = new TextBox();
            nameTextBox.Text = playerName;
            nameTextBox.Location = new Point(55, 12);
            nameTextBox.Size = new Size(150, 20);
            
            // Observer checkbox
            observerCheckBox = new CheckBox();
            observerCheckBox.Text = "Mode Observateur";
            observerCheckBox.Location = new Point(220, 12);
            observerCheckBox.Size = new Size(150, 20);
            
            // Connect button
            connectButton = new Button();
            connectButton.Text = "Se connecter";
            connectButton.Location = new Point(380, 10);
            connectButton.Size = new Size(120, 25);
            connectButton.BackColor = Color.LightGreen;
            connectButton.Click += ConnectButton_Click;
            
            // Status label
            statusLabel = new Label();
            statusLabel.Text = "Déconnecté";
            statusLabel.Location = new Point(10, 45);
            statusLabel.Size = new Size(400, 20);
            statusLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            statusLabel.ForeColor = Color.Red;
            
            // Score label
            scoreLabel = new Label();
            scoreLabel.Text = "Score: 0";
            scoreLabel.Location = new Point(10, 70);
            scoreLabel.Size = new Size(200, 20);
            scoreLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            
            // Players label
            playersLabel = new Label();
            playersLabel.Text = "Joueurs: 0";
            playersLabel.Location = new Point(10, 95);
            playersLabel.Size = new Size(300, 20);
            
            // Instructions
            var instructionsLabel = new Label();
            instructionsLabel.Text = "Contrôles: FLÈCHES pour jouer | ESPACE: pause | F2: focus";
            instructionsLabel.Location = new Point(450, 45);
            instructionsLabel.Size = new Size(300, 40);
            instructionsLabel.Font = new Font("Arial", 9);
            
            // Focus button
            var focusButton = new Button();
            focusButton.Text = "Activer Contrôles";
            focusButton.Location = new Point(510, 10);
            focusButton.Size = new Size(120, 25);
            focusButton.BackColor = Color.Yellow;
            focusButton.Click += (s, e) => { this.Focus(); statusLabel.Text = "Contrôles activés!"; };
            
            controlPanel.Controls.AddRange(new Control[] {
                nameLabel, nameTextBox, observerCheckBox, connectButton,
                statusLabel, scoreLabel, playersLabel, instructionsLabel, focusButton
            });
            
            // Game Panel (centre)
            gamePanel = new Panel();
            gamePanel.Dock = DockStyle.Fill;
            gamePanel.BackColor = Color.Black;
            gamePanel.Paint += GamePanel_Paint;
            
            this.Controls.Add(controlPanel);
            this.Controls.Add(gamePanel);
            
            // Game timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 50; // 20 FPS
            gameTimer.Tick += GameTimer_Tick;
        }
        
        private void InitializeGame()
        {
            gameBuffer = new Bitmap(GridWidth * TileSize, GridHeight * TileSize);
        }
        
        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                MessageBox.Show("Déjà connecté!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            try
            {
                playerName = nameTextBox.Text;
                isObserver = observerCheckBox.Checked;
                
                statusLabel.Text = "Connexion au serveur...";
                statusLabel.ForeColor = Color.Orange;
                
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 8888);
                
                string message = $"JOIN|{playerName}|{(isObserver ? "OBSERVER" : "PLAYER")}";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                
                // Recevoir l'ID du joueur
                byte[] buffer = new byte[1024];
                int bytesRead = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
                string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                if (response.StartsWith("ID|"))
                {
                    playerId = response.Split('|')[1];
                    statusLabel.Text = $"Connecté en tant que {playerName}";
                    statusLabel.ForeColor = Color.Green;
                    
                    if (!isObserver)
                    {
                        this.Focus();
                        statusLabel.Text += " - Utilisez les FLÈCHES";
                    }
                    else
                    {
                        statusLabel.Text += " (Observateur)";
                    }
                    
                    gameTimer.Start();
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Erreur: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                tcpClient?.Close();
                tcpClient = null;
            }
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            
            try
            {
                var stream = tcpClient.GetStream();
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[10000];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string json = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    gameState = JsonSerializer.Deserialize<GameState>(json);
                    UpdateUI();
                    gamePanel.Invalidate();
                }
            }
            catch
            {
                Disconnect();
            }
        }
        
        private void UpdateUI()
        {
            if (gameState == null || gameState.Players == null) return;
            
            // Mettre à jour le score
            var currentPlayer = gameState.Players.FirstOrDefault(p => p.Id == playerId);
            if (currentPlayer != null)
            {
                scoreLabel.Text = $"Score: {currentPlayer.Score}";
                if (currentPlayer.IsInvincible)
                {
                    scoreLabel.ForeColor = Color.Gold;
                    scoreLabel.Font = new Font("Arial", 12, FontStyle.Bold);
                }
                else
                {
                    scoreLabel.ForeColor = Color.Black;
                    scoreLabel.Font = new Font("Arial", 12, FontStyle.Regular);
                }
            }
            
            // Mettre à jour la liste des joueurs
            int playerCount = gameState.Players.Count(p => !p.IsObserver);
            int observerCount = gameState.Players.Count(p => p.IsObserver);
            playersLabel.Text = $"Joueurs: {playerCount} | Observateurs: {observerCount}";
        }
        
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (gameState == null) return;
            
            using (Graphics g = Graphics.FromImage(gameBuffer))
            {
                // Fond
                g.Clear(Color.Black);
                
                // Dessiner la grille
                for (int x = 0; x < GridWidth; x++)
                {
                    for (int y = 0; y < GridHeight; y++)
                    {
                        g.DrawRectangle(Pens.DarkGray, x * TileSize, y * TileSize, TileSize, TileSize);
                    }
                }
                
                // Dessiner la nourriture
                if (gameState.Food != null && gameState.Food.Exists)
                {
                    g.FillEllipse(Brushes.Red,
                        gameState.Food.Position.X * TileSize + 2,
                        gameState.Food.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                // Dessiner le power-up
                if (gameState.PowerUp != null && gameState.PowerUp.Exists)
                {
                    g.FillEllipse(Brushes.Gold,
                        gameState.PowerUp.Position.X * TileSize + 2,
                        gameState.PowerUp.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                // Dessiner les serpents
                Color[] playerColors = { Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Cyan, Color.Magenta };
                int colorIndex = 0;
                
                foreach (var player in gameState.Players)
                {
                    if (player.IsObserver || player.Snake == null || player.Snake.Count == 0) continue;
                    
                    Color snakeColor = playerColors[colorIndex % playerColors.Length];
                    
                    // Si c'est le joueur actuel et invincible
                    if (player.Id == playerId && player.IsInvincible)
                    {
                        snakeColor = Color.White;
                    }
                    
                    using (Brush snakeBrush = new SolidBrush(snakeColor))
                    {
                        for (int i = 0; i < player.Snake.Count; i++)
                        {
                            var segment = player.Snake[i];
                            
                            // Ignorer les segments hors écran
                            if (segment.X < 0 || segment.Y < 0) continue;
                            
                            Brush segmentBrush = (i == 0) ? Brushes.Lime : snakeBrush;
                            
                            g.FillRectangle(segmentBrush,
                                segment.X * TileSize + 1,
                                segment.Y * TileSize + 1,
                                TileSize - 2, TileSize - 2);
                        }
                    }
                    
                    // Dessiner le nom du joueur près de la tête
                    if (player.Snake.Count > 0)
                    {
                        var head = player.Snake[0];
                        g.DrawString(player.Name, this.Font, Brushes.White,
                            head.X * TileSize, head.Y * TileSize - 15);
                    }
                    
                    colorIndex++;
                }
            }
            
            // Centrer le buffer dans le panel
            int xPos = (gamePanel.Width - gameBuffer.Width) / 2;
            int yPos = (gamePanel.Height - gameBuffer.Height) / 2;
            e.Graphics.DrawImage(gameBuffer, xPos, yPos);
            
            // Afficher des informations supplémentaires
            if (gameState.Players.Count == 0)
            {
                string message = "En attente de joueurs...";
                Font font = new Font("Arial", 16, FontStyle.Bold);
                SizeF size = e.Graphics.MeasureString(message, font);
                e.Graphics.DrawString(message, font, Brushes.White,
                    (gamePanel.Width - size.Width) / 2,
                    (gamePanel.Height - size.Height) / 2);
            }
        }
        
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (isObserver || tcpClient == null || !tcpClient.Connected || string.IsNullOrEmpty(playerId))
                return;
            
            string direction = "";
            
            switch (e.KeyCode)
            {
                case Keys.Up:
                case Keys.W:
                    direction = "Up";
                    break;
                case Keys.Down:
                case Keys.S:
                    direction = "Down";
                    break;
                case Keys.Left:
                case Keys.A:
                    direction = "Left";
                    break;
                case Keys.Right:
                case Keys.D:
                    direction = "Right";
                    break;
                case Keys.Space:
                    if (gameTimer.Enabled)
                        gameTimer.Stop();
                    else
                        gameTimer.Start();
                    break;
                case Keys.F2:
                    this.Focus();
                    statusLabel.Text = "Focus activé - Utilisez les FLÈCHES";
                    break;
                case Keys.Escape:
                    this.Close();
                    break;
            }
            
            if (!string.IsNullOrEmpty(direction))
            {
                try
                {
                    string message = $"MOVE|{playerId}|{direction}";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                    tcpClient.GetStream().Write(data, 0, data.Length);
                }
                catch
                {
                    Disconnect();
                }
            }
            
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        
        private void Disconnect()
        {
            gameTimer?.Stop();
            tcpClient?.Close();
            statusLabel.Text = "Déconnecté";
            statusLabel.ForeColor = Color.Red;
            scoreLabel.Text = "Score: 0";
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
}
