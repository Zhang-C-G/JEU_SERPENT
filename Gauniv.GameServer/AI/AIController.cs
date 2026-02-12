using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.AI
{
    /// <summary>
    /// AI controller for computer-controlled snake
    /// </summary>
    public class AIController
    {
        private readonly Snake _aiSnake;
        private readonly GameState _state;
        private readonly PathFinder _pathFinder;
        private readonly SpeedScaler _speedScaler;
        private readonly Random _random = new();
        
        public AIController(Snake aiSnake, GameState state)
        {
            _aiSnake = aiSnake;
            _state = state;
            _pathFinder = new PathFinder(state);
            _speedScaler = new SpeedScaler(state.StartTime);
        }
        
        /// <summary>
        /// Get current AI speed in milliseconds per move
        /// </summary>
        public int GetCurrentSpeedMs()
        {
            return _speedScaler.GetCurrentSpeed();
        }
        
        /// <summary>
        /// Decide next move direction
        /// </summary>
        public Direction DecideDirection()
        {
            if (_aiSnake.IsDead) return _aiSnake.CurrentDirection;
            
            // Find nearest food
            var nearestFood = FindNearestFood();
            
            if (nearestFood != null)
            {
                // Try to path to food
                var path = _pathFinder.FindPath(_aiSnake.Head, nearestFood.Position);
                
                if (path != null && path.Count > 1)
                {
                    var nextPos = path[1];
                    var direction = GetDirectionTo(nextPos);
                    
                    if (IsSafeDirection(direction))
                    {
                        return direction;
                    }
                }
            }
            
            // If no safe path to food, find any safe direction
            var safeDirections = GetSafeDirections();
            if (safeDirections.Any())
            {
                return safeDirections[_random.Next(safeDirections.Count)];
            }
            
            // No safe direction, continue current path
            return _aiSnake.CurrentDirection;
        }
        
        private Collectibles.ICollectible? FindNearestFood()
        {
            Collectibles.ICollectible? nearest = null;
            int minDistance = int.MaxValue;
            
            foreach (var collectible in _state.Collectibles)
            {
                if (collectible.Type == Collectibles.CollectibleType.Food)
                {
                    int distance = ManhattanDistance(_aiSnake.Head, collectible.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = collectible;
                    }
                }
            }
            
            return nearest;
        }
        
        private int ManhattanDistance(Position a, Position b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
        
        private Direction GetDirectionTo(Position target)
        {
            var head = _aiSnake.Head;
            
            if (target.X > head.X) return Direction.Right;
            if (target.X < head.X) return Direction.Left;
            if (target.Y > head.Y) return Direction.Down;
            if (target.Y < head.Y) return Direction.Up;
            
            return _aiSnake.CurrentDirection;
        }
        
        private bool IsSafeDirection(Direction direction)
        {
            var testHead = GetNextPosition(_aiSnake.Head, direction);
            
            // Check bounds
            if (!_state.IsPositionValid(testHead))
                return false;
            
            // Check self collision
            if (_aiSnake.Body.Contains(testHead))
                return false;
            
            // Check opponent
            if (_state.Player1 != null && !_state.Player1.Snake.IsDead)
            {
                if (_state.Player1.Snake.Body.Contains(testHead))
                    return false;
            }
            
            return true;
        }
        
        private List<Direction> GetSafeDirections()
        {
            var safe = new List<Direction>();
            
            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                // Don't reverse
                if (IsOppositeDirection(dir, _aiSnake.CurrentDirection))
                    continue;
                    
                if (IsSafeDirection(dir))
                    safe.Add(dir);
            }
            
            return safe;
        }
        
        private bool IsOppositeDirection(Direction a, Direction b)
        {
            return (a == Direction.Up && b == Direction.Down) ||
                   (a == Direction.Down && b == Direction.Up) ||
                   (a == Direction.Left && b == Direction.Right) ||
                   (a == Direction.Right && b == Direction.Left);
        }
        
        private Position GetNextPosition(Position current, Direction direction)
        {
            return direction switch
            {
                Direction.Up => new Position(current.X, current.Y - 1),
                Direction.Down => new Position(current.X, current.Y + 1),
                Direction.Left => new Position(current.X - 1, current.Y),
                Direction.Right => new Position(current.X + 1, current.Y),
                _ => current
            };
        }
    }
}
