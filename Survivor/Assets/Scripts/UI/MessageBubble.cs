using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Survivor.UI
{
    public class MessageBubble : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image bubbleBackground;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private LayoutElement layoutElement;

        [Header("Styling")]
        [SerializeField] private Color npcBubbleColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private Color playerBubbleColor = new Color(0.1f, 0.4f, 0.8f, 0.9f);
        [SerializeField] private float maxBubbleWidth = 400f;
        [SerializeField] private float minBubbleWidth = 100f;
        [SerializeField] private float padding = 20f;

        private void Awake()
        {
            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
            }
        }

        public void SetMessage(string message, bool isPlayer)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (bubbleBackground != null)
            {
                bubbleBackground.color = isPlayer ? playerBubbleColor : npcBubbleColor;
            }

            // Adjust layout based on message length
            if (layoutElement != null)
            {
                float preferredWidth = messageText.preferredWidth + padding;
                layoutElement.preferredWidth = Mathf.Clamp(preferredWidth, minBubbleWidth, maxBubbleWidth);
            }

            // Align the bubble to the right for player messages, left for NPC messages
            RectTransform rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(isPlayer ? 1 : 0, 0);
                rectTransform.anchorMax = new Vector2(isPlayer ? 1 : 0, 1);
                rectTransform.pivot = new Vector2(isPlayer ? 1 : 0, 0.5f);
            }
        }
    }
} 