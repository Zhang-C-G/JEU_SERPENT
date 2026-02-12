using Gauniv.Game.Rendering;

namespace Gauniv.Game
{
    /// <summary>
    /// Local game controller - manages standalone game without network
    /// Integrates server-side game engine for local play
    /// </summary>
    public class LocalGameController
    {
        private readonly MenuForm.GameMode _mode;
        private readonly Random _random = new();
        
        // Simplified game state for local play
        private SnakeDto _player1Snake;
        private SnakeDto _player2Snake;
        private List<CollectibleDto> _collectibles = new();
        private int _player1Lives = 3;
        private int _player2Lives = 3;
        private int _player1Deaths = 0;
        private int _player2Deaths = 0;
        private DateTime _startTime;
        private bool _isGameOver = false;
        private string? _winnerName = null;
        
        // AI controller for AI mode
        private int _aiMoveCounter = 0;
        private DateTime _lastAIMove = DateTime.UtcNow;
        
        public LocalGameController(MenuForm.GameMode mode)
        {
            _mode = mode;
            _startTime = DateTime.UtcNow;
            InitializeGame();
        }
        
        private void InitializeGame()
        {
            // Initialize Player 1
            _player1Snake = new SnakeDto
            {
                Body = new List<PositionDto>
                {
                    new PositionDto { X = 5, Y = 15 },
                    new PositionDto { X = 4, Y = 15 },
                    new PositionDto { X = 3, Y = 15 }
                },
                Direction = DirectionDto.Right,
                IsDead = false
            };
            
            // Initialize Player 2 or AI
            _player2Snake = new SnakeDto
            {
                Body = new List<PositionDto>
                {
                    new PositionDto { X = 24, Y = 15 },
                    new PositionDto { X = 25, Y = 15 },
                    new PositionDto { X = 26, Y = 15 }
                },
                Direction = DirectionDto.Left,
                IsDead = false
            };
            
            // Spawn initial collectibles
            for (int i = 0; i < 5; i++)
            {
                SpawnCollectible();
            }
        }
        
        public void Tick()
        {
            if (_isGameOver) return;
            
            // Check time limit (2 minutes)
            var elapsed = DateTime.UtcNow - _startTime;
            if (elapsed.TotalSeconds >= 120)
            {
                DetermineWinner();
                return;
            }
            
            // Move snakes
            MoveSnake(_player1Snake);
            
            if (_mode == MenuForm.GameMode.AITraining)
            {
                // AI movement with progressive speed
                var aiSpeed = GetAISpeed();
                if ((DateTime.UtcNow - _lastAIMove).TotalMilliseconds >= aiSpeed)
                {
                    AIDecideDirection();
                    MoveSnake(_player2Snake);
                    _lastAIMove = DateTime.UtcNow;
                }
            }
            else
            {
                MoveSnake(_player2Snake);
            }
            
            // Maintain collectibles
            while (_collectibles.Count < 5)
            {
                SpawnCollectible();
            }
        }
        
        private void MoveSnake(SnakeDto snake)
        {
            if (snake.IsDead) return;
            
            // Calculate new head position
            var head = snake.Body[0];
            PositionDto newHead = snake.Direction switch
            {
                DirectionDto.Up => new PositionDto { X = head.X, Y = head.Y - 1 },
                DirectionDto.Down => new PositionDto { X = head.X, Y = head.Y + 1 },
                DirectionDto.Left => new PositionDto { X = head.X - 1, Y = head.Y },
                DirectionDto.Right => new PositionDto { X = head.X + 1, Y = head.Y },
                _ => head
            };
            
            // Check collisions
            bool collision = CheckCollision(newHead, snake);
            
            if (collision)
            {
                HandleDeath(snake);
                return;
            }
            
            // Check collectible
            var collected = _collectibles.FirstOrDefault(c => c.Position.X == newHead.X && c.Position.Y == newHead.Y);
            bool grow = collected != null;
            
            if (grow)
            {
                _collectibles.Remove(collected);
            }
            
            // Move
            snake.Body.Insert(0, newHead);
            if (!grow && snake.Body.Count > 0)
            {
                snake.Body.RemoveAt(snake.Body.Count - 1);
            }
        }
        
        private bool CheckCollision(PositionDto pos, SnakeDto snake)
        {
            // Wall collision
            if (pos.X < 0 || pos.X >= 30 || pos.Y < 0 || pos.Y >= 30)
                return true;
            
            // Self collision
            if (snake.Body.Any(segment => segment.X == pos.X && segment.Y == pos.Y))
                return true;
            
            // Opponent collision
            var opponent = snake == _player1Snake ? _player2Snake : _player1Snake;
            if (!opponent.IsDead && opponent.Body.Any(segment => segment.X == pos.X && segment.Y == pos.Y))
                return true;
            
            return false;
        }
        
        private void HandleDeath(SnakeDto snake)
        {
            if (snake == _player1Snake)
            {
                _player1Lives--;
                _player1Deaths++;
                if (_player1Lives <= 0)
                {
                    _isGameOver = true;
                    _winnerName = "Player 2";
                }
            }
            else
            {
                _player2Lives--;
                _player2Deaths++;
                if (_player2Lives <= 0)
                {
                    _isGameOver = true;
                    _winnerName = "Player 1";
                }
            }
            
            // Respawn (simplified - instant for local game)
            if (!_isGameOver)
            {
                var startX = snake == _player1Snake ? 5 : 24;
                snake.Body.Clear();
                snake.Body.Add(new PositionDto { X = startX, Y = 15 });
                snake.Body.Add(new PositionDto { X = startX - 1, Y = 15 });
                snake.Body.Add(new PositionDto { X = startX - 2, Y = 15 });
                snake.Direction = DirectionDto.Right;
                snake.IsDead = false;
            }
        }
        
