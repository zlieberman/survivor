using UnityEngine;

namespace Survivor.Challenges
{
    public class BalanceBeam : MonoBehaviour
    {
        [SerializeField] private float maxTilt = 45f;
        [SerializeField] private float tiltSpeed = 30f;
        [SerializeField] private float currentTilt;
        [SerializeField] private Transform beamVisual;
        [SerializeField] private Transform playerPosition;

        private void Update()
        {
            // Update beam visual rotation
            if (beamVisual != null)
            {
                beamVisual.localRotation = Quaternion.Euler(0f, 0f, currentTilt);
            }

            // Update player position to stay on beam
            if (playerPosition != null)
            {
                float height = Mathf.Abs(currentTilt) * 0.01f; // Slight height adjustment based on tilt
                playerPosition.localPosition = new Vector3(0f, height, 0f);
                playerPosition.localRotation = Quaternion.Euler(0f, 0f, -currentTilt * 0.5f); // Player leans against tilt
            }
        }

        public void SetTilt(float tilt)
        {
            currentTilt = Mathf.Clamp(tilt, -maxTilt, maxTilt);
        }

        public float GetTilt()
        {
            return currentTilt;
        }

        public bool IsFallen()
        {
            return Mathf.Abs(currentTilt) >= maxTilt;
        }
    }
} 