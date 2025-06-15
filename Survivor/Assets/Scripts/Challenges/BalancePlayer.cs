using UnityEngine;

namespace Survivor.Challenges
{
    public class BalancePlayer : MonoBehaviour
    {
        [SerializeField] private float balanceForce = 1f;
        [SerializeField] private float maxBalanceForce = 2f;
        [SerializeField] private float balanceRecoveryRate = 0.5f;
        [SerializeField] private float currentBalanceForce;
        [SerializeField] private Animator animator;

        private void Update()
        {
            // Get input
            float input = 0f;
            if (Input.GetKey(KeyCode.A))
            {
                input = -1f;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                input = 1f;
            }

            // Update balance force
            if (input != 0f)
            {
                currentBalanceForce = Mathf.MoveTowards(currentBalanceForce, input * maxBalanceForce, balanceForce * Time.deltaTime);
            }
            else
            {
                currentBalanceForce = Mathf.MoveTowards(currentBalanceForce, 0f, balanceRecoveryRate * Time.deltaTime);
            }

            // Update animation
            if (animator != null)
            {
                animator.SetFloat("Balance", currentBalanceForce);
            }
        }

        public float GetBalanceForce()
        {
            return currentBalanceForce;
        }
    }
} 