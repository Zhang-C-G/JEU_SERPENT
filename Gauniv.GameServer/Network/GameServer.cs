using System.Net;
using System.Net.Sockets;
using MessagePack;
using Gauniv.GameServer.Engine;
using Gauniv.GameServer.Models;
using Gauniv.Shared.Network;

namespace Gauniv.GameServer.Network
{
    /// <summary>
    /// TCP Server for hosting Snake game matches
    /// </summary>
    public class GameServer
    {
        private const int PORT = 7777;
        private TcpListener? _listener;
        private bool _isRunning = false;
        private Dictionary<string, TcpClient> _clients = new();
        private Dictionary<string, GameEngine> _activeGames = new();
        
        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, PORT);
            _listener.Start();
            _isRunning = true;
            
            Console.WriteLine($"🎮 Game Server started on port {PORT}");
            
            while (_isRunning)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accepting client: {ex.Message}");
                }
            }
        }
        
        private async Task HandleClientAsync(TcpClient client)
        {
            string? playerId = null;
            
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[4096];
                
                while (client.Connected)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;
                    
                    // Deserialize message
                    var message = MessagePackSerializer.Deserialize<PlayerInputMessage>(buffer.AsMemory(0, bytesRead));
                    
                    if (playerId == null)
                    {
                        playerId = message.PlayerId;
                        _clients[playerId] = client;
                        Console.WriteLine($"Player {playerId} connected");
                    }
                    
                    // Process input (update game state)
                    // TODO: Implement game logic update
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
            finally
            {
                if (playerId != null)
                {
                    _clients.Remove(playerId);
                    Console.WriteLine($"Player {playerId} disconnected");
                }
                client.Close();
            }
        }
        
        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
        }
    }
}
