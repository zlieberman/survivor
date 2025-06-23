using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Survivor.UI
{
    public class RestPanelUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] public Slider restHoursSlider;
        [SerializeField] public TextMeshProUGUI restHoursText;
        [SerializeField] public TextMeshProUGUI energyGainText;
        [SerializeField] public Button sleepButton;
        [SerializeField] public Button cancelButton;
        [SerializeField] public TextMeshProUGUI titleText;
        
        [Header("Settings")]
        [SerializeField] private float energyPerHour = 5f;
        [SerializeField] private float maxRestHours = 12f;
        
        private System.Action<float> onSleepCallback;
        private System.Action onCancelCallback;
        private bool isProcessing = false; // Flag to prevent multiple button clicks

        private void Start()
        {
            SetupUI();
        }

        private void SetupUI()
        {
            // Set up slider
            if (restHoursSlider != null)
            {
                restHoursSlider.minValue = 1f;
                restHoursSlider.maxValue = maxRestHours;
                restHoursSlider.value = 1f;
                restHoursSlider.onValueChanged.AddListener(OnRestHoursChanged);
            }

            // Set up buttons
            if (sleepButton != null)
            {
                sleepButton.onClick.AddListener(OnSleepButtonClicked);
            }
            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }

            // Set title
            if (titleText != null)
            {
                titleText.text = "Rest in Tent";
            }

            // Initial update
            OnRestHoursChanged(1f);
        }

        public void Initialize(float energyPerHour, float maxRestHours, System.Action<float> onSleep, System.Action onCancel)
        {
            this.energyPerHour = energyPerHour;
            this.maxRestHours = maxRestHours;
            this.onSleepCallback = onSleep;
            this.onCancelCallback = onCancel;
            
            SetupUI();
        }

        private void OnRestHoursChanged(float hours)
        {
            float energyGain = hours * energyPerHour;
            
            if (restHoursText != null)
            {
                restHoursText.text = $"Rest for {hours:F1} hours";
            }
            
            if (energyGainText != null)
            {
                energyGainText.text = $"Energy gain: +{energyGain:F0}";
            }
        }

        private void OnSleepButtonClicked()
        {
            // Prevent multiple clicks
            if (isProcessing)
            {
                Debug.Log("[RestPanelUI] Already processing sleep request, ignoring duplicate click");
                return;
            }
            
            isProcessing = true;
            Debug.Log("[RestPanelUI] Sleep button clicked, processing rest request");
            
            // Disable the button to prevent further clicks
            if (sleepButton != null)
            {
                sleepButton.interactable = false;
            }
            
            float restHours = restHoursSlider != null ? restHoursSlider.value : 1f;
            onSleepCallback?.Invoke(restHours);
            
            // Note: Processing flag will be reset when Show() is called next time
            // This prevents issues with inactive GameObjects and coroutines
        }

        private void OnCancelButtonClicked()
        {
            // Prevent multiple clicks
            if (isProcessing)
            {
                Debug.Log("[RestPanelUI] Already processing request, ignoring duplicate cancel click");
                return;
            }
            
            isProcessing = true;
            Debug.Log("[RestPanelUI] Cancel button clicked");
            
            // Disable buttons to prevent further clicks
            if (sleepButton != null)
            {
                sleepButton.interactable = false;
            }
            if (cancelButton != null)
            {
                cancelButton.interactable = false;
            }
            
            onCancelCallback?.Invoke();
            
            // Note: Processing flag will be reset when Show() is called next time
            // This prevents issues with inactive GameObjects and coroutines
        }

        public void Show()
        {
            Debug.Log("[RestPanelUI] Show() called");
            
            // Reset processing state
            isProcessing = false;
            
            // Ensure buttons are enabled
            if (sleepButton != null)
            {
                sleepButton.interactable = true;
            }
            if (cancelButton != null)
            {
                cancelButton.interactable = true;
            }
            
            // Activate the parent GameObject (this component's GameObject)
            gameObject.SetActive(true);
            Debug.Log($"[RestPanelUI] Parent GameObject '{gameObject.name}' activated: {gameObject.activeInHierarchy}");
            
            // Also activate the child RestPanel if it exists
            Transform restPanelChild = transform.Find("RestPanel");
            if (restPanelChild != null)
            {
                restPanelChild.gameObject.SetActive(true);
                Debug.Log($"[RestPanelUI] Child RestPanel '{restPanelChild.name}' activated: {restPanelChild.gameObject.activeInHierarchy}");
            }
            else
            {
                Debug.LogWarning("[RestPanelUI] RestPanel child not found, UI may not be visible");
            }
        }

        public void Hide()
        {
            Debug.Log("[RestPanelUI] Hide() called");
            
            // Deactivate the parent GameObject (this component's GameObject)
            gameObject.SetActive(false);
            
            // Also deactivate the child RestPanel if it exists
            Transform restPanelChild = transform.Find("RestPanel");
            if (restPanelChild != null)
            {
                restPanelChild.gameObject.SetActive(false);
            }
        }

        public void ResetProcessingState()
        {
            isProcessing = false;
            if (sleepButton != null)
            {
                sleepButton.interactable = true;
            }
            if (cancelButton != null)
            {
                cancelButton.interactable = true;
            }
            Debug.Log("[RestPanelUI] Processing state reset");
        }
    }
} 