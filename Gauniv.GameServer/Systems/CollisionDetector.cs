using Gauniv.GameServer.Collectibles;
using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.Systems
{
    /// <summary>
    /// Collision detection system
    /// </summary>
    public class CollisionDetector
    {
        private readonly GameState _state;
        
        public CollisionDetector(GameState state)
        {
            _state = state;
        }
        
        /// <summary>
        /// Check if snake collides with walls
        /// </summary>
        public bool CheckWallCollision(Snake snake)
        {
            var head = snake.Head;
            return !_state.IsPositionValid(head);
        }
        
        /// <summary>
        /// Check if snake collides with itself
        /// </summary>
        public bool CheckSelfCollision(Snake snake)
        {
            if (snake.Body.Count < 2) return false;
            
            var head = snake.Head;
            // Check if head collides with body (skip first element which is head)
            return snake.Body.Skip(1).Any(segment => segment == head);
        }
        
        /// <summary>
        /// Check if snake collides with opponent
        /// </summary>
        public bool CheckOpponentCollision(Snake snake, Snake opponent)
        {
            if (opponent.IsDead) return false;
            
            var head = snake.Head;
            return opponent.Body.Any(segment => segment == head);
        }
        
        /// <summary>
        /// Check all collisions for a snake
        /// </summary>
        public bool CheckCollisions(Snake snake, Snake? opponent)
        {
            if (CheckWallCollision(snake))
                return true;
                
            if (CheckSelfCollision(snake))
                return true;
                
            if (opponent != null && CheckOpponentCollision(snake, opponent))
                return true;
                
            return false;
        }
        
        /// <summary>
        /// Check if snake collects any collectible
        /// </summary>
        public ICollectible? CheckCollectibleCollision(Snake snake)
        {
            var head = snake.Head;
            return _state.Collectibles.FirstOrDefault(c => c.Position == head);
        }
    }
}
