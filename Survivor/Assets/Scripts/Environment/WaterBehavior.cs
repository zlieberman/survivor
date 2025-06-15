using UnityEngine;
using Survivor.Player;

namespace Survivor.Environment
{
    public class WaterBehavior : MonoBehaviour
    {
        public float waterHeight = 0f;
        public float buoyancyForce = 2f;
        public float dragForce = 0.5f;
        public float swimForce = 5f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Notify player they're in water
                PlayerSwimming playerSwimming = other.GetComponent<PlayerSwimming>();
                if (playerSwimming != null)
                {
                    playerSwimming.EnterWater(waterHeight);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // Notify player they're out of water
                PlayerSwimming playerSwimming = other.GetComponent<PlayerSwimming>();
                if (playerSwimming != null)
                {
                    playerSwimming.ExitWater();
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Rigidbody rb = other.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Apply buoyancy force
                    float depth = waterHeight - other.transform.position.y;
                    if (depth > 0)
                    {
                        Vector3 buoyancy = Vector3.up * buoyancyForce * depth;
                        rb.AddForce(buoyancy, ForceMode.Acceleration);
                    }

                    // Apply drag
                    rb.AddForce(-rb.velocity * dragForce, ForceMode.Acceleration);
                }
            }
        }
    }
} 