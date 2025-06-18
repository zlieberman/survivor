using UnityEngine;
using Survivor.Characters;
using System.Collections;

namespace Survivor.Core
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float realTimePerGameHour = 120f; // 2 minutes = 1 hour

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

        private void Start()
        {
            StartCoroutine(UpdateGameTime());
        }

        private IEnumerator UpdateGameTime()
        {
            while (true)
            {
                yield return new WaitForSeconds(realTimePerGameHour);
                UpdatePlayerStats();
            }
        }

        private void UpdatePlayerStats()
        {
            // Find the player character
            var characters = FindObjectsOfType<Character>();
            Character playerCharacter = null;
            foreach (var character in characters)
            {
                if (character.IsPlayer)
                {
                    playerCharacter = character;
                    break;
                }
            }

            if (playerCharacter == null) return;

            var stats = playerCharacter.Stats;
            
            // Calculate stat modifiers based on stamina and grit
            float staminaModifier = 1f - (stats.stamina / 200f); // Higher stamina reduces negative effects
            float gritModifier = 1f - (stats.grit / 200f); // Higher grit reduces negative effects
            float combinedModifier = (staminaModifier + gritModifier) / 2f;

            // Update hunger (0.2-0.5 increase, reduced by stamina and grit)
            float hungerIncrease = Random.Range(0.2f, 0.5f) * combinedModifier;
            stats.hunger = Mathf.Clamp(stats.hunger + hungerIncrease, 0f, 10f);

            // Update thirst (1.5 increase, reduced by stamina and grit)
            float thirstIncrease = 1.5f * combinedModifier;
            stats.thirst = Mathf.Clamp(stats.thirst + thirstIncrease, 0f, 10f);

            // Update energy (1-5 decrease, reduced by stamina and grit)
            float energyDecrease = Random.Range(1f, 5f) * combinedModifier;
            stats.energy = Mathf.Clamp(stats.energy - energyDecrease, 0f, 100f);

            Debug.Log($"[TimeManager] Updated player stats - Hunger: +{hungerIncrease:F2}, Thirst: +{thirstIncrease:F2}, Energy: -{energyDecrease:F2}");
        }
    }
} 