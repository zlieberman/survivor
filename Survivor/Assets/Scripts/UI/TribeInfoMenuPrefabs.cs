using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Survivor.UI
{
    public static class TribeInfoMenuPrefabs
    {
        public static GameObject CreateNPCEntryPrefab()
        {
            GameObject entryObj = new GameObject("NPC Entry");
            RectTransform rectTransform = entryObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 40f);

            // Add button component
            Button button = entryObj.AddComponent<Button>();
            Image buttonImage = entryObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Add TribeInfoEntry component
            TribeInfoEntry tribeInfoEntry = entryObj.AddComponent<TribeInfoEntry>();

            // Add name text component
            GameObject nameTextObj = new GameObject("Name Text");
            nameTextObj.transform.SetParent(entryObj.transform, false);
            RectTransform nameTextRect = nameTextObj.AddComponent<RectTransform>();
            nameTextRect.anchorMin = new Vector2(0, 0);
            nameTextRect.anchorMax = new Vector2(1, 1);
            nameTextRect.offsetMin = new Vector2(10f, 5f);
            nameTextRect.offsetMax = new Vector2(-10f, -5f);

            TextMeshProUGUI nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.color = Color.white;
            nameText.fontSize = 16;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.raycastTarget = false;

            // Add role text component
            GameObject roleTextObj = new GameObject("Role Text");
            roleTextObj.transform.SetParent(entryObj.transform, false);
            RectTransform roleTextRect = roleTextObj.AddComponent<RectTransform>();
            roleTextRect.anchorMin = new Vector2(0, 0);
            roleTextRect.anchorMax = new Vector2(1, 1);
            roleTextRect.offsetMin = new Vector2(10f, 5f);
            roleTextRect.offsetMax = new Vector2(-10f, -5f);

            TextMeshProUGUI roleText = roleTextObj.AddComponent<TextMeshProUGUI>();
            roleText.color = Color.white;
            roleText.fontSize = 14;
            roleText.alignment = TextAlignmentOptions.Right;
            roleText.raycastTarget = false;

            // Set up TribeInfoEntry references
            tribeInfoEntry.nameText = nameText;
            tribeInfoEntry.roleText = roleText;
            tribeInfoEntry.backgroundImage = buttonImage;

            return entryObj;
        }

        public static GameObject CreateStatsPanelPrefab()
        {
            GameObject panelObj = new GameObject("Stats Panel");
            RectTransform rectTransform = panelObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(300f, 400f);

            // Add panel background
            Image panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            // Add text component
            GameObject textObj = new GameObject("Stats Text");
            textObj.transform.SetParent(panelObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 50f);
            textRect.offsetMax = new Vector2(-20f, -20f);

            TMP_Text text = textObj.AddComponent<TextMeshProUGUI>();
            text.color = Color.white;
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Left;

            // Add close button
            GameObject closeButtonObj = new GameObject("Close Button");
            closeButtonObj.transform.SetParent(panelObj.transform, false);
            RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);
            closeButtonRect.sizeDelta = new Vector2(30f, 30f);
            closeButtonRect.anchoredPosition = new Vector2(-10f, -10f);

            Image closeButtonImage = closeButtonObj.AddComponent<Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);

            Button closeButton = closeButtonObj.AddComponent<Button>();
            ColorBlock colors = closeButton.colors;
            colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
            closeButton.colors = colors;

            // Add X text to close button
            GameObject closeTextObj = new GameObject("Close Text");
            closeTextObj.transform.SetParent(closeButtonObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;

            TMP_Text closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "X";
            closeText.color = Color.white;
            closeText.fontSize = 20;
            closeText.alignment = TextAlignmentOptions.Center;

            return panelObj;
        }
    }
} 