using UnityEngine;
using TMPro;
using Survivor.Shared;
using System.Collections;

namespace Survivor.UI
{
    public class TimeDisplay : MonoBehaviour
    {
        [Header("Time Display")]
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI dayText;
        
        [Header("Time Format")]
        [SerializeField] private bool use24HourFormat = true;
        
        private int currentGameHour = 0;
        private int currentGameDay = 1;
        private int currentGameMinute = 0;
        private bool isInitialized = false;
        
        private void Start()
        {
            if (timeText == null)
            {
                Debug.LogError("[TimeDisplay] Time text component is not assigned!");
            }
            
            if (dayText == null)
            {
                Debug.LogWarning("[TimeDisplay] Day text component is not assigned - day display will be disabled");
            }
            
            // Subscribe to time provider events
            GameTimeService.OnTimeProviderRegistered += OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered += OnTimeProviderUnregistered;
            
            // Check if time provider is already available
            if (GameTimeService.HasTimeProvider)
            {
                OnTimeProviderRegistered(GameTimeService.TimeProvider);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameTimeService.OnTimeProviderRegistered -= OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered -= OnTimeProviderUnregistered;
        }
        
        private void OnTimeProviderRegistered(ITimeProvider provider)
        {
            Debug.Log("[TimeDisplay] Time provider registered, starting time display");
            isInitialized = true;
            UpdateTimeDisplay();
        }
        
        private void OnTimeProviderUnregistered()
        {
            Debug.Log("[TimeDisplay] Time provider unregistered");
            isInitialized = false;
            
            if (timeText != null)
            {
                timeText.text = "--:--";
            }
            
            if (dayText != null)
            {
                dayText.text = "Day --";
            }
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                UpdateTimeDisplay();
            }
        }
        
        private void UpdateTimeDisplay()
        {
            // Get current game time from the service
            var (gameHour, gameMinute, gameDay) = GameTimeService.GetCurrentGameTime();
            
            // Update if time has changed (hour, minute, or day)
            if (gameHour != currentGameHour || gameMinute != currentGameMinute || gameDay != currentGameDay)
            {
                currentGameHour = gameHour;
                currentGameMinute = gameMinute;
                currentGameDay = gameDay;
                
                // Format time as HH:MM
                string timeString = GameTimeService.GetFormattedTime(use24HourFormat);
                
                if (timeText != null)
                {
                    timeText.text = timeString;
                }
                
                if (dayText != null)
                {
                    dayText.text = $"Day {gameDay}";
                }                
            }
        }
        
        // Public method to get current game time for other systems
        public (int hour, int day) GetCurrentGameTime()
        {
            var (hour, _, day) = GameTimeService.GetCurrentGameTime();
            return (hour, day);
        }
        
        // Public method to get current game time with minutes
        public (int hour, int minute, int day) GetCurrentGameTimeWithMinutes()
        {
            return GameTimeService.GetCurrentGameTime();
        }
        
        // Public method to get formatted time string
        public string GetFormattedTime()
        {
            return GameTimeService.GetFormattedTime(use24HourFormat);
        }
    }
} 