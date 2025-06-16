using UnityEngine;
using TMPro;

namespace Survivor.Characters.UI
{
    public class CharacterUIManager : MonoBehaviour
    {
        private static CharacterUIManager instance;
        public static CharacterUIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<CharacterUIManager>();
                    if (instance == null)
                    {
                        Debug.LogWarning("CharacterUIManager not found in scene. Creating new instance.");
                        GameObject go = new GameObject("CharacterUIManager");
                        instance = go.AddComponent<CharacterUIManager>();
                    }
                }
                return instance;
            }
        }

        [Header("NPC Info")]
        public GameObject npcInfoPanel;
        public TextMeshProUGUI npcNameText;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialize UI elements
            if (npcInfoPanel != null)
                npcInfoPanel.SetActive(false);
        }

        public void ShowNPCInfo(string npcName)
        {
            if (npcInfoPanel != null && npcNameText != null)
            {
                npcNameText.text = npcName;
                npcInfoPanel.SetActive(true);
            }
        }

        public void HideNPCInfo()
        {
            if (npcInfoPanel != null)
                npcInfoPanel.SetActive(false);
        }
    }
} 