        private void SpawnCollectible()
        {
            for (int i = 0; i < 100; i++)
            {
                var pos = new PositionDto { X = _random.Next(30), Y = _random.Next(30) };
                
                // Check if position is free
                bool occupied = _player1Snake.Body.Any(s => s.X == pos.X && s.Y == pos.Y) ||
                               _player2Snake.Body.Any(s => s.X == pos.X && s.Y == pos.Y) ||
                               _collectibles.Any(c => c.Position.X == pos.X && c.Position.Y == pos.Y);
                
                if (!occupied)
                {
                    _collectibles.Add(new CollectibleDto
                    {
                        Position = pos,
                        RenderVisualId = _random.Next(10) == 0 ? "golden_apple" : "apple"
                    });
                    return;
                }
            }
        }
        
        private void AIDecideDirection()
        {
            // Simple AI: find nearest collectible
            var head = _player2Snake.Body[0];
            CollectibleDto? nearest = null;
            int minDist = int.MaxValue;
            
            foreach (var collectible in _collectibles)
            {
                int dist = Math.Abs(collectible.Position.X - head.X) + Math.Abs(collectible.Position.Y - head.Y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = collectible;
                }
            }
            
            if (nearest != null)
            {
                // Move towards collectible
                if (nearest.Position.X > head.X && !WillCollide(DirectionDto.Right))
                    _player2Snake.Direction = DirectionDto.Right;
                else if (nearest.Position.X < head.X && !WillCollide(DirectionDto.Left))
                    _player2Snake.Direction = DirectionDto.Left;
                else if (nearest.Position.Y > head.Y && !WillCollide(DirectionDto.Down))
                    _player2Snake.Direction = DirectionDto.Down;
                else if (nearest.Position.Y < head.Y && !WillCollide(DirectionDto.Up))
                    _player2Snake.Direction = DirectionDto.Up;
            }
        }
        
        private bool WillCollide(DirectionDto direction)
        {
            var head = _player2Snake.Body[0];
            PositionDto testPos = direction switch
            {
                DirectionDto.Up => new PositionDto { X = head.X, Y = head.Y - 1 },
                DirectionDto.Down => new PositionDto { X = head.X, Y = head.Y + 1 },
                DirectionDto.Left => new PositionDto { X = head.X - 1, Y = head.Y },
                DirectionDto.Right => new PositionDto { X = head.X + 1, Y = head.Y },
                _ => head
            };
            
            return CheckCollision(testPos, _player2Snake);
        }
        
        private int GetAISpeed()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            int intervals = (int)elapsed.TotalSeconds / 10;
            double multiplier = Math.Pow(1.1, intervals);
            multiplier = Math.Min(multiplier, 1.76);
            return Math.Max((int)(100 / multiplier), 57);
        }
        
        private void DetermineWinner()
        {
            _isGameOver = true;
            
            if (_player1Deaths < _player2Deaths)
                _winnerName = "Player 1";
            else if (_player2Deaths < _player1Deaths)
                _winnerName = "Player 2";
            else if (_player1Snake.Body.Count > _player2Snake.Body.Count)
                _winnerName = "Player 1";
            else if (_player2Snake.Body.Count > _player1Snake.Body.Count)
                _winnerName = "Player 2";
            else
                _winnerName = null; // Draw
        }
        
        public void SetPlayer1Direction(DirectionDto direction)
        {
            // Prevent 180-degree turns
            if ((_player1Snake.Direction == DirectionDto.Up && direction == DirectionDto.Down) ||
                (_player1Snake.Direction == DirectionDto.Down && direction == DirectionDto.Up) ||
                (_player1Snake.Direction == DirectionDto.Left && direction == DirectionDto.Right) ||
                (_player1Snake.Direction == DirectionDto.Right && direction == DirectionDto.Left))
                return;
            
            _player1Snake.Direction = direction;
        }
        
        public void SetPlayer2Direction(DirectionDto direction)
        {
            // Only allow manual control in PvP mode
            if (_mode != MenuForm.GameMode.PvP)
                return;
            
            // Prevent 180-degree turns
            if ((_player2Snake.Direction == DirectionDto.Up && direction == DirectionDto.Down) ||
                (_player2Snake.Direction == DirectionDto.Down && direction == DirectionDto.Up) ||
                (_player2Snake.Direction == DirectionDto.Left && direction == DirectionDto.Right) ||
                (_player2Snake.Direction == DirectionDto.Right && direction == DirectionDto.Left))
                return;
            
            _player2Snake.Direction = direction;
        }
        
        public GameStateDto GetGameState()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            int remaining = Math.Max(0, 120 - (int)elapsed.TotalSeconds);
            
            return new GameStateDto
            {
                Player1Snake = _player1Snake,
                Player2Snake = _player2Snake,
                Collectibles = _collectibles,
                Player1Lives = _player1Lives,
                Player2Lives = _player2Lives,
                Player1Deaths = _player1Deaths,
                Player2Deaths = _player2Deaths,
                RemainingSeconds = remaining,
                IsGameOver = _isGameOver,
                WinnerName = _winnerName
            };
        }
    }
}
