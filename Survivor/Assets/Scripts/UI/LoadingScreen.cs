using UnityEngine;
using UnityEngine.UI;

namespace Survivor.UI
{
    /// <summary>
    /// Simple loading screen that can be used with NavMeshLoadingManager
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        [Header("UI References")]
        public Text loadingText;
        public Image progressBar;
        public GameObject loadingIcon;

        [Header("Animation Settings")]
        public float iconRotationSpeed = 90f; // Degrees per second
        public bool animateIcon = true;

        private void Start()
        {
            // Ensure this is active when the scene starts
            gameObject.SetActive(true);
            
            // Set up initial text
            if (loadingText != null)
            {
                loadingText.text = "Loading...";
            }
        }

        private void Update()
        {
            // Animate loading icon
            if (animateIcon && loadingIcon != null)
            {
                loadingIcon.transform.Rotate(0, 0, iconRotationSpeed * Time.deltaTime);
            }
        }

        public void UpdateLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }

        public void UpdateProgress(float progress)
        {
            if (progressBar != null)
            {
                progressBar.fillAmount = Mathf.Clamp01(progress);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
} 