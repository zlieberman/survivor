using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Survivor.Characters;

namespace Survivor.UI
{
    public class TribeInfoEntry : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI roleText;
        public Image backgroundImage;
        public GameObject statsPanelPrefab;
        
        private Button button;
        private Action<Character> onClickCallback;
        private Character npc;
        private bool isExpanded;
        private GameObject statsPanel;

        private void Awake()
        {
            button = GetComponent<Button>();
            if (button == null)
            {
                button = gameObject.AddComponent<Button>();
            }

            // Set up button colors
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
        }

        public void Initialize(Character npc, Action<Character> onClick)
        {
            this.npc = npc;
            this.onClickCallback = onClick;

            // Set up button click handler
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            // Update UI
            if (nameText != null)
            {
                nameText.text = npc.CharacterName;
            }

            if (roleText != null)
            {
                roleText.text = npc.TribeName;
            }

            // Set initial size
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280f, 40f);
        }

        private void OnClicked()
        {
            Debug.Log($"[TribeInfoEntry] Clicked on {npc.CharacterName}");
            isExpanded = !isExpanded;

            if (isExpanded)
            {
                // Expand the entry
                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(280f, 200f);

                // Create stats panel
                if (statsPanel == null && statsPanelPrefab != null)
                {
                    statsPanel = Instantiate(statsPanelPrefab, transform);
                    RectTransform statsRect = statsPanel.GetComponent<RectTransform>();
                    statsRect.anchorMin = new Vector2(0, 0);
                    statsRect.anchorMax = new Vector2(1, 1);
                    statsRect.offsetMin = new Vector2(10, 40);
                    statsRect.offsetMax = new Vector2(-10, -10);

                    var statsPanelComponent = statsPanel.GetComponent<TribeInfoStatsPanel>();
                    if (statsPanelComponent != null)
                    {
                        statsPanelComponent.Initialize(npc);
                    }
                }
            }
            else
            {
                // Collapse the entry
                RectTransform rectTransform = GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(280f, 40f);

                // Destroy stats panel
                if (statsPanel != null)
                {
                    Destroy(statsPanel);
                    statsPanel = null;
                }
            }

            onClickCallback?.Invoke(npc);
        }

        private void OnDestroy()
        {
            if (statsPanel != null)
            {
                Destroy(statsPanel);
            }
        }
    }
} 