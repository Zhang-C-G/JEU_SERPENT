using Gauniv.GameServer.AI;
using Gauniv.GameServer.Collectibles;
using Gauniv.GameServer.Models;
using Gauniv.GameServer.Systems;

namespace Gauniv.GameServer.Engine
{
    /// <summary>
    /// Main game engine - handles game loop and logic
    /// </summary>
    public class GameEngine
    {
        private const int GAME_TICK_MS = 100;  // 100ms per tick (10 ticks/second)
        
        private readonly GameState _state;
        private readonly CollisionDetector _collisionDetector;
        private readonly DeathSystem _deathSystem;
        private readonly TimerSystem _timerSystem;
        private readonly CollectibleFactory _collectibleFactory;
        private readonly Random _random = new();
        
        private AIController? _aiController;
        private DateTime _lastAIMove = DateTime.UtcNow;
        
        public GameEngine(GameState state)
        {
            _state = state;
            _collisionDetector = new CollisionDetector(state);
            _deathSystem = new DeathSystem(state);
            _timerSystem = new TimerSystem(state);
            _collectibleFactory = new CollectibleFactory();
        }
        
        /// <summary>
        /// Initialize game
        /// </summary>
        public void Initialize()
        {
            _state.StartTime = DateTime.UtcNow;
            _state.RemainingSeconds = 120;
            
            // Initialize Player 1
            if (_state.Player1 != null)
            {
                var p1Pos = new Position(5, GameState.MAP_SIZE / 2);
                _state.Player1.Snake.Initialize(p1Pos, Direction.Right);
                _state.Player1.LivesRemaining = 3;
                _state.Player1.DeathCount = 0;
                _state.Player1.Score = 0;
            }
            
            // Initialize Player 2 or AI
            if (_state.Player2 != null)
            {
                var p2Pos = new Position(GameState.MAP_SIZE - 6, GameState.MAP_SIZE / 2);
                _state.Player2.Snake.Initialize(p2Pos, Direction.Left);
                _state.Player2.LivesRemaining = 3;
                _state.Player2.DeathCount = 0;
                _state.Player2.Score = 0;
                
                // Setup AI if in AI mode
                if (_state.Mode == GameMode.AITraining)
                {
                    _aiController = new AIController(_state.Player2.Snake, _state);
                }
            }
            
            // Spawn initial collectibles
            SpawnInitialCollectibles();
        }
        
        /// <summary>
        /// Main game tick
        /// </summary>
        public void Tick()
        {
            if (_state.IsGameOver) return;
            
            // Update timer
            _timerSystem.Update();
            
            // Process respawns
            _deathSystem.ProcessRespawns();
            
            // Move snakes
            MovePlayer(_state.Player1);
            MovePlayer(_state.Player2, isAI: _state.Mode == GameMode.AITraining);
            
            // Maintain collectible count
            MaintainCollectibles();
            
            // Check game over conditions
            CheckGameOver();
        }
        
        private void MovePlayer(Player? player, bool isAI = false)
        {
            if (player == null || player.Snake.IsDead || player.IsRespawning)
                return;
            
            // AI decision
            if (isAI && _aiController != null)
            {
                var aiSpeed = _aiController.GetCurrentSpeedMs();
                if ((DateTime.UtcNow - _lastAIMove).TotalMilliseconds >= aiSpeed)
                {
                    var direction = _aiController.DecideDirection();
                    player.Snake.SetDirection(direction);
                    _lastAIMove = DateTime.UtcNow;
                }
                else
                {
                    return;  // Not time for AI to move yet
                }
            }
            
            // Check collectible collision BEFORE moving
            var collectible = _collisionDetector.CheckCollectibleCollision(player.Snake);
            bool shouldGrow = false;
            
            if (collectible != null)
            {
                var result = collectible.OnCollected(player.Snake, _state);
                
                // Apply results
                shouldGrow = result.GrowSnake;
                if (result.GrowthAmount < 0)
                {
                    // Shrink snake
                    int toRemove = Math.Abs(result.GrowthAmount);
                    for (int i = 0; i < toRemove && player.Snake.Body.Count > 1; i++)
                    {
                        player.Snake.Body.RemoveAt(player.Snake.Body.Count - 1);
                    }
                }
                
                player.Score += result.ScoreBonus;
                
                // TODO: Apply effects (speed, shield, etc.)
                
                if (result.RemoveCollectible)
                {
                    _state.Collectibles.Remove(collectible);
                }
            }
            
            // Move snake
            player.Snake.Move(grow: shouldGrow);
            
            // Check collisions AFTER moving
            var opponent = player == _state.Player1 ? _state.Player2?.Snake : _state.Player1?.Snake;
            bool collision = _collisionDetector.CheckCollisions(player.Snake, opponent);
            
            if (collision)
            {
                _deathSystem.HandleDeath(player);
            }
        }
        
        private void SpawnInitialCollectibles()
        {
            for (int i = 0; i < GameState.INITIAL_FOOD_COUNT; i++)
            {
                SpawnCollectible();
            }
        }
        
        private void SpawnCollectible()
        {
            var position = _state.GetRandomEmptyPosition(_random);
            var collectible = _collectibleFactory.SpawnRandom(position);
            _state.Collectibles.Add(collectible);
        }
        
        private void MaintainCollectibles()
        {
            // Keep collectible count at target
            while (_state.Collectibles.Count < GameState.INITIAL_FOOD_COUNT)
            {
                SpawnCollectible();
            }
        }
        
        private void CheckGameOver()
        {
            if (_state.Player1 == null || _state.Player2 == null)
            {
                _state.IsGameOver = true;
                return;
            }
            
            // Check if any player has no lives
            if (!_state.Player1.IsAlive || !_state.Player2.IsAlive)
            {
                _state.DetermineWinner();
            }
        }
    }
}
