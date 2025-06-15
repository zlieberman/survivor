using UnityEngine;

namespace Survivor.Environment
{
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        [Header("Environment Settings")]
        public float dayNightCycleDuration = 300f; // 5 minutes
        public float currentTimeOfDay = 0f;
        public bool isDay = true;

        [Header("Environment Events")]
        public UnityEngine.Events.UnityEvent onDayStart;
        public UnityEngine.Events.UnityEvent onNightStart;

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

        public void Initialize()
        {
            // Reset time of day
            currentTimeOfDay = 0f;
            isDay = true;
            
            // Trigger initial day start event
            onDayStart?.Invoke();
        }

        private void Update()
        {
            // Update time of day
            currentTimeOfDay += Time.deltaTime;
            if (currentTimeOfDay >= dayNightCycleDuration)
            {
                currentTimeOfDay = 0f;
                isDay = !isDay;
                if (isDay)
                    onDayStart?.Invoke();
                else
                    onNightStart?.Invoke();
            }
        }

        public float GetDayNightProgress()
        {
            return currentTimeOfDay / dayNightCycleDuration;
        }

        public bool IsDay()
        {
            return isDay;
        }
    }
} 