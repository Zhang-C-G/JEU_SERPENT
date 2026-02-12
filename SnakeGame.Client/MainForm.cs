using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using SnakeGame.Shared;

namespace SnakeGame.Client
{
    public class MainForm : Form
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
            this.Text = "SNAKE MULTIJOUEUR";
            this.ClientSize = new Size(800, 700);
            this.DoubleBuffered = true;
            this.KeyDown += MainForm_KeyDown;
            this.FormClosing += MainForm_FormClosing;
            this.BackColor = Color.Black;
            
            // Panneau de contrôle
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 100;
            controlPanel.BackColor = Color.FromArgb(40, 40, 40);
            controlPanel.ForeColor = Color.White;
            
            // Nom
            Label nameLabel = new Label();
            nameLabel.Text = "Nom:";
            nameLabel.ForeColor = Color.White;
            nameLabel.Location = new Point(10, 15);
            nameLabel.Size = new Size(40, 20);
            
            nameTextBox = new TextBox();
            nameTextBox.Text = playerName;
            nameTextBox.Location = new Point(55, 12);
            nameTextBox.Size = new Size(120, 20);
            nameTextBox.BackColor = Color.FromArgb(60, 60, 60);
            nameTextBox.ForeColor = Color.White;
            nameTextBox.BorderStyle = BorderStyle.FixedSingle;
            
            // Observateur
            observerCheckBox = new CheckBox();
            observerCheckBox.Text = "Observateur";
            observerCheckBox.ForeColor = Color.White;
            observerCheckBox.Location = new Point(190, 12);
            observerCheckBox.Size = new Size(100, 20);
            
            // Bouton Connecter
            connectButton = new Button();
            connectButton.Text = "CONNECTER";
            connectButton.Location = new Point(300, 10);
            connectButton.Size = new Size(100, 30);
            connectButton.BackColor = Color.FromArgb(0, 120, 200);
            connectButton.ForeColor = Color.White;
            connectButton.FlatStyle = FlatStyle.Flat;
            connectButton.FlatAppearance.BorderSize = 0;
            connectButton.Click += ConnectButton_Click;
            
            // Status
            statusLabel = new Label();
            statusLabel.Text = "Déconnecté";
            statusLabel.ForeColor = Color.Red;
            statusLabel.Location = new Point(10, 45);
            statusLabel.Size = new Size(200, 20);
            statusLabel.Font = new Font("Arial", 9, FontStyle.Bold);
            
            // Score
            scoreLabel = new Label();
            scoreLabel.Text = "Score: 0";
            scoreLabel.ForeColor = Color.White;
            scoreLabel.Location = new Point(10, 70);
            scoreLabel.Size = new Size(150, 20);
            scoreLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            
            // Joueurs
            playersLabel = new Label();
            playersLabel.Text = "Joueurs: 0";
            playersLabel.ForeColor = Color.White;
            playersLabel.Location = new Point(150, 70);
            playersLabel.Size = new Size(200, 20);
            
            // Bouton Focus
            Button focusButton = new Button();
            focusButton.Text = "ACTIVER CONTROLES";
            focusButton.Location = new Point(420, 10);
            focusButton.Size = new Size(150, 30);
            focusButton.BackColor = Color.FromArgb(200, 120, 0);
            focusButton.ForeColor = Color.White;
            focusButton.FlatStyle = FlatStyle.Flat;
            focusButton.Click += (s, e) => { 
                this.Focus(); 
                statusLabel.Text = "Contrôles activés!";
                statusLabel.ForeColor = Color.Yellow;
            };
            
            // Instructions
            Label instrLabel = new Label();
            instrLabel.Text = "FLECHES: Déplacer | ESPACE: Pause | F2: Focus";
            instrLabel.ForeColor = Color.LightGray;
            instrLabel.Location = new Point(420, 45);
            instrLabel.Size = new Size(300, 20);
            
