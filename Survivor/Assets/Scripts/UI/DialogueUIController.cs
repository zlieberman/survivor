using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DialogueUIController : MonoBehaviour
{
    public static DialogueUIController Instance { get; private set; }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseDialogue);
        }
    }

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    public void ShowDialogue(string npcName, string dialogue)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }
    }

    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }
} 