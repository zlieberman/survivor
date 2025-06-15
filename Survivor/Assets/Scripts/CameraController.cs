using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform target;
    public float smoothSpeed = 10f;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float lookAheadFactor = 0.5f;

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = target.position + offset;
        
        // Smoothly move camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothSpeed * Time.deltaTime);

        // Look at target
        transform.LookAt(target.position + Vector3.up * lookAheadFactor);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            // Reset camera position and rotation
            transform.position = target.position + offset;
            transform.LookAt(target.position + Vector3.up * lookAheadFactor);
        }
    }
} 