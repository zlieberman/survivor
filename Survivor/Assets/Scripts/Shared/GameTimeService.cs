using UnityEngine;
using System;

namespace Survivor.Shared
{
    /// <summary>
    /// Static service for managing game time that can be used by any assembly
    /// </summary>
    public static class GameTimeService
    {
        /// <summary>
        /// Event triggered when the time provider is registered
        /// </summary>
        public static event Action<ITimeProvider> OnTimeProviderRegistered;
        
        /// <summary>
        /// Event triggered when the time provider is unregistered
        /// </summary>
        public static event Action OnTimeProviderUnregistered;
        
        private static ITimeProvider _timeProvider;
        
        /// <summary>
        /// Gets the current time provider
        /// </summary>
        public static ITimeProvider TimeProvider => _timeProvider;
        
        /// <summary>
        /// Gets whether a time provider is currently registered
        /// </summary>
        public static bool HasTimeProvider => _timeProvider != null;
        
        /// <summary>
        /// Registers a time provider
        /// </summary>
        /// <param name="provider">The time provider to register</param>
        public static void RegisterTimeProvider(ITimeProvider provider)
        {
            _timeProvider = provider;
            OnTimeProviderRegistered?.Invoke(provider);
            Debug.Log("[GameTimeService] Time provider registered");
        }
        
        /// <summary>
        /// Unregisters the current time provider
        /// </summary>
        public static void UnregisterTimeProvider()
        {
            _timeProvider = null;
            OnTimeProviderUnregistered?.Invoke();
            Debug.Log("[GameTimeService] Time provider unregistered");
        }
        
        /// <summary>
        /// Gets the current game time in hours and minutes
        /// </summary>
        /// <returns>Tuple of (hour, minute, day)</returns>
        public static (int hour, int minute, int day) GetCurrentGameTime()
        {
            if (_timeProvider == null)
            {
                Debug.LogWarning("[GameTimeService] No time provider registered");
                return (10, 0, 1); // Default to 10:00 AM if no provider
            }
            
            float elapsedRealTime = _timeProvider.ElapsedRealTime;
            float realTimePerGameHour = _timeProvider.RealTimePerGameHour;
            float totalGameHours = elapsedRealTime / realTimePerGameHour;
            
            // Get starting time from the time provider
            int startHour = _timeProvider.StartHour;
            int startMinute = _timeProvider.StartMinute;
            
            // Calculate total time including starting offset
            float totalTimeInHours = totalGameHours + startHour + (startMinute / 60f);
            
            int gameHour = Mathf.FloorToInt(totalTimeInHours) % 24;
            int gameDay = Mathf.FloorToInt(totalTimeInHours / 24f) + 1;
            
            float fractionalHour = totalTimeInHours - Mathf.FloorToInt(totalTimeInHours);
            int gameMinute = Mathf.FloorToInt(fractionalHour * 60f);
            
            return (gameHour, gameMinute, gameDay);
        }
        
        /// <summary>
        /// Gets the formatted time string
        /// </summary>
        /// <param name="use24HourFormat">Whether to use 24-hour format</param>
        /// <returns>Formatted time string</returns>
        public static string GetFormattedTime(bool use24HourFormat = true)
        {
            var (hour, minute, _) = GetCurrentGameTime();
            
            if (use24HourFormat)
            {
                return $"{hour:D2}:{minute:D2}";
            }
            else
            {
                int displayHour = hour == 0 ? 12 : (hour > 12 ? hour - 12 : hour);
                string ampm = hour >= 12 ? "PM" : "AM";
                return $"{displayHour:D2}:{minute:D2} {ampm}";
            }
        }
    }
} 