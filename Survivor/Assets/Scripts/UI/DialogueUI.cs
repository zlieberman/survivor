using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Survivor.Shared;
using Survivor.Shared.Interfaces;
using System;

namespace Survivor.UI
{
    public class DialogueUI : MonoBehaviour, IDialogueUI
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI npcNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TMP_InputField playerInput;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;

        // Public property to access sendButton for external event wiring
        public Button SendButton => sendButton;

        // Event that the DialogueManager can subscribe to
        public event Action OnDialogueClosed;

        private bool isProcessingResponse = false;

        private void Start()
        {
            // Set up UI event listeners
            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SendMessage);
            }
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseDialogue);
            }
            if (playerInput != null)
            {
                playerInput.onSubmit.AddListener(_ => SendMessage());
            }

            // Hide dialogue panel initially
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }
        }

        /// <summary>
        /// Set the dialogue UI state and content from the manager/controller
        /// </summary>
        public void SetDialogueState(bool isInDialogue, string npcName = null, string message = null)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(isInDialogue);
            }

            if (isInDialogue)
            {
                if (npcNameText != null && npcName != null)
                {
                    npcNameText.text = npcName;
                }
                if (dialogueText != null && message != null)
                {
                    dialogueText.text = message;
                }
                if (playerInput != null)
                {
                    playerInput.ActivateInputField();
                }
            }
            else
            {
                if (playerInput != null)
                {
                    playerInput.text = string.Empty;
                }
            }
        }

        public void SetDialogueLine(string message)
        {
            if (dialogueText != null)
            {
                dialogueText.text = message;
            }
        }

        public void ShowDialogue(string npcName, string initialMessage)
        {
            SetDialogueState(true, npcName, initialMessage);
        }

        public void CloseDialogue()
        {
            SetDialogueState(false);
            
            // Raise the event for the DialogueManager to handle
            OnDialogueClosed?.Invoke();
        }

        // The manager/controller should call this and handle async/response logic
        public async void SendMessage()
        {
            if (isProcessingResponse || playerInput == null || string.IsNullOrWhiteSpace(playerInput.text))
            {
                return;
            }

            string message = playerInput.text;
            playerInput.text = string.Empty;
            isProcessingResponse = true;

            // The manager/controller should handle the response and call SetDialogueLine
            // This method can be left empty or raise an event if needed
            isProcessingResponse = false;
            if (playerInput != null)
            {
                playerInput.ActivateInputField();
            }
        }
    }
} 