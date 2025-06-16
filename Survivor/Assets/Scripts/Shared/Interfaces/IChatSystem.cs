using UnityEngine;

namespace Survivor.Shared.Interfaces
{
    public interface IChatSystem
    {
        void AddMessage(string message, Color color);
        void ClearChat();
        void FocusInputField();
    }
} 