using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Survivor.Challenges
{
    public class BalanceBeamManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private BalanceBeam balanceBeam;
        [SerializeField] private GameObject playerArmature;
        [SerializeField] private Animator playerAnimator;
        [SerializeField] private WindEffect windEffect;

        [Header("Challenge Settings")]
        [SerializeField] private float challengeDuration = 60f;
        [SerializeField] private float windChangeInterval = 5f;
        [SerializeField] private float maxWindForce = 1f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float maxTilt = 45f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button quitButton;

        private float currentTime;
        private bool isChallengeActive;
        private float currentWindDirection;
        private float currentTilt;

        private void Start()
        {
            // Initialize UI
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            // Set up button listeners
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(RestartChallenge);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitChallenge);
            }

            // Position player on beam
            if (playerArmature != null && balanceBeam != null)
            {
                playerArmature.transform.position = balanceBeam.transform.position + Vector3.up * 1f;
                playerArmature.transform.rotation = Quaternion.identity;
            }

            // Start the challenge
            StartChallenge();
        }

        private void StartChallenge()
        {
            isChallengeActive = true;
            currentTime = challengeDuration;
            currentWindDirection = 0f;
            currentTilt = 0f;

            if (windEffect != null)
            {
                windEffect.SetWindDirection(currentWindDirection);
            }

            StartCoroutine(ChangeWindDirection());
        }

        private void Update()
        {
            if (!isChallengeActive) return;

            // Update timer
            currentTime -= Time.deltaTime;
            if (timerText != null)
            {
                timerText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
            }

            // Handle player input
            if (Input.GetKey(KeyCode.A))
            {
                currentTilt -= rotationSpeed * Time.deltaTime;
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Balance", -1f);
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                currentTilt += rotationSpeed * Time.deltaTime;
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Balance", 1f);
                }
            }
            else
            {
                if (playerAnimator != null)
                {
                    playerAnimator.SetFloat("Balance", 0f);
                }
            }

            // Apply wind force
            currentTilt += currentWindDirection * maxWindForce * Time.deltaTime;

            // Clamp tilt
            currentTilt = Mathf.Clamp(currentTilt, -maxTilt, maxTilt);

            // Update beam rotation
            if (balanceBeam != null)
            {
                balanceBeam.SetTilt(currentTilt);
            }

            // Update player position and rotation
            if (playerArmature != null && balanceBeam != null)
            {
                Vector3 beamPosition = balanceBeam.transform.position;
                playerArmature.transform.position = new Vector3(beamPosition.x, beamPosition.y + 1f, beamPosition.z);
                playerArmature.transform.rotation = Quaternion.Euler(0, 0, currentTilt);
            }

            // Update balance UI
            if (balanceText != null)
            {
                float balancePercent = 100f * (1f - Mathf.Abs(currentTilt) / maxTilt);
                balanceText.text = $"Balance: {balancePercent:F1}%";
            }

            // Check for game over conditions
            if (currentTime <= 0f || Mathf.Abs(currentTilt) >= maxTilt)
            {
                EndChallenge();
            }
        }

        private IEnumerator ChangeWindDirection()
        {
            while (isChallengeActive)
            {
                yield return new WaitForSeconds(windChangeInterval);
                
                // Randomly change wind direction
                currentWindDirection = Random.Range(-maxWindForce, maxWindForce);
                
                if (windEffect != null)
                {
                    windEffect.SetWindDirection(currentWindDirection);
                }
            }
        }

        private void EndChallenge()
        {
            isChallengeActive = false;
            StopAllCoroutines();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            if (resultText != null)
            {
                if (currentTime <= 0f)
                {
                    resultText.text = "Challenge Complete!";
                }
                else
                {
                    resultText.text = "You Fell!";
                }
            }

            // Play fall animation
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Fall");
            }
        }

        private void RestartChallenge()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            if (balanceBeam != null)
            {
                balanceBeam.SetTilt(0f);
            }

            StartChallenge();
        }

        private void QuitChallenge()
        {
            // Load the challenges menu scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChallengesMenu");
        }
    }
} 