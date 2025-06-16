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
        private Character playerCharacter;
        private TribeManager tribeManager;
        private PlayerManager playerManager;
        private TextMeshProUGUI headerText;

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
            menuPanelRect.anchorMin = new Vector2(1f, 1f);
            menuPanelRect.anchorMax = new Vector2(1f, 1f);
            menuPanelRect.pivot = new Vector2(1f, 1f);
            menuPanelRect.anchoredPosition = new Vector2(-20f, -80f);
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
            // Check for T key press
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("[TribeInfoMenuController] T key pressed!");
                ToggleMenu();
            }

            // Try to find required components if not found yet
            if (playerManager == null)
            {
                playerManager = FindObjectOfType<PlayerManager>();
                if (playerManager != null)
                {
                    Debug.Log("[TribeInfoMenuController] Found PlayerManager!");
                    // Debug all registered players
                    var allPlayers = playerManager.GetAllPlayers();
                    Debug.Log($"[TribeInfoMenuController] PlayerManager has {allPlayers.Count} registered players");
                    foreach (var player in allPlayers)
                    {
                        Debug.Log($"[TribeInfoMenuController] Registered player: {player.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("[TribeInfoMenuController] PlayerManager not found in scene!");
                }
            }
            else if (playerCharacter == null)
            {
                // Try to find the player character
                var allCharacters = FindObjectsOfType<Character>();
                Debug.Log($"[TribeInfoMenuController] Found {allCharacters.Length} Character components in scene");
                foreach (var character in allCharacters)
                {
                    Debug.Log($"[TribeInfoMenuController] Found Character: {character.name}, IsPlayer: {character.IsPlayer}, Tribe: {character.TribeName}");
                    if (character.IsPlayer)
                    {
                        playerCharacter = character;
                        Debug.Log($"[TribeInfoMenuController] Found player character: {character.name}");
                        break;
                    }
                }
            }

            if (tribeManager == null)
            {
                tribeManager = FindObjectOfType<TribeManager>();
                if (tribeManager != null)
                {
                    Debug.Log("[TribeInfoMenuController] Found TribeManager!");
                    // Debug all tribes
                    var allTribes = tribeManager.GetAllTribeMembers();
                    Debug.Log($"[TribeInfoMenuController] TribeManager has {allTribes.Count} total tribe members");
                    foreach (var member in allTribes)
                    {
                        Debug.Log($"[TribeInfoMenuController] Tribe member: {member.CharacterName}, Tribe: {member.TribeName}, IsPlayer: {member.IsPlayer}");
                    }
                }
                else
                {
                    Debug.LogWarning("[TribeInfoMenuController] TribeManager not found in scene!");
                }
            }

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

        private void UpdateTribeInfo()
        {
            Debug.Log("[TribeInfoMenuController] Updating tribe info");
            
            // Clear existing entries
            foreach (var entry in npcEntries)
            {
                Destroy(entry);
            }
            npcEntries.Clear();

            // Check if we have all required components
            if (playerCharacter == null)
            {
                Debug.LogWarning("[TribeInfoMenuController] Player character not found! Attempting to find player...");
                var allCharacters = FindObjectsOfType<Character>();
                foreach (var character in allCharacters)
                {
                    if (character.IsPlayer)
                    {
                        playerCharacter = character;
                        Debug.Log($"[TribeInfoMenuController] Found player character: {character.name}");
                        break;
                    }
                }
                
                if (playerCharacter == null)
                {
                    Debug.LogError("[TribeInfoMenuController] Failed to find player character after multiple attempts!");
                    return;
                }
            }

            if (tribeManager == null)
            {
                Debug.LogWarning("[TribeInfoMenuController] TribeManager not found! Attempting to find TribeManager...");
                tribeManager = FindObjectOfType<TribeManager>();
                if (tribeManager == null)
                {
                    Debug.LogError("[TribeInfoMenuController] Failed to find TribeManager!");
                    return;
                }
                Debug.Log("[TribeInfoMenuController] Found TribeManager!");
            }

            // Update header text with tribe name
            if (headerText != null)
            {
                headerText.text = $"{playerCharacter.TribeName} Tribe";
            }

            // Get tribe members
            var tribeMembers = tribeManager.GetTribeMembers(playerCharacter.TribeName);
            if (tribeMembers == null || tribeMembers.Count == 0)
            {
                Debug.LogWarning($"[TribeInfoMenuController] No tribe members found for tribe: {playerCharacter.TribeName}");
                return;
            }

            Debug.Log($"[TribeInfoMenuController] Found {tribeMembers.Count} tribe members");

            // Create entries for each NPC
            foreach (var npc in tribeMembers)
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