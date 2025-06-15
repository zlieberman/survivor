using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Challenges;

namespace Survivor.UI
{
    public class ChallengeButton : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private Button button;

        [Header("Layout Settings")]
        [SerializeField] private float thumbnailSize = 100f;
        [SerializeField] private float padding = 10f;
        [SerializeField] private float titleHeight = 30f;
        [SerializeField] private float descriptionHeight = 60f;
        [SerializeField] private float difficultyHeight = 20f;

        private void Awake()
        {
            SetupLayout();
        }

        private void SetupLayout()
        {
            // Set up the button's RectTransform
            RectTransform buttonRect = GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(400f, 150f);

            // Set up thumbnail
            if (thumbnailImage != null)
            {
                RectTransform thumbnailRect = thumbnailImage.GetComponent<RectTransform>();
                thumbnailRect.anchorMin = new Vector2(0, 0.5f);
                thumbnailRect.anchorMax = new Vector2(0, 0.5f);
                thumbnailRect.pivot = new Vector2(0, 0.5f);
                thumbnailRect.sizeDelta = new Vector2(thumbnailSize, thumbnailSize);
                thumbnailRect.anchoredPosition = new Vector2(padding, 0);
            }

            // Set up content container
            Transform contentTransform = transform.Find("Content");
            if (contentTransform != null)
            {
                RectTransform contentRect = contentTransform.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.offsetMin = new Vector2(thumbnailSize + padding * 2, padding);
                contentRect.offsetMax = new Vector2(-padding, -padding);

                // Set up title
                if (titleText != null)
                {
                    RectTransform titleRect = titleText.GetComponent<RectTransform>();
                    titleRect.anchorMin = new Vector2(0, 1);
                    titleRect.anchorMax = new Vector2(1, 1);
                    titleRect.pivot = new Vector2(0.5f, 1);
                    titleRect.sizeDelta = new Vector2(0, titleHeight);
                    titleRect.anchoredPosition = new Vector2(0, 0);
                    titleText.fontSize = 24;
                    titleText.fontStyle = FontStyles.Bold;
                    titleText.alignment = TextAlignmentOptions.Left;
                }

                // Set up description
                if (descriptionText != null)
                {
                    RectTransform descRect = descriptionText.GetComponent<RectTransform>();
                    descRect.anchorMin = new Vector2(0, 1);
                    descRect.anchorMax = new Vector2(1, 1);
                    descRect.pivot = new Vector2(0.5f, 1);
                    descRect.sizeDelta = new Vector2(0, descriptionHeight);
                    descRect.anchoredPosition = new Vector2(0, -titleHeight);
                    descriptionText.fontSize = 16;
                    descriptionText.fontStyle = FontStyles.Normal;
                    descriptionText.alignment = TextAlignmentOptions.Left;
                    descriptionText.enableWordWrapping = true;
                }

                // Set up difficulty
                if (difficultyText != null)
                {
                    RectTransform diffRect = difficultyText.GetComponent<RectTransform>();
                    diffRect.anchorMin = new Vector2(0, 0);
                    diffRect.anchorMax = new Vector2(1, 0);
                    diffRect.pivot = new Vector2(0, 0);
                    diffRect.sizeDelta = new Vector2(0, difficultyHeight);
                    diffRect.anchoredPosition = new Vector2(0, padding);
                    difficultyText.fontSize = 14;
                    difficultyText.fontStyle = FontStyles.Italic;
                    difficultyText.alignment = TextAlignmentOptions.Left;
                }
            }
        }

        public void Initialize(Challenge challenge)
        {
            if (titleText != null)
                titleText.text = challenge.title;
            
            if (descriptionText != null)
                descriptionText.text = challenge.description;
            
            if (difficultyText != null)
                difficultyText.text = $"Difficulty: {challenge.difficulty}";
            
            if (thumbnailImage != null && challenge.thumbnail != null)
                thumbnailImage.sprite = challenge.thumbnail;
        }

        public void SetOnClickCallback(UnityEngine.Events.UnityAction callback)
        {
            if (button != null)
                button.onClick.AddListener(callback);
        }
    }
} 