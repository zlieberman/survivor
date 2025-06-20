using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Survivor.Characters;
using System.Collections.Generic;

namespace Survivor.UI
{
    public class TribeInfoMenuController : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject menuPanelPrefab;
        [SerializeField] private GameObject npcEntryPrefab;
        [SerializeField] private GameObject statsPanelPrefab;

        [Header("Settings")]
        [SerializeField] private float updateInterval = 1f;

        private bool isMenuOpen;
        private float nextUpdateTime;
        private GameObject menuPanel;
        private Transform contentParent;
        private GameObject currentStatsPanel;
        private List<GameObject> npcEntries = new List<GameObject>();
        private TextMeshProUGUI headerText;
        
        // Store current tribe data
        private string currentTribeName;
        private List<Character> currentTribeMembers = new List<Character>();

        private void Awake()
        {
            Debug.Log("[TribeInfoMenuController] Awake called");
            
            // Verify prefabs are assigned
            if (menuPanelPrefab == null)
            {
                Debug.LogError("[TribeInfoMenuController] Menu Panel Prefab is not assigned!");
                return;
            }
            if (npcEntryPrefab == null)
            {
                Debug.LogError("[TribeInfoMenuController] NPC Entry Prefab is not assigned!");
                return;
            }
            if (statsPanelPrefab == null)
            {
                Debug.LogError("[TribeInfoMenuController] Stats Panel Prefab is not assigned!");
                return;
            }

            // Ensure we have a Canvas
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.Log("[TribeInfoMenuController] Adding Canvas component");
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100; // Ensure it's above other UI elements
                
                CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                
                gameObject.AddComponent<GraphicRaycaster>();
            }

            // Create the menu panel
            menuPanel = Instantiate(menuPanelPrefab, transform);
            menuPanel.name = "Menu Panel";
            RectTransform menuPanelRect = menuPanel.GetComponent<RectTransform>();
            menuPanelRect.anchorMin = new Vector2(1f, 0f);
            menuPanelRect.anchorMax = new Vector2(1f, 0f);
            menuPanelRect.pivot = new Vector2(1f, 0f);
            menuPanelRect.anchoredPosition = new Vector2(-20f, 20f);
            menuPanelRect.sizeDelta = new Vector2(300f, 400f);

            // Get the header text component
            Transform headerTransform = menuPanel.transform.Find("Header");
            if (headerTransform != null)
            {
                headerText = headerTransform.GetComponent<TextMeshProUGUI>();
            }

            // Get the content parent from the panel
            contentParent = menuPanel.transform.Find("Content");
            if (contentParent == null)
            {
                Debug.LogError("[TribeInfoMenuController] Content parent not found in menu panel prefab!");
                return;
            }

            // Add VerticalLayoutGroup to content parent
            VerticalLayoutGroup layoutGroup = contentParent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = contentParent.gameObject.AddComponent<VerticalLayoutGroup>();
                layoutGroup.spacing = 5f;
                layoutGroup.padding = new RectOffset(5, 5, 5, 5);
                layoutGroup.childAlignment = TextAnchor.UpperCenter;
                layoutGroup.childForceExpandWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childControlHeight = true;
            }

            // Add ContentSizeFitter to content parent
            ContentSizeFitter sizeFitter = contentParent.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
                sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            // Initial setup
            menuPanel.SetActive(false);
            isMenuOpen = false;

            Debug.Log("[TribeInfoMenuController] Initialization complete!");
        }

        private void Update()
        {
            // Update UI if menu is open and enough time has passed
            if (isMenuOpen && Time.time >= nextUpdateTime)
            {
                nextUpdateTime = Time.time + updateInterval;
                UpdateTribeInfo();
            }
        }

        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            menuPanel.SetActive(isMenuOpen);
            
            if (isMenuOpen)
            {
                UpdateTribeInfo();
            }
            else if (currentStatsPanel != null)
            {
                Destroy(currentStatsPanel);
                currentStatsPanel = null;
            }
        }

        /// <summary>
        /// Opens the menu if it's closed, or closes it if it's open
        /// </summary>
        public void ToggleMenu(bool forceOpen = false)
        {
            if (forceOpen)
            {
                isMenuOpen = true;
                menuPanel.SetActive(true);
                UpdateTribeInfo();
            }
            else
            {
                ToggleMenu();
            }
        }

        /// <summary>
        /// Public method to set tribe data from the manager
        /// </summary>
        /// <param name="tribeName">Name of the tribe</param>
        /// <param name="tribeMembers">List of tribe members</param>
        public void SetTribeInfo(string tribeName, List<Character> tribeMembers)
        {
            Debug.Log($"[TribeInfoMenuController] Setting tribe info for {tribeName} with {tribeMembers?.Count ?? 0} members");
            
            currentTribeName = tribeName;
            currentTribeMembers = tribeMembers ?? new List<Character>();
            
            // Update the UI if the menu is currently open
            if (isMenuOpen)
            {
                UpdateTribeInfo();
            }
        }

        private void UpdateTribeInfo()
        {
            Debug.Log("[TribeInfoMenuController] Updating tribe info");
            
            // Clear existing entries
            foreach (var entry in npcEntries)
            {
                Destroy(entry);
            }
            npcEntries.Clear();

            // Check if we have tribe data
            if (string.IsNullOrEmpty(currentTribeName) || currentTribeMembers == null || currentTribeMembers.Count == 0)
            {
                Debug.LogWarning("[TribeInfoMenuController] No tribe data available to display!");
                if (headerText != null)
                {
                    headerText.text = "No Tribe Data";
                }
                return;
            }

            // Update header text with tribe name
            if (headerText != null)
            {
                headerText.text = $"{currentTribeName} Tribe";
            }

            Debug.Log($"[TribeInfoMenuController] Found {currentTribeMembers.Count} tribe members");

            // Create entries for each NPC
            foreach (var npc in currentTribeMembers)
            {
                if (npc.IsPlayer) continue; // Skip player

                var entryObj = Instantiate(npcEntryPrefab, contentParent);
                var entry = entryObj.GetComponent<TribeInfoEntry>();
                
                if (entry != null)
                {
                    entry.statsPanelPrefab = statsPanelPrefab; // Set the stats panel prefab
                    entry.Initialize(npc, OnEntryClicked);
                    npcEntries.Add(entryObj);
                    Debug.Log($"[TribeInfoMenuController] Created entry for NPC: {npc.CharacterName}");
                }
                else
                {
                    Debug.LogError($"[TribeInfoMenuController] Failed to get TribeInfoEntry component for NPC: {npc.CharacterName}");
                }
            }
        }

        private void OnEntryClicked(Character npc)
        {
            Debug.Log($"[TribeInfoMenuController] Entry clicked for NPC: {npc.CharacterName}");
            
            // Close existing stats panel if any
            if (currentStatsPanel != null)
            {
                Destroy(currentStatsPanel);
            }

            // Create new stats panel
            currentStatsPanel = Instantiate(statsPanelPrefab, transform);
            var statsPanel = currentStatsPanel.GetComponent<TribeInfoStatsPanel>();
            
            if (statsPanel != null)
            {
                statsPanel.Initialize(npc);
            }
        }
    }
} 