using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using SnakeGame.Shared;

namespace SnakeGame.Server
{
    class Program
    {
        static TcpListener? server;
        static List<ClientHandler> clients = new List<ClientHandler>();
        static List<GameRoom> rooms = new List<GameRoom>();
        static Random random = new Random();
        static object gameLock = new object();
        static int gridWidth = 30;
        static int gridHeight = 20;
        
        static void Main()
        {
            Console.Title = "Snake Server";
            Console.WriteLine("=== SERVEUR SNAKE ===");
            
            CreateDefaultRoom();
            StartServer();
        }
        
        static void CreateDefaultRoom()
        {
            var defaultRoom = new GameRoom
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Salon Principal",
                MaxPlayers = 4
            };
            
            // Initialiser la nourriture
            defaultRoom.Food = new FoodItem 
            { 
                Exists = true,
                Position = new Position { X = 10, Y = 10 }
            };
            
            // Initialiser le power-up
            defaultRoom.PowerUp = new PowerUp 
            { 
                Exists = false 
            };
            
            defaultRoom.Players = new List<Player>();
            
            lock (gameLock)
            {
                rooms.Add(defaultRoom);
            }
            
            Console.WriteLine($" Salon créé: {defaultRoom.Name}");
        }
        
        static void StartServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 8888);
                server.Start();
                
                Console.WriteLine($" Serveur démarré sur le port 8888");
                Console.WriteLine($" En attente de joueurs...");
                Console.WriteLine("");
                Console.WriteLine("COMMANDES:");
                Console.WriteLine("  exit - Arrêter le serveur");
                Console.WriteLine("");
                
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.Start();
                
                Thread gameThread = new Thread(GameLoop);
                gameThread.Start();
                
                while (Console.ReadLine()?.ToLower() != "exit") { }
                
                server.Stop();
                Console.WriteLine("Serveur arrêté.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur: {ex.Message}");
            }
        }
        
        static void AcceptClients()
        {
            try
            {
                while (true)
                {
                    TcpClient client = server!.AcceptTcpClient();
                    ClientHandler handler = new ClientHandler(client);
                    
                    lock (gameLock)
                    {
                        clients.Add(handler);
                    }
                    
                    Thread clientThread = new Thread(handler.HandleClient);
                    clientThread.Start();
                    
                    Console.WriteLine($" Nouveau client connecté ({clients.Count} total)");
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
                    lock (gameLock)
                    {
                        foreach (var room in rooms)
                        {
                            UpdateRoom(room);
                        }
                    }
                    
                    // ENVOYER L'ÉTAT À TOUS LES CLIENTS
                    foreach (var client in clients)
                    {
                        client.SendGameState();
                    }
                    
                    Thread.Sleep(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur boucle: {ex.Message}");
                }
            }
        }
        
        static void UpdateRoom(GameRoom room)
        {
            if (room.Players == null) room.Players = new List<Player>();
            if (room.Players.Count == 0) return;
            
            foreach (var player in room.Players)
            {
                if (player == null || player.IsObserver || player.Snake == null || player.Snake.Count == 0)
                    continue;
                
                MovePlayer(player);
                CheckCollisions(player, room);
                CheckFood(player, room);
                CheckPowerUp(player, room);
            }
            
            // Générer un power-up aléatoirement
            if (!room.PowerUp.Exists && random.Next(0, 100) < 3)
            {
                GeneratePowerUp(room);
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
        
        static void CheckCollisions(Player player, GameRoom room)
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
            
            // Collision avec d'autres joueurs
            foreach (var otherPlayer in room.Players)
            {
                if (otherPlayer == null || otherPlayer.Id == player.Id || otherPlayer.IsObserver) 
                    continue;
                
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
        
        static void CheckFood(Player player, GameRoom room)
        {
            var head = player.Snake[0];
            
            if (room.Food != null && room.Food.Exists && 
                head.X == room.Food.Position.X && head.Y == room.Food.Position.Y)
            {
                player.Score += 10;
                player.Snake.Add(new Position { X = -1, Y = -1 });
                GenerateFood(room);
                Console.WriteLine($"  {player.Name} a mangé! Score: {player.Score}");
            }
        }
        
        static void CheckPowerUp(Player player, GameRoom room)
        {
            if (room.PowerUp != null && room.PowerUp.Exists)
            {
                var head = player.Snake[0];
                
                if (head.X == room.PowerUp.Position.X && head.Y == room.PowerUp.Position.Y)
                {
                    player.Score += 50;
                    player.IsInvincible = true;
                    room.PowerUp.Exists = false;
                    Console.WriteLine($"  {player.Name} a pris le BONUS! +50");
                    
                    // Timer pour désactiver l'invincibilité
                    Thread timerThread = new Thread(() =>
                    {
                        Thread.Sleep(3000);
                        lock (gameLock)
                        {
                            player.IsInvincible = false;
                            Console.WriteLine($"  {player.Name} n'est plus invincible");
                        }
                    });
                    timerThread.Start();
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
            player.Score = 0;
            Console.WriteLine($"  {player.Name} a perdu! Réinitialisation");
        }
        
        static void GenerateFood(GameRoom room)
        {
            bool valid;
            do
            {
                valid = true;
                room.Food.Position = new Position
                {
                    X = random.Next(0, gridWidth),
                    Y = random.Next(0, gridHeight)
                };
                
                foreach (var player in room.Players)
                {
                    if (player == null || player.Snake == null) continue;
                    foreach (var segment in player.Snake)
                    {
                        if (segment.X == room.Food.Position.X && segment.Y == room.Food.Position.Y)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }
            } while (!valid);
            
            room.Food.Exists = true;
        }
        
        static void GeneratePowerUp(GameRoom room)
        {
            bool valid;
            do
            {
                valid = true;
                room.PowerUp.Position = new Position
                {
                    X = random.Next(0, gridWidth),
                    Y = random.Next(0, gridHeight)
                };
                
                // Pas sur la nourriture
                if (room.Food != null && room.Food.Exists &&
                    room.PowerUp.Position.X == room.Food.Position.X && 
                    room.PowerUp.Position.Y == room.Food.Position.Y)
                {
                    valid = false;
                    continue;
                }
                
                // Pas sur un serpent
                foreach (var player in room.Players)
                {
                    if (player == null || player.Snake == null) continue;
                    foreach (var segment in player.Snake)
                    {
                        if (segment.X == room.PowerUp.Position.X && segment.Y == room.PowerUp.Position.Y)
                        {
                            valid = false;
                            break;
                        }
                    }
                    if (!valid) break;
                }
            } while (!valid);
            
            room.PowerUp.Exists = true;
            Console.WriteLine($"  Bonus généré à ({room.PowerUp.Position.X}, {room.PowerUp.Position.Y})");
        }
        
        class ClientHandler
        {
            private TcpClient client;
            private NetworkStream stream;
            public Player? Player { get; set; }
            
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
                            if (bytesRead > 0)
                            {
                                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                ProcessMessage(message);
                            }
                        }
                        Thread.Sleep(10);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  Erreur client: {ex.Message}");
                }
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
                        
                        GameRoom? room = null;
                        lock (gameLock)
                        {
                            room = rooms.FirstOrDefault();
                        }
                        
                        if (room == null) return;
                        
                        Player = new Player
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = playerName,
                            IsObserver = isObserver,
                            RoomId = room.Id,
                            Direction = "Right",
                            Score = 0,
                            Snake = new List<Position>()
                        };
                        
                        if (!isObserver)
                        {
                            Player.Snake.Add(new Position { X = 10, Y = 10 });
                            Player.Snake.Add(new Position { X = 9, Y = 10 });
                            Player.Snake.Add(new Position { X = 8, Y = 10 });
                        }
                        
                        lock (gameLock)
                        {
                            if (room.Players == null) room.Players = new List<Player>();
                            room.Players.Add(Player);
                        }
                        
                        string response = $"ID|{Player.Id}";
                        byte[] data = Encoding.UTF8.GetBytes(response);
                        stream.Write(data, 0, data.Length);
                        
                        Console.WriteLine($"   {playerName} a rejoint {(isObserver ? "(Obs)" : "")}");
                        
                        // S'assurer qu'il y a de la nourriture
                        if (room.Food == null || !room.Food.Exists)
                        {
                            GenerateFood(room);
                        }
                    }
                    else if (parts[0] == "MOVE" && Player != null && !Player.IsObserver)
                    {
                        string playerId = parts[1];
                        string direction = parts[2];
                        
                        if (Player.Id == playerId)
                        {
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
                    Console.WriteLine($"  Erreur message: {ex.Message}");
                }
            }
            
            public void SendGameState()
            {
                try
                {
                    if (Player == null) return;
                    
                    GameRoom? room = null;
                    lock (gameLock)
                    {
                        room = rooms.FirstOrDefault(r => r.Id == Player.RoomId);
                    }
                    
                    if (room == null) return;
                    
                    var gameState = new GameState
                    {
                        Rooms = new List<GameRoom> { room },
                        CurrentRoomId = Player.RoomId
                    };
                    
                    string json = JsonSerializer.Serialize(gameState);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    
                    if (client.Connected)
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                catch { }
            }
            
            void Disconnect()
            {
                try
                {
                    if (Player != null)
                    {
                        lock (gameLock)
                        {
                            var room = rooms.FirstOrDefault(r => r.Id == Player.RoomId);
                            if (room != null && room.Players != null)
                            {
                                room.Players.Remove(Player);
                            }
                        }
                        Console.WriteLine($"   {Player.Name} a quitté");
                    }
                    client.Close();
                }
                catch { }
            }
        }
    }
}
