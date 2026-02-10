using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using SnakeGame.Shared;

namespace SnakeGame.Server
{
    class Program
    {
        static TcpListener? server;
        static GameState gameState = new GameState();
        static List<ClientHandler> clients = new List<ClientHandler>();
        static Random random = new Random();
        static object gameLock = new object();
        static int gridWidth = 30;
        static int gridHeight = 20;
        
        static void Main()
        {
            Console.Title = "Snake Server";
            Console.WriteLine("=== SERVEUR SNAKE MULTIJOUEUR ===");
            Console.WriteLine("Initialisation...");
            
            InitializeGame();
            StartServer();
        }
        
        static void InitializeGame()
        {
            lock (gameLock)
            {
                gameState.Players.Clear();
                GenerateFood();
            }
        }
        
        static void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();
                
                Console.WriteLine($"Serveur démarré sur le port 8888");
                Console.WriteLine($"Taille de grille: {gridWidth}x{gridHeight}");
                Console.WriteLine("En attente de joueurs...");
                Console.WriteLine("Tapez 'exit' pour quitter");
                
                // Thread pour accepter les clients
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.Start();
                
                // Thread pour la boucle de jeu
                Thread gameThread = new Thread(GameLoop);
                gameThread.Start();
                
                // Gérer la commande exit
                while (Console.ReadLine()?.ToLower() != "exit") { }
                
