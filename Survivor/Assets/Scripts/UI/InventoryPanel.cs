using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Shared;
using System.Collections.Generic;
using Survivor.Characters;

namespace Survivor.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class InventoryPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI inventoryText;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.I;
        [SerializeField] private float panelHeight = 300f;
        [SerializeField] private float panelWidth = 500f;

        private RectTransform rectTransform;
        private IInventory playerInventory;
        private bool isVisible = false;
        private bool isInitialized = false;

        private void Awake()
        {
            Debug.Log("[InventoryPanel] Awake called");
            rectTransform = GetComponent<RectTransform>();
            
            // Ensure we have a CanvasGroup
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            // Verify we're in a Canvas
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("[InventoryPanel] InventoryPanel must be a child of a Canvas!");
            }
            else
            {
                Debug.Log($"[InventoryPanel] Found parent Canvas: {parentCanvas.name}");
            }

            // Set initial visibility
            SetVisibility(false);
        }

        private void Start()
        {
            Debug.Log("[InventoryPanel] Start called");
            TryFindPlayer();
        }

        private void Update()
        {
            // Keep trying to find the player if we haven't found them yet
            if (!isInitialized)
            {
                TryFindPlayer();
            }

            if (Input.GetKeyDown(toggleKey))
            {
                Debug.Log($"[InventoryPanel] Toggle key {toggleKey} pressed");
                ToggleVisibility();
            }
        }

        private void TryFindPlayer()
        {
            if (isInitialized) return;

            PlayerCharacter player = FindObjectOfType<PlayerCharacter>();
            if (player != null)
            {
                Debug.Log("[InventoryPanel] Found PlayerCharacter");
                playerInventory = player;
                playerInventory.OnInventoryChanged += UpdateInventoryDisplay;
                UpdateInventoryDisplay();
                isInitialized = true;
            }
            else
            {
                Debug.Log("[InventoryPanel] Still looking for PlayerCharacter...");
            }
        }

        private void OnValidate()
        {
            Debug.Log("[InventoryPanel] OnValidate called");
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            // Set panel size
            rectTransform.sizeDelta = new Vector2(panelWidth, panelHeight);
            
            // Position at bottom left
            rectTransform.anchorMin = new Vector2(0, 0);
            rectTransform.anchorMax = new Vector2(0, 0);
            rectTransform.pivot = new Vector2(0, 0);
            rectTransform.anchoredPosition = new Vector2(20, 20); // Add some padding from the edge
        }

        private void ToggleVisibility()
        {
            Debug.Log($"[InventoryPanel] Toggling visibility from {isVisible} to {!isVisible}");
            SetVisibility(!isVisible);
        }

        private void SetVisibility(bool visible)
        {
            Debug.Log($"[InventoryPanel] Setting visibility to {visible}");
            isVisible = visible;
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void UpdateInventoryDisplay()
        {
            Debug.Log("[InventoryPanel] Updating inventory display");
            if (playerInventory == null || inventoryText == null)
            {
                Debug.LogError("[InventoryPanel] Missing required references for UpdateInventoryDisplay");
                return;
            }

            var items = playerInventory.GetAllItems();
            string displayText = "Inventory:\n";
            
            if (items.Count == 0)
            {
                displayText += "Empty";
            }
            else
            {
                foreach (var item in items)
                {
                    float itemWeight = playerInventory.GetItemWeight(item.Key);
                    displayText += $"{item.Key}: {item.Value} ({(itemWeight * item.Value):F1} weight)\n";
                }
            }

            float totalWeight = playerInventory.GetTotalWeight();
            float maxWeight = playerInventory.GetMaxWeight();
            displayText += $"\nWeight: {totalWeight:F1}/{maxWeight:F1}";
            inventoryText.text = displayText;
            Debug.Log($"[InventoryPanel] Updated display text: {displayText}");
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= UpdateInventoryDisplay;
            }
        }
    }
} 