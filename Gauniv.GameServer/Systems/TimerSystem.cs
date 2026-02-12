using Gauniv.GameServer.Models;

namespace Gauniv.GameServer.Systems
{
    /// <summary>
    /// Timer system for 2-minute game matches
    /// </summary>
    public class TimerSystem
    {
        private readonly GameState _state;
        
        public TimerSystem(GameState state)
        {
            _state = state;
        }
        
        /// <summary>
        /// Update game timer
        /// </summary>
        public void Update()
        {
            if (_state.IsGameOver) return;
            
            var elapsed = DateTime.UtcNow - _state.StartTime;
            var remainingSeconds = 120 - (int)elapsed.TotalSeconds;
            
            _state.RemainingSeconds = Math.Max(0, remainingSeconds);
            
            if (_state.RemainingSeconds <= 0)
            {
                // Time's up - determine winner
                _state.DetermineWinner();
            }
        }
    }
}
