using UnityEngine;

namespace StarterAssets
{
    /// <summary>
    /// Test script to verify swimming sound functionality
    /// Attach this to a GameObject to test swimming sounds manually
    /// Swimming sounds now play continuously while moving in water
    /// </summary>
    public class SwimmingSoundTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [Tooltip("Key to toggle swimming state")]
        public KeyCode toggleSwimmingKey = KeyCode.T;
        
        [Tooltip("Key to simulate movement while swimming")]
        public KeyCode simulateMovementKey = KeyCode.M;
        
        [Tooltip("Reference to ThirdPersonController to test")]
        public ThirdPersonController controller;
        
        private bool isSwimming = false;
        private bool isMoving = false;
        
        private void Start()
        {
            // Auto-find controller if not assigned
            if (controller == null)
            {
                controller = FindObjectOfType<ThirdPersonController>();
            }
            
            if (controller == null)
            {
                Debug.LogError("[SwimmingSoundTest] No ThirdPersonController found!");
            }
        }
        
        private void Update()
        {
            if (controller == null) return;
            
            // Toggle swimming state with test key
            if (Input.GetKeyDown(toggleSwimmingKey))
            {
                isSwimming = !isSwimming;
                controller.SetSwimming(isSwimming);
                
                Debug.Log($"[SwimmingSoundTest] Swimming state: {isSwimming}");
            }
            
            // Simulate movement while swimming
            if (Input.GetKeyDown(simulateMovementKey))
            {
                isMoving = !isMoving;
                Debug.Log($"[SwimmingSoundTest] Movement simulation: {isMoving}");
            }
        }
        
        [ContextMenu("Start Swimming")]
        public void StartSwimming()
        {
            if (controller == null)
            {
                Debug.LogError("[SwimmingSoundTest] No controller assigned!");
                return;
            }
            
            controller.SetSwimming(true);
            Debug.Log("[SwimmingSoundTest] Swimming started - sounds will play while moving");
        }
        
        [ContextMenu("Stop Swimming")]
        public void StopSwimming()
        {
            if (controller == null)
            {
                Debug.LogError("[SwimmingSoundTest] No controller assigned!");
                return;
            }
            
            controller.SetSwimming(false);
            Debug.Log("[SwimmingSoundTest] Swimming stopped");
        }
        
        private void OnGUI()
        {
            if (controller == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Swimming Sound Test");
            GUILayout.Label($"Swimming: {isSwimming}");
            GUILayout.Label($"Moving: {isMoving}");
            GUILayout.Label($"Press {toggleSwimmingKey} to toggle swimming");
            GUILayout.Label($"Press {simulateMovementKey} to simulate movement");
            GUILayout.Label("Swimming sounds play while moving in water");
            GUILayout.EndArea();
        }
    }
} 