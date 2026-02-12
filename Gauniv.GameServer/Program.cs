using Gauniv.GameServer.Engine;
using Gauniv.GameServer.Models;
using Gauniv.GameServer.Network;

namespace Gauniv.GameServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=================================");
            Console.WriteLine("    Snake Duel - Game Server    ");
            Console.WriteLine("=================================");
            Console.WriteLine();
            
            // Option 1: Start network server
            Console.WriteLine("Select mode:");
            Console.WriteLine("1. Start TCP Server (multiplayer)");
            Console.WriteLine("2. Test Local Game Engine");
            Console.Write("Choice: ");
            
            var choice = Console.ReadLine();
            
            if (choice == "1")
            {
                var server = new GameServer.Network.GameServer();
                await server.StartAsync();
            }
            else if (choice == "2")
            {
                TestLocalGame();
            }
        }
        
        static void TestLocalGame()
        {
            Console.WriteLine("\n🎮 Testing Local Game Engine...\n");
            
            // Create game state
            var state = new GameState
            {
                Mode = GameMode.AITraining,
                Player1 = new Player
                {
                    PlayerId = "player1",
                    PlayerName = "Human Player"
                },
                Player2 = new Player
                {
                    PlayerId = "ai",
                    PlayerName = "AI Opponent"
                }
            };
            
            // Create and initialize engine
            var engine = new GameEngine(state);
            engine.Initialize();
            
            Console.WriteLine("✅ Game initialized!");
            Console.WriteLine($"   Player 1: {state.Player1.Snake.Body.Count} segments at {state.Player1.Snake.Head.X},{state.Player1.Snake.Head.Y}");
            Console.WriteLine($"   Player 2 (AI): {state.Player2.Snake.Body.Count} segments at {state.Player2.Snake.Head.X},{state.Player2.Snake.Head.Y}");
            Console.WriteLine($"   Collectibles: {state.Collectibles.Count}");
            Console.WriteLine($"   Time: {state.RemainingSeconds}s");
            
            // Run a few ticks
            Console.WriteLine("\n🔄 Running 5 game ticks...");
            for (int i = 0; i < 5; i++)
            {
                Thread.Sleep(100);
                engine.Tick();
                Console.WriteLine($"   Tick {i + 1}: P1={state.Player1.Snake.Length} segments, P2={state.Player2.Snake.Length} segments, Time={state.RemainingSeconds}s");
            }
            
            Console.WriteLine("\n✅ Engine test completed!");
        }
    }
}
