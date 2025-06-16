namespace Survivor.Shared.Interfaces
{
    public interface IDialogueUI
    {
        void ShowDialogue(string npcName, string dialogue);
        void CloseDialogue();
    }
} 