                server.Stop();
                Console.WriteLine("Serveur arrêté.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur serveur: {ex.Message}");
            }
        }
        
        static void AcceptClients()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server.AcceptTcpClient();
                    ClientHandler handler = new ClientHandler(client);
                    lock (gameLock)
                    {
                        clients.Add(handler);
                    }
                    Thread clientThread = new Thread(handler.HandleClient);
                    clientThread.Start();
                    Console.WriteLine($"Nouveau client connecté ({clients.Count} total)");
                }
            }
            catch { }
        }
        
        static void GameLoop()
        {
            while (true)
            {
                try
                {
                    UpdateGame();
                    BroadcastGameState();
                    Thread.Sleep(100); // 10 FPS
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur boucle de jeu: {ex.Message}");
                }
            }
        }
        
        static void UpdateGame()
        {
            lock (gameLock)
            {
                // Mettre à jour chaque joueur
                foreach (var player in gameState.Players)
                {
                    if (player.IsObserver || player.Snake.Count == 0) continue;
                    
                    // Vérifier expiration invincibilité
                    if (player.IsInvincible && DateTime.Now > player.InvincibleUntil)
                    {
                        player.IsInvincible = false;
                    }
                    
                    // Déplacer le serpent
                    MovePlayer(player);
                    
                    // Vérifier collisions
                    CheckCollisions(player);
                    
                    // Vérifier nourriture
                    CheckFood(player);
                    
                    // Vérifier power-up
                    CheckPowerUp(player);
                }
                
                // Générer aléatoirement un power-up
                if (!gameState.PowerUp.Exists && random.Next(0, 100) < 5) // 5% chance
                {
                    GeneratePowerUp();
                }
            }
        }
        
        static void MovePlayer(Player player)
        {
            var head = player.Snake[0];
            var newHead = new Position { X = head.X, Y = head.Y };
            
            switch (player.Direction)
            {
                case "Up": newHead.Y--; break;
                case "Down": newHead.Y++; break;
                case "Left": newHead.X--; break;
                case "Right": newHead.X++; break;
            }
            
            // Warp autour des bords
            if (newHead.X < 0) newHead.X = gridWidth - 1;
            if (newHead.X >= gridWidth) newHead.X = 0;
            if (newHead.Y < 0) newHead.Y = gridHeight - 1;
            if (newHead.Y >= gridHeight) newHead.Y = 0;
            
            player.Snake.Insert(0, newHead);
            player.Snake.RemoveAt(player.Snake.Count - 1);
        }
        
        static void CheckCollisions(Player player)
        {
            if (player.IsInvincible) return;
            
            var head = player.Snake[0];
            
            // Collision avec soi-même
            for (int i = 1; i < player.Snake.Count; i++)
            {
                if (player.Snake[i].X == head.X && player.Snake[i].Y == head.Y)
                {
                    ResetPlayer(player);
                    return;
                }
            }
            
            // Collision avec d'autres serpents
            foreach (var otherPlayer in gameState.Players)
            {
                if (otherPlayer.Id == player.Id || otherPlayer.IsObserver) continue;
                
                foreach (var segment in otherPlayer.Snake)
                {
                    if (segment.X == head.X && segment.Y == head.Y)
                    {
                        ResetPlayer(player);
                        return;
                    }
                }
            }
        }
        
        static void CheckFood(Player player)
        {
            var head = player.Snake[0];
            
            if (head.X == gameState.Food.Position.X && head.Y == gameState.Food.Position.Y)
            {
                player.Score += 10;
                player.Snake.Add(new Position { X = -1, Y = -1 }); // Agrandir le serpent
                GenerateFood();
            }
        }
        
        static void CheckPowerUp(Player player)
        {
            if (gameState.PowerUp.Exists)
            {
                var head = player.Snake[0];
                
                if (head.X == gameState.PowerUp.Position.X && head.Y == gameState.PowerUp.Position.Y)
                {
                    player.Score += 50;
                    player.IsInvincible = true;
                    player.InvincibleUntil = DateTime.Now.AddSeconds(3);
                    gameState.PowerUp.Exists = false;
                }
            }
        }
        
        static void ResetPlayer(Player player)
        {
            player.Snake.Clear();
            player.Snake.Add(new Position { X = random.Next(5, 15), Y = random.Next(5, 15) });
            player.Snake.Add(new Position { X = player.Snake[0].X - 1, Y = player.Snake[0].Y });
            player.Snake.Add(new Position { X = player.Snake[0].X - 2, Y = player.Snake[0].Y });
            player.Direction = "Right";
        }
        
        static void GenerateFood()
        {
            bool valid;
            do
            {
                valid = true;
                gameState.Food.Position = new Position
                {
                    X = random.Next(0, gridWidth),
                    Y = random.Next(0, gridHeight)
                };
                
                // Vérifier que la nourriture n'est pas sur un serpent
                foreach (var player in gameState.Players)
                {
                    foreach (var segment in player.Snake)
                    {
                        if (segment.Equals(gameState.Food.Position))
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }
            } while (!valid);
            
            gameState.Food.Exists = true;
        }
        
        static void GeneratePowerUp()
        {
            bool valid;
            do
            {
                valid = true;
                gameState.PowerUp.Position = new Position
                {
                    X = random.Next(0, gridWidth),
                    Y = random.Next(0, gridHeight)
                };
                
                // Ne pas générer sur la nourriture
                if (gameState.PowerUp.Position.Equals(gameState.Food.Position))
                {
                    valid = false;
                    continue;
                }
                
                // Ne pas générer sur un serpent
                foreach (var player in gameState.Players)
                {
                    foreach (var segment in player.Snake)
                    {
                        if (segment.Equals(gameState.PowerUp.Position))
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }
            } while (!valid);
            
            gameState.PowerUp.Exists = true;
            Console.WriteLine($"Power-up généré à ({gameState.PowerUp.Position.X}, {gameState.PowerUp.Position.Y})");
        }
        
        static void BroadcastGameState()
        {
            try
            {
                byte[] gameData;
                lock (gameLock)
                {
                    var json = JsonSerializer.Serialize(gameState);
                    gameData = System.Text.Encoding.UTF8.GetBytes(json);
                }
                
                List<ClientHandler> clientsToRemove = new List<ClientHandler>();
                
                foreach (var client in clients)
                {
                    if (!client.SendGameData(gameData))
                    {
                        clientsToRemove.Add(client);
                    }
                }
                
                // Supprimer les clients déconnectés
                if (clientsToRemove.Count > 0)
                {
                    lock (gameLock)
                    {
                        foreach (var client in clientsToRemove)
                        {
                            clients.Remove(client);
                            if (client.Player != null)
                            {
                                gameState.Players.Remove(client.Player);
                                Console.WriteLine($"Joueur déconnecté: {client.Player.Name}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur diffusion: {ex.Message}");
            }
        }
        
        class ClientHandler
        {
            private TcpClient client;
            private NetworkStream stream;
            public Player? Player { get; private set; }
            
            public ClientHandler(TcpClient client)
            {
                this.client = client;
                this.stream = client.GetStream();
            }
            
            public void HandleClient()
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    
                    while (client.Connected)
                    {
                        if (stream.DataAvailable)
                        {
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) break;
                            
                            string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead);
                            ProcessMessage(message);
                        }
                        Thread.Sleep(10);
                    }
                }
                catch { }
                finally
                {
                    Disconnect();
                }
            }
            
            void ProcessMessage(string message)
            {
                try
                {
                    var parts = message.Split('|');
                    
                    if (parts[0] == "JOIN")
                    {
                        string playerName = parts[1];
                        bool isObserver = parts[2] == "OBSERVER";
                        
                        Player = new Player
                        {
                            Name = playerName,
                            IsObserver = isObserver
                        };
                        
                        if (!isObserver)
                        {
                            Player.Snake.Add(new Position { X = random.Next(5, 15), Y = random.Next(5, 15) });
                            Player.Snake.Add(new Position { X = Player.Snake[0].X - 1, Y = Player.Snake[0].Y });
                            Player.Snake.Add(new Position { X = Player.Snake[0].X - 2, Y = Player.Snake[0].Y });
                        }
                        
                        lock (gameLock)
                        {
                            gameState.Players.Add(Player);
                        }
                        
                        string response = $"ID|{Player.Id}";
                        Send(System.Text.Encoding.UTF8.GetBytes(response));
                        
                        Console.WriteLine($"Nouveau {(isObserver ? "observateur" : "joueur")}: {playerName}");
                        
                        // Si c'est le premier joueur, générer de la nourriture
                        if (gameState.Players.Count == 1)
                        {
                            GenerateFood();
                        }
                    }
                    else if (parts[0] == "MOVE" && Player != null && !Player.IsObserver)
                    {
                        string playerId = parts[1];
                        string direction = parts[2];
                        
                        if (Player.Id == playerId)
                        {
                            // Empêcher les mouvements opposés immédiats
                            if ((Player.Direction == "Up" && direction != "Down") ||
                                (Player.Direction == "Down" && direction != "Up") ||
                                (Player.Direction == "Left" && direction != "Right") ||
                                (Player.Direction == "Right" && direction != "Left"))
                            {
                                Player.Direction = direction;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur traitement message: {ex.Message}");
                }
            }
            
            public bool SendGameData(byte[] data)
            {
                try
                {
                    stream.Write(data, 0, data.Length);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            
            void Send(byte[] data)
            {
                try
                {
                    stream.Write(data, 0, data.Length);
                }
                catch { }
            }
            
            void Disconnect()
            {
                try
                {
                    client.Close();
                }
                catch { }
            }
        }
    }
}

