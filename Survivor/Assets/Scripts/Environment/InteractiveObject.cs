using UnityEngine;
using UnityEngine.Events;

namespace Survivor.Environment
{
    public class InteractiveObject : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float pickupRange = 2f;
        public bool canBePickedUp = true;
        public bool canBeThrown = true;
        public float throwForce = 10f;

        [Header("Events")]
        public UnityEvent onPickup;
        public UnityEvent onDrop;
        public UnityEvent onThrow;

        private bool isPickedUp = false;
        private Transform holder;
        private Rigidbody rb;
        private Collider objectCollider;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            objectCollider = GetComponent<Collider>();
        }

        public void Pickup(Transform newHolder)
        {
            if (!canBePickedUp || isPickedUp) return;

            holder = newHolder;
            isPickedUp = true;

            // Disable physics while held
            rb.isKinematic = true;
            objectCollider.enabled = false;

            // Parent to holder
            transform.SetParent(holder);
            transform.localPosition = Vector3.forward;
            transform.localRotation = Quaternion.identity;

            onPickup?.Invoke();
        }

        public void Drop()
        {
            if (!isPickedUp) return;

            // Re-enable physics
            rb.isKinematic = false;
            objectCollider.enabled = true;

            // Unparent from holder
            transform.SetParent(null);
            isPickedUp = false;
            holder = null;

            onDrop?.Invoke();
        }

        public void Throw(Vector3 direction)
        {
            if (!canBeThrown || !isPickedUp) return;

            Drop();
            rb.AddForce(direction * throwForce, ForceMode.Impulse);
            onThrow?.Invoke();
        }

        public bool IsPickedUp()
        {
            return isPickedUp;
        }

        public bool IsInRange(Vector3 position)
        {
            return Vector3.Distance(transform.position, position) <= pickupRange;
        }
    }

    // Specific implementation for coconuts
    public class Coconut : InteractiveObject
    {
        [Header("Coconut Properties")]
        public float nutritionValue = 25f;
        public bool isCracked = false;

        public void Crack()
        {
            if (!isCracked)
            {
                isCracked = true;
                // You could change the mesh/material here to show a cracked version
                Debug.Log("Coconut cracked!");
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!isCracked && collision.relativeVelocity.magnitude > 5f)
            {
                Crack();
            }
        }
    }
} 