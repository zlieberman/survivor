using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Survivor.Tribes
{
    public class NameTag : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private float heightOffset = 2.2f;
        [SerializeField] private float fadeDistance = 20f;
        [SerializeField] private float minAlpha = 0.3f;
        [SerializeField] private float fontSize = 0.3f;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.5f);

        private Camera mainCamera;
        private Canvas canvas;
        private RectTransform canvasRect;
        private Image backgroundImage;

        private void Awake()
        {
            // Create canvas if it doesn't exist
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 100; // Ensure it renders above other elements
            }

            // Add canvas scaler
            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.dynamicPixelsPerUnit = 100f;
            }

            // Create text if it doesn't exist
            if (nameText == null)
            {
                GameObject textObj = new GameObject("NameText");
                textObj.transform.SetParent(transform, false);
                nameText = textObj.AddComponent<TextMeshProUGUI>();
                nameText.alignment = TextAlignmentOptions.Center;
                nameText.fontSize = fontSize;
                nameText.color = textColor;
                nameText.font = TMP_Settings.defaultFontAsset;
                nameText.enableWordWrapping = false;
                nameText.overflowMode = TextOverflowModes.Overflow;
                
                // Add outline to make text more visible
                nameText.outlineWidth = 0.2f;
                nameText.outlineColor = new Color(0, 0, 0, 0.5f);

                // Set up text rect transform
                RectTransform textRect = nameText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
            }

            // Create background image
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(transform, false);
            backgroundImage = bgObj.AddComponent<Image>();
            backgroundImage.color = backgroundColor;
            RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.anchoredPosition = Vector2.zero;

            // Set up canvas rect
            canvasRect = canvas.GetComponent<RectTransform>();
            
            // Add ContentSizeFitter to automatically size based on content
            ContentSizeFitter sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // Set initial size to be very small, ContentSizeFitter will expand it
            canvasRect.sizeDelta = new Vector2(0.01f, 0.01f);
            canvasRect.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            // Ensure the canvas is properly positioned
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main camera not found!");
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            // Update position
            Vector3 targetPosition = transform.parent.position + Vector3.up * heightOffset;
            transform.position = targetPosition;

            // Make name tag face camera
            transform.rotation = mainCamera.transform.rotation;

            // Calculate distance-based alpha
            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            float alpha = Mathf.Lerp(1f, minAlpha, distance / fadeDistance);
            
            // Update text alpha
            Color textColor = nameText.color;
            textColor.a = alpha;
            nameText.color = textColor;

            // Update background alpha
            Color bgColor = backgroundImage.color;
            bgColor.a = backgroundColor.a * alpha;
            backgroundImage.color = bgColor;
        }

        public void SetName(string name)
        {
            if (nameText != null)
            {
                nameText.text = name;
                Debug.Log($"Setting name tag text to: {name}"); // Debug log
            }
            else
            {
                Debug.LogError("NameText component is null!"); // Debug log
            }
        }
    }
} 