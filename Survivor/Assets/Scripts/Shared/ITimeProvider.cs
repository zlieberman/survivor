namespace Survivor.Shared
{
    /// <summary>
    /// Interface for objects that provide time-related functionality
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the real time (in seconds) that represents one game hour
        /// </summary>
        float RealTimePerGameHour { get; }
        
        /// <summary>
        /// Gets the current elapsed real time since the game started
        /// </summary>
        float ElapsedRealTime { get; }
        
        /// <summary>
        /// Gets the starting hour of the game (0-23)
        /// </summary>
        int StartHour { get; }
        
        /// <summary>
        /// Gets the starting minute of the game (0-59)
        /// </summary>
        int StartMinute { get; }
    }
} 