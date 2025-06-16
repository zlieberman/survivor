using UnityEngine;

namespace Survivor.Environment
{
    public class WaterBehavior : MonoBehaviour
    {
        public float waterHeight = 0f;

        private void OnTriggerEnter(Collider other)
        {
            // Handle water interaction
            if (other.CompareTag("Player"))
            {
                // Add water interaction logic here
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Handle water exit
            if (other.CompareTag("Player"))
            {
                // Add water exit logic here
            }
        }
    }
} 