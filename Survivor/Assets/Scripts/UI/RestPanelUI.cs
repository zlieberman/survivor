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
            float restHours = restHoursSlider != null ? restHoursSlider.value : 1f;
            onSleepCallback?.Invoke(restHours);
        }

        private void OnCancelButtonClicked()
        {
            onCancelCallback?.Invoke();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
} 