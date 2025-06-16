namespace Survivor.Shared
{
    public interface IDialogueInteractable
    {
        string GetDialogueId();
        string GetDisplayName();
        void OnDialogueStart();
        void OnDialogueEnd();
    }
} 