using UnityEngine;
using System.Collections.Generic;
using Survivor.Shared;

namespace Survivor.Interactables
{
    public class InteractableManager : MonoBehaviour
    {
        public static InteractableManager Instance { get; private set; }

        [Header("Interactable Tracking")]
        private List<IGameHourListener> gameHourListeners = new List<IGameHourListener>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Register an interactable that needs game hour updates
        /// </summary>
        public void RegisterGameHourListener(IGameHourListener listener)
        {
            if (!gameHourListeners.Contains(listener))
            {
                gameHourListeners.Add(listener);
                Debug.Log($"[InteractableManager] Registered game hour listener: {listener}");
            }
        }

        /// <summary>
        /// Unregister an interactable that no longer needs game hour updates
        /// </summary>
        public void UnregisterGameHourListener(IGameHourListener listener)
        {
            if (gameHourListeners.Contains(listener))
            {
                gameHourListeners.Remove(listener);
                Debug.Log($"[InteractableManager] Unregistered game hour listener: {listener}");
            }
        }

        /// <summary>
        /// Get all registered game hour listeners
        /// </summary>
        public List<IGameHourListener> GetGameHourListeners()
        {
            return new List<IGameHourListener>(gameHourListeners);
        }

        /// <summary>
        /// Get the count of registered listeners
        /// </summary>
        public int GetListenerCount()
        {
            return gameHourListeners.Count;
        }
    }
} 