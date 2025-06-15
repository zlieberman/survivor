using UnityEngine;
using Survivor.Shared;

namespace Survivor.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerSwimming : MonoBehaviour, IWaterInteractable
    {
        [Header("Swimming Settings")]
        public float swimSpeed = 5f;
        public float swimAcceleration = 10f;
        public float waterDrag = 1f;
        public float waterAngularDrag = 0.5f;

        private Rigidbody rb;
        private bool isInWater = false;
        private float waterHeight = 0f;
        private float originalDrag;
        private float originalAngularDrag;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            originalDrag = rb.drag;
            originalAngularDrag = rb.angularDrag;
        }

        public void OnEnterWater(float waterHeight)
        {
            isInWater = true;
            this.waterHeight = waterHeight;
            rb.drag = waterDrag;
            rb.angularDrag = waterAngularDrag;
        }

        public void OnExitWater()
        {
            isInWater = false;
            rb.drag = originalDrag;
            rb.angularDrag = originalAngularDrag;
        }

        public void OnStayInWater(float waterHeight, float buoyancyForce, float dragForce)
        {
            // Apply buoyancy force
            float depth = waterHeight - transform.position.y;
            if (depth > 0)
            {
                Vector3 buoyancy = Vector3.up * buoyancyForce * depth;
                rb.AddForce(buoyancy, ForceMode.Acceleration);
            }

            // Apply drag
            rb.AddForce(-rb.velocity * dragForce, ForceMode.Acceleration);
        }

        private void Update()
        {
            if (isInWater)
            {
                // Handle swimming input
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");

                Vector3 swimDirection = new Vector3(horizontal, 0, vertical).normalized;
                if (swimDirection.magnitude > 0.1f)
                {
                    // Apply swimming force
                    rb.AddForce(swimDirection * swimSpeed * swimAcceleration, ForceMode.Acceleration);
                }
            }
        }
    }
} 