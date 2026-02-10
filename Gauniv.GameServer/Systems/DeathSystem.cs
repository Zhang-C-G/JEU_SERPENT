using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.Systems
{
    /// <summary>
    /// Death and respawn system
    /// </summary>
    public class DeathSystem
    {
        private const int RESPAWN_DELAY_MS = 3000;  // 3 seconds
        private readonly GameState _state;
        private readonly Random _random = new();
        
        public DeathSystem(GameState state)
        {
            _state = state;
        }
        
        /// <summary>
        /// Handle player death
        /// </summary>
        public void HandleDeath(Player player)
        {
            if (player.LivesRemaining <= 0) return;
            
            // Mark snake as dead
            player.Snake.IsDead = true;
            
            // Update stats
            player.DeathCount++;
            player.LivesRemaining--;
            
            // Track max length
            if (player.Snake.Length > player.MaxLength)
            {
                player.MaxLength = player.Snake.Length;
            }
            
            // Schedule respawn if lives remaining
            if (player.LivesRemaining > 0)
            {
                player.IsRespawning = true;
                player.RespawnTime = DateTime.UtcNow.AddMilliseconds(RESPAWN_DELAY_MS);
            }
        }
        
        /// <summary>
        /// Process respawn for all players
        /// </summary>
        public void ProcessRespawns()
        {
            ProcessPlayerRespawn(_state.Player1);
            ProcessPlayerRespawn(_state.Player2);
        }
        
        private void ProcessPlayerRespawn(Player? player)
        {
            if (player == null || !player.IsRespawning || player.RespawnTime == null)
                return;
                
            if (DateTime.UtcNow >= player.RespawnTime)
            {
                // Find safe spawn position
                var spawnPos = _state.GetRandomEmptyPosition(_random);
                
                // Reset snake
                player.Snake.Initialize(spawnPos, Direction.Right);
                player.IsRespawning = false;
                player.RespawnTime = null;
            }
        }
    }
}
