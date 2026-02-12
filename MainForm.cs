using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows.Forms;
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
        
        private Panel gamePanel;
        private Panel controlPanel;
        private TextBox nameTextBox;
        private CheckBox observerCheckBox;
        private Button connectButton;
        private Label statusLabel;
        private Label scoreLabel;
        private Label playersLabel;
        private ComboBox roomComboBox;
        private Button refreshRoomsButton;
        private Button createRoomButton;
        private TextBox createRoomTextBox;
        
        public MainForm()
        {
            InitializeComponent();
            InitializeGame();
            this.KeyPreview = true;
        }
        
        private void InitializeComponent()
        {
            this.Text = "Snake Multijoueur - Salons";
            this.ClientSize = new Size(800, 700);
            this.DoubleBuffered = true;
            this.KeyDown += MainForm_KeyDown;
            this.FormClosing += MainForm_FormClosing;
            
            controlPanel = new Panel();
            controlPanel.Dock = DockStyle.Top;
            controlPanel.Height = 180;
            controlPanel.BackColor = Color.LightGray;
            controlPanel.BorderStyle = BorderStyle.FixedSingle;
            
            int y = 10;
            
            Label nameLabel = new Label();
            nameLabel.Text = "Nom:";
            nameLabel.Location = new Point(10, y + 5);
            nameLabel.Size = new Size(40, 20);
            
            nameTextBox = new TextBox();
            nameTextBox.Text = playerName;
            nameTextBox.Location = new Point(55, y + 2);
            nameTextBox.Size = new Size(150, 20);
            
            observerCheckBox = new CheckBox();
            observerCheckBox.Text = "Mode Observateur";
            observerCheckBox.Location = new Point(220, y + 2);
            observerCheckBox.Size = new Size(150, 20);
            
            connectButton = new Button();
            connectButton.Text = "Se connecter";
            connectButton.Location = new Point(380, y);
            connectButton.Size = new Size(120, 25);
            connectButton.BackColor = Color.LightGreen;
            connectButton.Click += ConnectButton_Click;
            
            y += 35;
            
            statusLabel = new Label();
            statusLabel.Text = "Déconnecté";
            statusLabel.Location = new Point(10, y);
            statusLabel.Size = new Size(400, 20);
            statusLabel.Font = new Font("Arial", 10, FontStyle.Bold);
            statusLabel.ForeColor = Color.Red;
            
            y += 30;
            
            Label roomLabel = new Label();
            roomLabel.Text = "Salon:";
            roomLabel.Location = new Point(10, y + 3);
            roomLabel.Size = new Size(50, 20);
            
            roomComboBox = new ComboBox();
            roomComboBox.Location = new Point(65, y);
            roomComboBox.Size = new Size(200, 25);
            roomComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            roomComboBox.Enabled = false;
            
            refreshRoomsButton = new Button();
            refreshRoomsButton.Text = "Rafraîchir";
            refreshRoomsButton.Location = new Point(275, y);
            refreshRoomsButton.Size = new Size(80, 25);
            refreshRoomsButton.Click += RefreshRoomsButton_Click;
            refreshRoomsButton.Enabled = false;
            
            y += 35;
            
            Label createLabel = new Label();
            createLabel.Text = "Nouveau salon:";
            createLabel.Location = new Point(10, y + 3);
            createLabel.Size = new Size(90, 20);
            
            createRoomTextBox = new TextBox();
            createRoomTextBox.Location = new Point(105, y);
            createRoomTextBox.Size = new Size(160, 20);
            createRoomTextBox.Text = "Salon " + DateTime.Now.ToString("HHmmss");
            
            createRoomButton = new Button();
            createRoomButton.Text = "Créer";
            createRoomButton.Location = new Point(275, y);
            createRoomButton.Size = new Size(80, 25);
            createRoomButton.Click += CreateRoomButton_Click;
            createRoomButton.Enabled = false;
            
            y += 35;
            
            scoreLabel = new Label();
            scoreLabel.Text = "Score: 0";
            scoreLabel.Location = new Point(10, y);
            scoreLabel.Size = new Size(200, 20);
            scoreLabel.Font = new Font("Arial", 12, FontStyle.Bold);
            
            playersLabel = new Label();
            playersLabel.Text = "Joueurs: 0 | Observateurs: 0";
            playersLabel.Location = new Point(220, y);
            playersLabel.Size = new Size(300, 20);
            
            y += 25;
            
            Button focusButton = new Button();
            focusButton.Text = "Activer Contrôles (F2)";
            focusButton.Location = new Point(10, y);
            focusButton.Size = new Size(200, 25);
            focusButton.BackColor = Color.Yellow;
            focusButton.Click += (s, e) => { this.Focus(); statusLabel.Text = "Contrôles activés!"; };
            
            Label instructionsLabel = new Label();
            instructionsLabel.Text = "FLÈCHES: Jouer | ESPACE: Pause | F2: Focus | ÉCHAP: Quitter";
            instructionsLabel.Location = new Point(380, y + 3);
            instructionsLabel.Size = new Size(350, 20);
            instructionsLabel.Font = new Font("Arial", 9);
            
            controlPanel.Controls.AddRange(new Control[] {
                nameLabel, nameTextBox, observerCheckBox, connectButton,
                statusLabel,
                roomLabel, roomComboBox, refreshRoomsButton,
                createLabel, createRoomTextBox, createRoomButton,
                scoreLabel, playersLabel,
                focusButton, instructionsLabel
            });
            
            gamePanel = new Panel();
            gamePanel.Dock = DockStyle.Fill;
            gamePanel.BackColor = Color.Black;
            gamePanel.Paint += GamePanel_Paint;
            
            this.Controls.Add(controlPanel);
            this.Controls.Add(gamePanel);
            
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
                isObserver = observerCheckBox.Checked;
                
                statusLabel.Text = "Connexion au serveur...";
                statusLabel.ForeColor = Color.Orange;
                
                tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 8888);
                
                string selectedRoomId = "";
                if (roomComboBox.SelectedValue != null)
                {
                    selectedRoomId = roomComboBox.SelectedValue.ToString();
                }
                
                string message = "JOIN|" + playerName + "|" + (isObserver ? "OBSERVER" : "PLAYER") + "|" + selectedRoomId;
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                
                byte[] buffer = new byte[10000];
                int bytesRead = await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);
                string response = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                string[] parts = response.Split('|');
                if (parts[0] == "ID")
                {
                    playerId = parts[1];
                    
                    if (parts.Length > 3 && parts[2] == "ROOMS")
                    {
                        UpdateRoomList(parts[3]);
                    }
                    
                    statusLabel.Text = "Connecté - " + playerName;
                    statusLabel.ForeColor = Color.Green;
                    
                    connectButton.Text = "Déconnecter";
                    connectButton.BackColor = Color.LightCoral;
                    
                    roomComboBox.Enabled = true;
                    refreshRoomsButton.Enabled = true;
                    createRoomButton.Enabled = true;
                    nameTextBox.Enabled = false;
                    observerCheckBox.Enabled = false;
                    
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
                statusLabel.Text = "Erreur: " + ex.Message;
                statusLabel.ForeColor = Color.Red;
                if (tcpClient != null)
                {
                    tcpClient.Close();
                    tcpClient = null;
                }
            }
        }
        
        private async void RefreshRoomsButton_Click(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            
            try
            {
                string message = "GET_ROOMS|";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
            }
            catch { }
        }
        
        private async void CreateRoomButton_Click(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            
            try
            {
                string roomName = createRoomTextBox.Text;
                if (string.IsNullOrWhiteSpace(roomName))
                {
                    roomName = "Salon " + DateTime.Now.ToString("HHmmss");
                }
                
                string message = "CREATE_ROOM|" + roomName;
                byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
                await tcpClient.GetStream().WriteAsync(data, 0, data.Length);
                
                createRoomTextBox.Text = "Salon " + DateTime.Now.ToString("HHmmss");
            }
            catch { }
        }
        
        private void UpdateRoomList(string roomsJson)
        {
            try
            {
                var rooms = JsonSerializer.Deserialize<List<RoomInfo>>(roomsJson);
                if (rooms != null)
                {
                    roomComboBox.DataSource = rooms;
                    roomComboBox.DisplayMember = "DisplayName";
                    roomComboBox.ValueMember = "Id";
                }
            }
            catch { }
        }
        
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (tcpClient == null || !tcpClient.Connected) return;
            
            try
            {
                var stream = tcpClient.GetStream();
                if (stream.DataAvailable)
                {
                    byte[] buffer = new byte[50000];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string json = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    
                    if (json.StartsWith("ROOMS_LIST|"))
                    {
                        UpdateRoomList(json.Substring(11));
                    }
                    else
                    {
                        gameState = JsonSerializer.Deserialize<GameState>(json);
                        UpdateUI();
                        gamePanel.Invalidate();
                    }
                }
            }
            catch
            {
                Disconnect();
            }
        }
        
        private void UpdateUI()
        {
            if (gameState == null || gameState.Rooms == null) return;
            
            var currentRoom = gameState.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
            if (currentRoom != null)
            {
                var currentPlayer = currentRoom.Players.FirstOrDefault(p => p.Id == playerId);
                if (currentPlayer != null)
                {
                    scoreLabel.Text = "Score: " + currentPlayer.Score;
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
                
                int playerCount = currentRoom.Players.Count(p => !p.IsObserver);
                int observerCount = currentRoom.Players.Count(p => p.IsObserver);
                playersLabel.Text = "Joueurs: " + playerCount + " | Observateurs: " + observerCount;
            }
        }
        
        private void GamePanel_Paint(object sender, PaintEventArgs e)
        {
            if (gameState == null) return;
            
            var currentRoom = gameState.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
            if (currentRoom == null) return;
            
            using (Graphics g = Graphics.FromImage(gameBuffer))
            {
                g.Clear(Color.Black);
                
                for (int x = 0; x < GridWidth; x++)
                {
                    for (int y = 0; y < GridHeight; y++)
                    {
                        g.DrawRectangle(Pens.DarkGray, x * TileSize, y * TileSize, TileSize, TileSize);
                    }
                }
                
                if (currentRoom.Food != null && currentRoom.Food.Exists)
                {
                    g.FillEllipse(Brushes.Red,
                        currentRoom.Food.Position.X * TileSize + 2,
                        currentRoom.Food.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                if (currentRoom.PowerUp != null && currentRoom.PowerUp.Exists)
                {
                    g.FillEllipse(Brushes.Gold,
                        currentRoom.PowerUp.Position.X * TileSize + 2,
                        currentRoom.PowerUp.Position.Y * TileSize + 2,
                        TileSize - 4, TileSize - 4);
                }
                
                Color[] playerColors = { Color.Green, Color.Blue, Color.Orange, Color.Purple, Color.Cyan, Color.Magenta };
                int colorIndex = 0;
                
                foreach (var player in currentRoom.Players)
                {
                    if (player.IsObserver || player.Snake == null || player.Snake.Count == 0) continue;
                    
                    Color snakeColor = playerColors[colorIndex % playerColors.Length];
                    
                    if (player.Id == playerId && player.IsInvincible)
                    {
                        snakeColor = Color.White;
                    }
                    
                    using (Brush snakeBrush = new SolidBrush(snakeColor))
                    {
                        for (int i = 0; i < player.Snake.Count; i++)
                        {
                            var segment = player.Snake[i];
                            if (segment.X < 0 || segment.Y < 0) continue;
                            
                            Brush segmentBrush = (i == 0) ? Brushes.Lime : snakeBrush;
                            
                            g.FillRectangle(segmentBrush,
                                segment.X * TileSize + 1,
                                segment.Y * TileSize + 1,
                                TileSize - 2, TileSize - 2);
                        }
                    }
                    
                    if (player.Snake.Count > 0)
                    {
                        var head = player.Snake[0];
                        g.DrawString(player.Name, this.Font, Brushes.White,
                            head.X * TileSize, head.Y * TileSize - 15);
                    }
                    
                    colorIndex++;
                }
            }
            
            int xPos = (gamePanel.Width - gameBuffer.Width) / 2;
            int yPos = (gamePanel.Height - gameBuffer.Height) / 2;
            e.Graphics.DrawImage(gameBuffer, xPos, yPos);
            
            if (currentRoom.Players.Count == 0)
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
                    string message = "MOVE|" + playerId + "|" + direction;
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
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }
            playerId = "";
            
            statusLabel.Text = "Déconnecté";
            statusLabel.ForeColor = Color.Red;
            scoreLabel.Text = "Score: 0";
            
            connectButton.Text = "Se connecter";
            connectButton.BackColor = Color.LightGreen;
            
            roomComboBox.Enabled = false;
            refreshRoomsButton.Enabled = false;
            createRoomButton.Enabled = false;
            nameTextBox.Enabled = true;
            observerCheckBox.Enabled = true;
        }
        
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Disconnect();
        }
    }
    
    public class RoomInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int PlayerCount { get; set; }
        public int ObserverCount { get; set; }
        
        public string DisplayName
        {
            get { return Name + " (" + PlayerCount + "/" + MaxPlayers + ")"; }
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
