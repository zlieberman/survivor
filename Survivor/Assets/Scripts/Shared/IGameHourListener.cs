namespace Survivor.Shared
{
    /// <summary>
    /// Interface for objects that need to be notified when game hours pass
    /// </summary>
    public interface IGameHourListener
    {
        void OnGameHourPassed();
    }
} 