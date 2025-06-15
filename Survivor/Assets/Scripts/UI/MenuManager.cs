using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace Survivor.UI
{
    public class MenuManager : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button playButton;
        public Button challengesButton;
        public Button quitButton;
        
        [Header("Optional Settings")]
        public TMP_InputField seedInput; // Optional: for custom seeds
        public Toggle useCustomSeedToggle; // Optional: to enable/disable custom seed

        [Header("Scene Settings")]
        public string gameSceneName = "Game"; // Name of your game scene
        public string challengesSceneName = "ChallengesMenu"; // Name of your challenges scene

        private void Start()
        {
            // Setup button listeners
            if (playButton != null)
                playButton.onClick.AddListener(StartGame);
            
            if (challengesButton != null)
                challengesButton.onClick.AddListener(OpenChallenges);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);

            if (useCustomSeedToggle != null)
                useCustomSeedToggle.onValueChanged.AddListener(OnCustomSeedToggleChanged);

            // Initialize seed input
            if (seedInput != null)
            {
                seedInput.interactable = false; // Disabled by default
                seedInput.text = Random.Range(0, 99999).ToString();
            }
        }

        private void OnCustomSeedToggleChanged(bool isOn)
        {
            if (seedInput != null)
                seedInput.interactable = isOn;
        }

        public void StartGame()
        {
            // Save seed preference if using custom seed
            if (useCustomSeedToggle != null && useCustomSeedToggle.isOn && seedInput != null)
            {
                if (int.TryParse(seedInput.text, out int seed))
                {
                    PlayerPrefs.SetInt("CustomSeed", seed);
                    PlayerPrefs.SetInt("UseCustomSeed", 1);
                }
            }
            else
            {
                PlayerPrefs.SetInt("UseCustomSeed", 0);
            }
            
            // Load the game scene
            SceneManager.LoadScene(gameSceneName);
        }

        public void OpenChallenges()
        {
            // Load the challenges menu scene
            SceneManager.LoadScene(challengesSceneName);
        }

        private void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (playButton != null)
                playButton.onClick.RemoveListener(StartGame);
            
            if (challengesButton != null)
                challengesButton.onClick.RemoveListener(OpenChallenges);
            
            if (quitButton != null)
                quitButton.onClick.RemoveListener(QuitGame);

            if (useCustomSeedToggle != null)
                useCustomSeedToggle.onValueChanged.RemoveListener(OnCustomSeedToggleChanged);
        }
    }
} 