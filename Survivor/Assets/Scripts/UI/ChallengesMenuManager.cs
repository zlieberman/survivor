using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Survivor.Challenges;
using TMPro;
using UnityEngine.SceneManagement;

namespace Survivor.UI
{
    public class ChallengesMenuManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button backButton;
        [SerializeField] private Transform challengeListContainer;
        [SerializeField] private GameObject challengeButtonPrefab;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Challenge Data")]
        [SerializeField] private List<Challenge> availableChallenges;
        
        private void Start()
        {
            InitializeUI();
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void InitializeUI()
        {
            titleText.text = "Challenges";
            
            // Clear existing buttons
            foreach (Transform child in challengeListContainer)
            {
                Destroy(child.gameObject);
            }

            // Create buttons for each challenge
            foreach (var challenge in availableChallenges)
            {
                GameObject buttonObj = Instantiate(challengeButtonPrefab, challengeListContainer);
                ChallengeButton challengeButton = buttonObj.GetComponent<ChallengeButton>();
                
                if (challengeButton != null)
                {
                    challengeButton.Initialize(challenge);
                    challengeButton.SetOnClickCallback(() => OnChallengeSelected(challenge));
                }
            }
        }

        private void OnChallengeSelected(Challenge challenge)
        {
            // Load the challenge scene
            SceneManager.LoadScene(challenge.sceneName);
        }

        private void OnBackButtonClicked()
        {
            // Return to main menu
            SceneManager.LoadScene("MainMenu");
        }
    }
} 