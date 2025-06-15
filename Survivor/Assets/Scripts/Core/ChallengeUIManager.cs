using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Survivor.Challenges;
using TMPro;

namespace Survivor.Core
{
    public class ChallengeUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Transform challengeListContainer;
        [SerializeField] private GameObject challengeButtonPrefab;
        
        [Header("Challenge Data")]
        [SerializeField] private List<Challenge> availableChallenges;
        
        private void Start()
        {
            InitializeUI();
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void InitializeUI()
        {
            // Clear existing buttons
            foreach (Transform child in challengeListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for each challenge
            foreach (var challenge in availableChallenges)
            {
                GameObject buttonObj = Instantiate(challengeButtonPrefab, challengeListContainer);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI titleText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                Image thumbnailImage = buttonObj.GetComponentInChildren<Image>();

                if (titleText != null)
                {
                    titleText.text = challenge.data.title;
                }

                if (thumbnailImage != null && challenge.data.thumbnail != null)
                {
                    thumbnailImage.sprite = challenge.data.thumbnail;
                }

                button.onClick.AddListener(() => OnChallengeSelected(challenge));
            }
        }

        private void OnChallengeSelected(Challenge challenge)
        {
            // Load the challenge scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(challenge.data.sceneName);
        }

        private void OnBackButtonClicked()
        {
            // Return to main menu
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
} 