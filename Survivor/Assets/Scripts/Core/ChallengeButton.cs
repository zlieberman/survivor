using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Challenges;

namespace Survivor.Core
{
    public class ChallengeButton : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI challengeNameText;
        public TextMeshProUGUI challengeDescriptionText;
        public TextMeshProUGUI challengeTypeText;
        public Button button;

        private Survivor.Challenges.Challenge challenge;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
                button.onClick.AddListener(OnButtonClick);
        }

        public void SetChallenge(Survivor.Challenges.Challenge challenge)
        {
            this.challenge = challenge;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (challenge == null) return;

            if (challengeNameText != null)
                challengeNameText.text = challenge.data.title;

            if (challengeDescriptionText != null)
                challengeDescriptionText.text = challenge.data.description;

            if (challengeTypeText != null)
                challengeTypeText.text = challenge.data.type.ToString();
        }

        private void OnButtonClick()
        {
            if (challenge != null && ChallengeSystem.Instance != null)
            {
                ChallengeSystem.Instance.StartChallenge(challenge);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClick);
        }
    }
} 