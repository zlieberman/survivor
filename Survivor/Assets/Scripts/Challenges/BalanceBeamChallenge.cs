using UnityEngine;
using Survivor.Challenges;
using TMPro;

namespace Survivor.Challenges
{
    public class BalanceBeamChallenge : MonoBehaviour
    {
        [Header("Balance Settings")]
        [SerializeField] private float maxTilt = 45f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float windForce = 5f;
        [SerializeField] private float fallThreshold = 60f;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI balanceText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;

        [Header("References")]
        [SerializeField] private GameObject playerArmature;
        [SerializeField] private Transform balanceBeam;
        [SerializeField] private Animator playerAnimator;

        private float currentTilt;
        private float windDirection;
        private float windTimer;
        private float gameTimer;
        private bool isGameOver;
        private EnduranceChallenge challenge;

        private void Start()
        {
            // Get the challenge from the ChallengeManager
            challenge = ChallengeManager.Instance.CurrentChallenge as EnduranceChallenge;
            if (challenge == null)
            {
                Debug.LogError("No EnduranceChallenge found in ChallengeManager!");
                return;
            }

            // Initialize the challenge
            challenge.Initialize();
            challenge.StartChallenge();

            // Hide game over panel
            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            // Initialize wind
            ChangeWindDirection();

            // Set up player armature
            if (playerArmature != null)
            {
                // Position the player on the beam
                playerArmature.transform.position = balanceBeam.position + Vector3.up * 1f;
                playerArmature.transform.rotation = Quaternion.identity;
            }
        }

        private void Update()
        {
            if (isGameOver) return;

            // Update wind direction
            windTimer -= Time.deltaTime;
            if (windTimer <= 0)
            {
                ChangeWindDirection();
            }

            // Apply wind force
            currentTilt += windDirection * windForce * Time.deltaTime;

            // Handle player input for direct rotation
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

            // Clamp tilt
            currentTilt = Mathf.Clamp(currentTilt, -maxTilt, maxTilt);

            // Update beam rotation
            if (balanceBeam != null)
            {
                balanceBeam.rotation = Quaternion.Euler(0, 0, currentTilt);
            }

            // Update player position and rotation
            if (playerArmature != null)
            {
                // Keep player on the beam
                Vector3 beamPosition = balanceBeam.position;
                playerArmature.transform.position = new Vector3(beamPosition.x, beamPosition.y + 1f, beamPosition.z);
                
                // Rotate player to match beam tilt
                playerArmature.transform.rotation = Quaternion.Euler(0, 0, currentTilt);
            }

            // Check for fall
            if (Mathf.Abs(currentTilt) > fallThreshold)
            {
                GameOver();
            }

            // Update UI
            UpdateUI();

            // Update challenge
            challenge.UpdateBalance(Mathf.Abs(currentTilt) < 30f);
        }

        private void ChangeWindDirection()
        {
            windDirection = Random.Range(-1f, 1f);
            windTimer = Random.Range(1f, 3f);
        }

        private void UpdateUI()
        {
            if (balanceText != null)
            {
                float balancePercentage = 100f - (Mathf.Abs(currentTilt) / maxTilt * 100f);
                balanceText.text = $"Balance: {balancePercentage:F0}%";
            }

            if (timerText != null)
            {
                gameTimer += Time.deltaTime;
                timerText.text = $"Time: {gameTimer:F1}s";
            }
        }

        private void GameOver()
        {
            isGameOver = true;
            challenge.EndChallenge();

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
                if (finalScoreText != null)
                {
                    finalScoreText.text = $"You survived for {gameTimer:F1} seconds!";
                }
            }

            // Play fall animation
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("Fall");
            }
        }

        public void OnRestartClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        public void OnMainMenuClicked()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }
} 