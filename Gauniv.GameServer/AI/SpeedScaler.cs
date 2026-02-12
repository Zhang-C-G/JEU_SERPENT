namespace Gauniv.GameServer.AI
{
    /// <summary>
    /// Progressive speed scaler for AI - increases 10% every 10 seconds
    /// </summary>
    public class SpeedScaler
    {
        private const int BASE_SPEED_MS = 100;
        private const double SPEED_INCREASE_FACTOR = 1.1;  // 10% per interval
        private const int INTERVAL_SECONDS = 10;
        private const double MAX_MULTIPLIER = 1.76;  // Maximum 176% speed
        private const int MIN_SPEED_MS = 57;  // Fastest possible
        
        private readonly DateTime _startTime;
        
        public SpeedScaler(DateTime startTime)
        {
            _startTime = startTime;
        }
        
        /// <summary>
        /// Get current AI speed in milliseconds
        /// </summary>
        public int GetCurrentSpeed()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            int elapsedSeconds = (int)elapsed.TotalSeconds;
            
            // Calculate which interval we're in (0, 1, 2, ...)
            int intervals = elapsedSeconds / INTERVAL_SECONDS;
            
            // Calculate speed multiplier
            double multiplier = Math.Pow(SPEED_INCREASE_FACTOR, intervals);
            multiplier = Math.Min(multiplier, MAX_MULTIPLIER);
            
            // Calculate actual speed (lower ms = faster)
            int speedMs = (int)(BASE_SPEED_MS / multiplier);
            speedMs = Math.Max(speedMs, MIN_SPEED_MS);
            
            return speedMs;
        }
        
        /// <summary>
        /// Get current speed as a percentage (for display)
        /// </summary>
        public int GetSpeedPercentage()
        {
            var elapsed = DateTime.UtcNow - _startTime;
            int elapsedSeconds = (int)elapsed.TotalSeconds;
            int intervals = elapsedSeconds / INTERVAL_SECONDS;
            
            double multiplier = Math.Pow(SPEED_INCREASE_FACTOR, intervals);
            multiplier = Math.Min(multiplier, MAX_MULTIPLIER);
            
            return (int)(multiplier * 100);
        }
    }
}
