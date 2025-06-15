using UnityEngine;

namespace Survivor
{
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 2, -5);
        public float smoothSpeed = 0.125f;

        private void LateUpdate()
        {
            if (target == null) return;

            // Calculate desired position
            Vector3 desiredPosition = target.position + offset;
            
            // Smoothly move towards that position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Look at the target
            transform.LookAt(target.position + Vector3.up * 1.5f);
        }
    }
} 