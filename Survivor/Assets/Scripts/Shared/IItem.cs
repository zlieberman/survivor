namespace Survivor.Shared
{
    public interface IItem
    {
        string ItemName { get; }
        float Weight { get; }
        string Description { get; }
    }
} 