using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Characters;

namespace Survivor.UI
{
    public class TribeInfoStatsPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button closeButton;
        
        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseClicked);
            }
        }

        public void Initialize(Character npc)
        {
            // Position the panel
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(-20f, -20f);

            // Update UI
            if (nameText != null)
            {
                nameText.text = npc.CharacterName;
            }

            if (statsText != null)
            {
                statsText.text = FormatStats(npc);
            }
        }

        private string FormatStats(Character npc)
        {
            return $"Tribe: {npc.TribeName}\n" +
                   $"Perception: {npc.Stats.perception}\n" +
                   $"Deception: {npc.Stats.deception}\n" +
                   $"Persuasion: {npc.Stats.persuasion}\n" +
                   $"Puzzle Solving: {npc.Stats.puzzleSolving}\n" +
                   $"Swimming: {npc.Stats.swimming}\n" +
                   $"Speed: {npc.Stats.speed}\n" +
                   $"Strength: {npc.Stats.strength}\n" +
                   $"Agility: {npc.Stats.agility}\n" +
                   $"Intelligence: {npc.Stats.intelligence}\n" +
                   $"Stamina: {npc.Stats.stamina}\n" +
                   $"Charisma: {npc.Stats.charisma}\n" +
                   $"Honesty: {npc.Stats.honesty}\n" +
                   $"Trust: {npc.Stats.trust}\n" +
                   $"Honor: {npc.Stats.honor}";
        }

        private void OnCloseClicked()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
            }
        }
    }
} 