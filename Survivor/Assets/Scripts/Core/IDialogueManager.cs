namespace Survivor.Dialogue
{
    public interface IDialogueManager
    {
        void StartDialogue(string dialogueId);
        void EndDialogue();
        bool IsInDialogue();
    }
} 