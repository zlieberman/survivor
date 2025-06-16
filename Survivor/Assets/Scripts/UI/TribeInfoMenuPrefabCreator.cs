using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEngine.Events;

namespace Survivor.UI
{
    public class TribeInfoMenuPrefabCreator : MonoBehaviour
    {
        [MenuItem("Tools/Create Tribe Info Menu Prefabs")]
        public static void CreatePrefabs()
        {
            // Create menu button prefab
            GameObject menuButtonObj = new GameObject("MenuButton");
            RectTransform buttonRect = menuButtonObj.AddComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(50f, 50f);

            // Add CanvasGroup for better interaction control
            CanvasGroup canvasGroup = menuButtonObj.AddComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            Image buttonImage = menuButtonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            buttonImage.raycastTarget = true;

            Button button = menuButtonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
            button.targetGraphic = buttonImage;

            // Add our custom button component
            menuButtonObj.AddComponent<TribeInfoMenuButton>();

            GameObject iconTextObj = new GameObject("Icon");
            iconTextObj.transform.SetParent(menuButtonObj.transform, false);
            RectTransform iconRect = iconTextObj.AddComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;

            TMP_Text iconText = iconTextObj.AddComponent<TextMeshProUGUI>();
            iconText.text = "👥";
            iconText.color = Color.white;
            iconText.fontSize = 24;
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.raycastTarget = false;

            // Create menu panel prefab
            GameObject menuPanelObj = new GameObject("MenuPanel");
            RectTransform panelRect = menuPanelObj.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(250f, 600f);

            Image panelImage = menuPanelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            panelImage.raycastTarget = true;

            // Create content parent
            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(menuPanelObj.transform, false);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(10f, 10f);
            contentRect.offsetMax = new Vector2(-10f, -10f);

            VerticalLayoutGroup layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5f;
            layoutGroup.padding = new RectOffset(5, 5, 5, 5);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;

            ContentSizeFitter sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Add header text as first child of content
            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(contentObj.transform, false);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 40f);

            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "Tribe Members";
            headerText.color = Color.white;
            headerText.fontSize = 20;
            headerText.fontStyle = FontStyles.Bold;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.raycastTarget = false;

            // Create NPC entry prefab
            GameObject npcEntryObj = new GameObject("NPCEntry");
            RectTransform entryRect = npcEntryObj.AddComponent<RectTransform>();
            entryRect.sizeDelta = new Vector2(280f, 40f);

            Image entryImage = npcEntryObj.AddComponent<Image>();
            entryImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            entryImage.raycastTarget = true;

            Button entryButton = npcEntryObj.AddComponent<Button>();
            ColorBlock entryColors = entryButton.colors;
            entryColors.normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            entryColors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
            entryColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            entryButton.colors = entryColors;

            // Add TribeInfoEntry component
            TribeInfoEntry tribeInfoEntry = npcEntryObj.AddComponent<TribeInfoEntry>();

            GameObject entryTextObj = new GameObject("Text");
            entryTextObj.transform.SetParent(npcEntryObj.transform, false);
            RectTransform entryTextRect = entryTextObj.AddComponent<RectTransform>();
            entryTextRect.anchorMin = Vector2.zero;
            entryTextRect.anchorMax = Vector2.one;
            entryTextRect.offsetMin = new Vector2(10f, 5f);
            entryTextRect.offsetMax = new Vector2(-10f, -5f);

            TMP_Text entryText = entryTextObj.AddComponent<TextMeshProUGUI>();
            entryText.text = "NPC Name";
            entryText.color = Color.white;
            entryText.fontSize = 26;
            entryText.alignment = TextAlignmentOptions.Left;
            entryText.raycastTarget = false;

            // Add role text component
            GameObject roleTextObj = new GameObject("Role Text");
            roleTextObj.transform.SetParent(npcEntryObj.transform, false);
            RectTransform roleTextRect = roleTextObj.AddComponent<RectTransform>();
            roleTextRect.anchorMin = new Vector2(0, 0);
            roleTextRect.anchorMax = new Vector2(1, 1);
            roleTextRect.offsetMin = new Vector2(10f, 25f);
            roleTextRect.offsetMax = new Vector2(-10f, -25f);