            controlPanel.Controls.Add(nameLabel);
            controlPanel.Controls.Add(nameTextBox);
            controlPanel.Controls.Add(observerCheckBox);
            controlPanel.Controls.Add(connectButton);
            controlPanel.Controls.Add(statusLabel);
            controlPanel.Controls.Add(scoreLabel);
            controlPanel.Controls.Add(playersLabel);
            controlPanel.Controls.Add(focusButton);
            controlPanel.Controls.Add(instrLabel);
            
            // Panneau du jeu
            gamePanel = new Panel();
            gamePanel.Dock = DockStyle.Fill;
            gamePanel.BackColor = Color.Black;
            gamePanel.Paint += GamePanel_Paint;
            gamePanel.Click += (s, e) => this.Focus();
            
            this.Controls.Add(controlPanel);
            this.Controls.Add(gamePanel);
            
            // Timer
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 50;
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
                Disconnect();
                return;
            }
            
            try
            {
                playerName = nameTextBox.Text;
                if (string.IsNullOrWhiteSpace(playerName)) playerName = "Joueur";
                
                isObserver = observerCheckBox.Checked;
                
                statusLabel.Text = "Connexion...";
                statusLabel.ForeColor = Color.Orange;
                connectButton.Enabled = false;
                
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 8888);
                
                string message = $"JOIN|{playerName}|{(isObserver ? "OBSERVER" : "PLAYER")}";
                byte[] data = Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                
                byte[] buffer = new byte[1024];
                int bytesRead = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                if (response.StartsWith("ID|"))
                {
                    playerId = response.Split('|')[1];
                    
                    statusLabel.Text = "CONNECTÉ !";
                    statusLabel.ForeColor = Color.Green;
                    connectButton.Text = "DÉCONNECTER";
                    connectButton.BackColor = Color.FromArgb(200, 0, 0);
                    connectButton.Enabled = true;
                    
                    gameTimer.Start();
                    
                    if (!isObserver)
                    {
                        this.Focus();
                        statusLabel.Text = "CONNECTÉ - Utilisez les FLECHES";
                    }
                    else
                    {
                        statusLabel.Text = "CONNECTÉ (Observateur)";
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Erreur: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
                connectButton.Enabled = true;
                tcpClient?.Close();
                tcpClient = null;
            }
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            
            try
            {
                if (tcpClient.GetStream().DataAvailable)
                {
                    byte[] buffer = new byte[50000];
                    int bytesRead = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                    string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    gameState = JsonSerializer.Deserialize<GameState>(json);
                    if (gameState != null && gameState.Rooms != null && gameState.Rooms.Count > 0)
                    {
                        UpdateUI();
                        gamePanel.Invalidate();
                    }
                }
            }
            catch { }
        }
        
        private void UpdateUI()
        {
            try
            {
                var currentRoom = gameState.Rooms.FirstOrDefault();
                if (currentRoom == null) return;
                
                var currentPlayer = currentRoom.Players.FirstOrDefault(p => p.Id == playerId);
                if (currentPlayer != null)
                {
                    scoreLabel.Text = $"Score: {currentPlayer.Score}";
                    scoreLabel.ForeColor = currentPlayer.IsInvincible ? Color.Gold : Color.White;
                }
                
                int playerCount = currentRoom.Players.Count(p => !p.IsObserver);
                int observerCount = currentRoom.Players.Count(p => p.IsObserver);
                playersLabel.Text = $"Joueurs: {playerCount} | Obs: {observerCount}";
            }
            catch { }
        }
        
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (gameState == null || gameState.Rooms == null || gameState.Rooms.Count == 0)
            {
                e.Graphics.Clear(Color.Black);
                string msg = "En attente du serveur...";
                Font font = new Font("Arial", 16, FontStyle.Bold);
                SizeF size = e.Graphics.MeasureString(msg, font);
                e.Graphics.DrawString(msg, font, Brushes.White, 
                    (gamePanel.Width - size.Width) / 2, 
                    (gamePanel.Height - size.Height) / 2);
                return;
            }
            
            var currentRoom = gameState.Rooms[0];
            if (currentRoom == null) return;
            
            using (Graphics g = Graphics.FromImage(gameBuffer))
            {
                g.Clear(Color.Black);
                
                // Grille
                using (Pen gridPen = new Pen(Color.FromArgb(30, 30, 30)))
                {
                    for (int x = 0; x < GridWidth; x++)
                        for (int y = 0; y < GridHeight; y++)
                            g.DrawRectangle(gridPen, x * TileSize, y * TileSize, TileSize, TileSize);
                }
                
                // Nourriture
                if (currentRoom.Food != null && currentRoom.Food.Exists)
                {
                    g.FillEllipse(Brushes.Red,
                        currentRoom.Food.Position.X * TileSize + 2,
                        currentRoom.Food.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                // Power-up
                if (currentRoom.PowerUp != null && currentRoom.PowerUp.Exists)
                {
                    g.FillEllipse(Brushes.Gold,
                        currentRoom.PowerUp.Position.X * TileSize + 2,
                        currentRoom.PowerUp.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                // Serpents
                Color[] colors = { Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Cyan, Color.Magenta };
                int colorIndex = 0;
                
                foreach (var player in currentRoom.Players)
                {
                    if (player.IsObserver || player.Snake == null || player.Snake.Count == 0) continue;
                    
                    Color playerColor = colors[colorIndex % colors.Length];
                    if (player.Id == playerId && player.IsInvincible)
                        playerColor = Color.White;
                    
                    using (Brush bodyBrush = new SolidBrush(playerColor))
                    {
                        for (int i = 0; i < player.Snake.Count; i++)
                        {
                            var segment = player.Snake[i];
                            if (segment.X < 0 || segment.Y < 0) continue;
                            
                            Brush brush = (i == 0) ? Brushes.Lime : bodyBrush;
                            g.FillRectangle(brush,
                                segment.X * TileSize + 2,
                                segment.Y * TileSize + 2,
                                TileSize - 4, TileSize - 4);
                        }
                    }
                    
                    // Nom du joueur
                    if (player.Snake.Count > 0)
                    {
                        var head = player.Snake[0];
                        g.DrawString(player.Name, new Font("Arial", 8, FontStyle.Bold), 
                            Brushes.White, head.X * TileSize, head.Y * TileSize - 15);
                    }
                    
                    colorIndex++;
                }
            }
            
            int xPos = (gamePanel.Width - gameBuffer.Width) / 2;
            int yPos = (gamePanel.Height - gameBuffer.Height) / 2;
            e.Graphics.DrawImage(gameBuffer, xPos, yPos);
        }
        
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (isObserver)
            {
                statusLabel.Text = "Mode observateur - pas de contrôle";
                return;
            }
            
            if (tcpClient == null || !tcpClient.Connected || string.IsNullOrEmpty(playerId))
            {
                statusLabel.Text = "Connectez-vous d'abord!";
                return;
            }
            
            string direction = "";
            switch (e.KeyCode)
            {
                case Keys.Up: case Keys.W: direction = "Up"; break;
                case Keys.Down: case Keys.S: direction = "Down"; break;
                case Keys.Left: case Keys.A: direction = "Left"; break;
                case Keys.Right: case Keys.D: direction = "Right"; break;
                case Keys.Space: 
                    if (gameTimer.Enabled) gameTimer.Stop();
                    else gameTimer.Start();
                    break;
                case Keys.F2: 
                    this.Focus();
                    statusLabel.Text = "Focus activé!";
                    statusLabel.ForeColor = Color.Yellow;
                    break;
            }
            
            if (!string.IsNullOrEmpty(direction))
            {
                try
                {
                    string message = $"MOVE|{playerId}|{direction}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    tcpClient.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
            
            e.Handled = true;
            e.SuppressKeyPress = true;
        }
        
        private void Disconnect()
        {
            gameTimer?.Stop();
            tcpClient?.Close();
            tcpClient = null;
            playerId = "";
            statusLabel.Text = "Déconnecté";
            statusLabel.ForeColor = Color.Red;
            connectButton.Text = "CONNECTER";
            connectButton.BackColor = Color.FromArgb(0, 120, 200);
            scoreLabel.Text = "Score: 0";
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
