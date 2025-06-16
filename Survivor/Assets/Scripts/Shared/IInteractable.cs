namespace Survivor.Shared
{
    public interface IInteractable
    {
        void Interact();
        void OnInteractionEnter();
        void OnInteractionExit();
    }
} 