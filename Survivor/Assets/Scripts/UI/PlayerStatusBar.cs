using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Shared;

namespace Survivor.UI
{
    public class PlayerStatusBar : MonoBehaviour
    {
        [Header("Status Bars")]
        [SerializeField] private Image energyBarFill;    // This should be the child fill image
        [SerializeField] private Image hungerBarFill;    // This should be the child fill image
        [SerializeField] private Image thirstBarFill;    // This should be the child fill image

        [Header("Status Text")]
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI hungerText;
        [SerializeField] private TextMeshProUGUI thirstText;

        private bool isInitialized = false;

        private void Update()
        {
            // Keep trying to find and initialize with the player until successful
            if (!isInitialized)
            {
                var player = FindObjectOfType<Survivor.Characters.Character>();
                if (player != null && player.IsPlayer)
                {
                    SetStatus(player.Stats);
                    isInitialized = true;
                    Debug.Log("[PlayerStatusBar] Successfully initialized with player stats");
                }
            }
        }

        public void SetStatus(CharacterStats data)
        {
            // Energy
            if (energyBarFill != null)
                energyBarFill.fillAmount = data.energy / 100f;
            if (energyText != null)
                energyText.text = $"{data.energy:F0}%";

            // Hunger
            if (hungerBarFill != null)
                hungerBarFill.fillAmount = data.hunger / 10f;
            if (hungerText != null)
                hungerText.text = $"{data.hunger:F1}/10";

            // Thirst
            if (thirstBarFill != null)
                thirstBarFill.fillAmount = data.thirst / 10f;
            if (thirstText != null)
                thirstText.text = $"{data.thirst:F1}/10";
        }
    }
} 