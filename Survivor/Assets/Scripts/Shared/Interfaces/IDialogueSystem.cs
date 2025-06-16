using System;
using System.Threading.Tasks;
using Survivor.Shared;

namespace Survivor.Shared.Interfaces
{
    public interface IDialogueSystem
    {
        event Action<string> OnDialogueLine;
        event Action<bool> OnDialogueStateChanged;
        
        bool IsInDialogue { get; }
        void StartDialogue(string npcName, string initialMessage);
        void EndDialogue();
        Task<string> GenerateResponse(string playerMessage);
        void SetInteractable(IDialogueInteractable interactable, bool inRange);
    }
} 