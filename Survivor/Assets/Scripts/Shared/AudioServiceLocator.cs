using UnityEngine;
using Survivor.Shared;

namespace Survivor.Shared
{
    /// <summary>
    /// Simple service locator for finding audio services in the scene.
    /// This provides a clean way to access audio functionality without creating dependencies.
    /// </summary>
    public static class AudioServiceLocator
    {
        private static IAudioService _cachedService;

        /// <summary>
        /// Gets the audio service from the scene. Caches the result for performance.
        /// </summary>
        /// <returns>The audio service if found, null otherwise</returns>
        public static IAudioService GetAudioService()
        {
            if (_cachedService != null)
            {
                return _cachedService;
            }

            // Find all MonoBehaviours and check if they implement IAudioService
            MonoBehaviour[] allBehaviours = Object.FindObjectsOfType<MonoBehaviour>();
            foreach (var behaviour in allBehaviours)
            {
                if (behaviour is IAudioService audioService)
                {
                    _cachedService = audioService;
                    return _cachedService;
                }
            }

            return null;
        }

        /// <summary>
        /// Clears the cached audio service. Call this when the audio service is destroyed.
        /// </summary>
        public static void ClearCache()
        {
            _cachedService = null;
        }

        /// <summary>
        /// Checks if an audio service is available in the scene.
        /// </summary>
        /// <returns>True if an audio service is found, false otherwise</returns>
        public static bool HasAudioService()
        {
            return GetAudioService() != null;
        }
    }
} 