            TextMeshProUGUI roleText = roleTextObj.AddComponent<TextMeshProUGUI>();
            roleText.text = "Role";
            roleText.color = Color.white;
            roleText.fontSize = 26;
            roleText.alignment = TextAlignmentOptions.Left;
            roleText.raycastTarget = false;

            // Set up TribeInfoEntry references
            tribeInfoEntry.nameText = (TextMeshProUGUI)entryText;
            tribeInfoEntry.roleText = roleText;
            tribeInfoEntry.backgroundImage = entryImage;

            // Create stats panel prefab
            GameObject statsPanelObj = new GameObject("StatsPanel");
            RectTransform statsRect = statsPanelObj.AddComponent<RectTransform>();
            statsRect.sizeDelta = new Vector2(300f, 400f);

            Image statsImage = statsPanelObj.AddComponent<Image>();
            statsImage.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
            statsImage.raycastTarget = true;

            GameObject statsTextObj = new GameObject("StatsText");
            statsTextObj.transform.SetParent(statsPanelObj.transform, false);
            RectTransform statsTextRect = statsTextObj.AddComponent<RectTransform>();
            statsTextRect.anchorMin = Vector2.zero;
            statsTextRect.anchorMax = Vector2.one;
            statsTextRect.offsetMin = new Vector2(20f, 50f);
            statsTextRect.offsetMax = new Vector2(-20f, -20f);

            TMP_Text statsText = statsTextObj.AddComponent<TextMeshProUGUI>();
            statsText.text = "Stats";
            statsText.color = Color.white;
            statsText.fontSize = 14;
            statsText.alignment = TextAlignmentOptions.Left;
            statsText.raycastTarget = false;

            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(statsPanelObj.transform, false);
            RectTransform closeButtonRect = closeButtonObj.AddComponent<RectTransform>();
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);
            closeButtonRect.sizeDelta = new Vector2(30f, 30f);
            closeButtonRect.anchoredPosition = new Vector2(-10f, -10f);

            Image closeButtonImage = closeButtonObj.AddComponent<Image>();
            closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            closeButtonImage.raycastTarget = true;

            Button closeButton = closeButtonObj.AddComponent<Button>();
            ColorBlock closeColors = closeButton.colors;
            closeColors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            closeColors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            closeColors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 0.7f);
            closeButton.colors = closeColors;

            GameObject closeTextObj = new GameObject("CloseText");
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
            closeText.raycastTarget = false;

            // Save prefabs
            string prefabPath = "Assets/Prefabs/UI/TribeInfoMenu/";
            System.IO.Directory.CreateDirectory(prefabPath);

            PrefabUtility.SaveAsPrefabAsset(menuButtonObj, prefabPath + "MenuButton.prefab");
            PrefabUtility.SaveAsPrefabAsset(menuPanelObj, prefabPath + "MenuPanel.prefab");
            PrefabUtility.SaveAsPrefabAsset(npcEntryObj, prefabPath + "NPCEntry.prefab");
            PrefabUtility.SaveAsPrefabAsset(statsPanelObj, prefabPath + "StatsPanel.prefab");

            // Clean up
            DestroyImmediate(menuButtonObj);
            DestroyImmediate(menuPanelObj);
            DestroyImmediate(npcEntryObj);
            DestroyImmediate(statsPanelObj);

            Debug.Log("Tribe Info Menu prefabs created successfully!");
        }

        [MenuItem("Tools/Regenerate Tribe Info Menu Prefabs")]
        public static void RegeneratePrefabs()
        {
            // Create NPC entry prefab
            GameObject npcEntryObj = TribeInfoMenuPrefabs.CreateNPCEntryPrefab();
            
            // Save prefab
            string prefabPath = "Assets/Prefabs/UI/TribeInfoMenu/";
            System.IO.Directory.CreateDirectory(prefabPath);
            
            PrefabUtility.SaveAsPrefabAsset(npcEntryObj, prefabPath + "NPCEntry.prefab");
            
            // Clean up
            DestroyImmediate(npcEntryObj);
            
            Debug.Log("Tribe Info Menu prefabs regenerated successfully!");
        }
    }
} 