using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.AI
{
    /// <summary>
    /// Simple A* pathfinding for AI
    /// </summary>
    public class PathFinder
    {
        private readonly GameState _state;
        
        public PathFinder(GameState state)
        {
            _state = state;
        }
        
        /// <summary>
        /// Find path from start to goal using simplified A*
        /// Returns null if no path found
        /// </summary>
        public List<Position>? FindPath(Position start, Position goal)
        {
            // Simplified pathfinding - BFS for now
            var visited = new HashSet<Position>();
            var queue = new Queue<(Position pos, List<Position> path)>();
            
            queue.Enqueue((start, new List<Position> { start }));
            visited.Add(start);
            
            int maxIterations = 100;  // Prevent infinite loops
            int iterations = 0;
            
            while (queue.Count > 0 && iterations++ < maxIterations)
            {
                var (current, path) = queue.Dequeue();
                
                if (current == goal)
                {
                    return path;
                }
                
                // Try all 4 directions
                foreach (var direction in new[] { Direction.Up, Direction.Down, Direction.Left, Direction.Right })
                {
                    var next = GetNextPosition(current, direction);
                    
                    if (visited.Contains(next)) continue;
                    if (!_state.IsPositionValid(next)) continue;
                    if (_state.IsPositionOccupied(next)) continue;
                    
                    visited.Add(next);
                    var newPath = new List<Position>(path) { next };
                    queue.Enqueue((next, newPath));
                }
            }
            
            return null;  // No path found
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
