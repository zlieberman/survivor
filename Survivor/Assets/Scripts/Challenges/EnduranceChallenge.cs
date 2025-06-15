using UnityEngine;

namespace Survivor.Challenges
{
    [CreateAssetMenu(fileName = "EnduranceChallenge", menuName = "Challenges/Endurance")]
    public class EnduranceChallenge : Challenge
    {
        [SerializeField] private float balanceThreshold = 0.5f;
        [SerializeField] private float balanceDecayRate = 0.1f;
        [SerializeField] private float balanceRecoveryRate = 0.2f;
        
        private float currentBalance;
        private bool isActive;

        public override void Initialize()
        {
            currentBalance = 1f;
            isActive = false;
        }

        public override void StartChallenge()
        {
            isActive = true;
            currentBalance = 1f;
        }

        public override void EndChallenge()
        {
            isActive = false;
        }

        public override bool IsCompleted()
        {
            return currentBalance <= 0f;
        }

        public override float GetProgress()
        {
            return 1f - currentBalance;
        }

        public void UpdateBalance(bool isMashing)
        {
            if (!isActive) return;

            if (isMashing)
            {
                currentBalance = Mathf.Min(1f, currentBalance + balanceRecoveryRate * Time.deltaTime);
            }
            else
            {
                currentBalance = Mathf.Max(0f, currentBalance - balanceDecayRate * Time.deltaTime);
            }

            if (currentBalance <= 0f)
            {
                EndChallenge();
            }
        }
    }
} 