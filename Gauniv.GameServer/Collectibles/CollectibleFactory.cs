using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.Collectibles
{
    /// <summary>
    /// Factory for creating and managing collectibles
    /// Supports runtime registration of new collectible types
    /// </summary>
    public class CollectibleFactory
    {
        private readonly Dictionary<string, Type> _registry = new();
        private readonly Random _random = new();
        
        public CollectibleFactory()
        {
            // Register built-in types
            Register<BasicFood>();
            Register<SuperFood>();
            Register<SpeedBoost>();
            Register<Shield>();
        }
        
        /// <summary>
        /// Register a new collectible type (extensibility)
        /// </summary>
        public void Register<T>() where T : ICollectible, new()
        {
            var instance = new T();
            _registry[instance.Id] = typeof(T);
        }
        
        /// <summary>
        /// Spawn a random collectible based on spawn weights
        /// </summary>
        public ICollectible SpawnRandom(Position position)
        {
            // Create temporary instances to get weights
            var collectibles = _registry.Values
                .Select(type => (ICollectible)Activator.CreateInstance(type)!)
                .ToList();
            
            if (collectibles.Count == 0)
            {
                // Fallback to basic food
                return new BasicFood { Position = position };
            }
            
            // Calculate total weight
            int totalWeight = collectibles.Sum(c => c.SpawnWeight);
            int randomValue = _random.Next(totalWeight);
            
            // Weighted random selection
            int currentWeight = 0;
            foreach (var collectible in collectibles)
            {
                currentWeight += collectible.SpawnWeight;
                if (randomValue < currentWeight)
                {
                    collectible.Position = position;
                    return collectible;
                }
            }
            
            // Fallback (shouldn't reach here)
            var fallback = new BasicFood { Position = position };
            return fallback;
        }
        
        /// <summary>
        /// Create specific collectible by ID
        /// </summary>
        public ICollectible Create(string id, Position position)
        {
            if (_registry.TryGetValue(id, out var type))
            {
                var collectible = (ICollectible)Activator.CreateInstance(type)!;
                collectible.Position = position;
                return collectible;
            }
            
            throw new ArgumentException($"Unknown collectible ID: {id}");
        }
        
        /// <summary>
        /// Get all registered collectible IDs
        /// </summary>
        public IEnumerable<string> GetRegisteredIds()
        {
            return _registry.Keys;
        }
    }
}
