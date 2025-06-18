using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Survivor.Interactables
{
    public class InteractionUISetup : MonoBehaviour
    {
        public static void SetupInteractionUI(InteractionManager interactionManager)
        {
            // Create Canvas if it doesn't exist
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("InteractionCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }

            // Create Panel
            GameObject panelObj = new GameObject("InteractionPrompt");
            panelObj.transform.SetParent(canvas.transform, false);
            Image panel = panelObj.AddComponent<Image>();
            panel.color = new Color(0, 0, 0, 0.7f);
            
            // Set Panel RectTransform
            RectTransform panelRect = panelObj.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.1f);
            panelRect.anchorMax = new Vector2(0.5f, 0.1f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(300, 50);
            panelRect.anchoredPosition = Vector2.zero;

            // Create Text
            GameObject textObj = new GameObject("InteractionText");
            textObj.transform.SetParent(panelObj.transform, false);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Press E to interact";
            text.color = Color.white;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            
            // Set Text RectTransform
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            // Set up InteractionManager references
            interactionManager.interactionPrompt = panelObj;
            interactionManager.interactionText = text;

            // Hide the prompt initially
            panelObj.SetActive(false);
        }
    }
} 