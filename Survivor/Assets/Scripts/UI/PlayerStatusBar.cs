using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Characters;
using System.Collections;

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

        private Character playerCharacter;

        private void Start()
        {
            // Ensure all bars are set to fill type
            SetupBar(energyBarFill, "Energy");
            SetupBar(hungerBarFill, "Hunger");
            SetupBar(thirstBarFill, "Thirst");

            StartCoroutine(WaitForPlayer());
        }

        private void SetupBar(Image bar, string barName)
        {
            if (bar != null)
            {
                bar.type = Image.Type.Filled;
                bar.fillMethod = Image.FillMethod.Horizontal;
                bar.fillOrigin = (int)Image.OriginHorizontal.Left;
                bar.fillAmount = 1f; // Start full
                Debug.Log($"[PlayerStatusBar] Set up {barName} bar with fill amount: {bar.fillAmount}");
            }
            else
            {
                Debug.LogError($"[PlayerStatusBar] {barName} bar fill image is not assigned!");
            }
        }

        private IEnumerator WaitForPlayer()
        {
            // Wait for the player to be spawned
            while (playerCharacter == null)
            {
                var characters = FindObjectsOfType<Character>();
                foreach (var character in characters)
                {
                    if (character.IsPlayer)
                    {
                        playerCharacter = character;
                        break;
                    }
                }

                if (playerCharacter == null)
                {
                    yield return new WaitForSeconds(0.1f); // Wait a bit before trying again
                }
            }

            UpdateStatusDisplay();
        }

        private void Update()
        {
            if (playerCharacter != null)
            {
                UpdateStatusDisplay();
            }
        }

        private void UpdateStatusDisplay()
        {
            var stats = playerCharacter.Stats;

            // Update energy
            if (energyBarFill != null)
            {
                float energyFill = stats.energy / 100f;
                energyBarFill.fillAmount = energyFill;
                Debug.Log($"[PlayerStatusBar] Energy fill amount: {energyFill}");
            }
            if (energyText != null)
            {
                energyText.text = $"{stats.energy:F0}%";
            }

            // Update hunger
            if (hungerBarFill != null)
            {
                float hungerFill = stats.hunger / 10f;
                hungerBarFill.fillAmount = hungerFill;
                Debug.Log($"[PlayerStatusBar] Hunger fill amount: {hungerFill}");
            }
            if (hungerText != null)
            {
                hungerText.text = $"{stats.hunger:F1}/10";
            }

            // Update thirst
            if (thirstBarFill != null)
            {
                float thirstFill = stats.thirst / 10f;
                thirstBarFill.fillAmount = thirstFill;
                Debug.Log($"[PlayerStatusBar] Thirst fill amount: {thirstFill}");
            }
            if (thirstText != null)
            {
                thirstText.text = $"{stats.thirst:F1}/10";
            }
        }
    }
} 