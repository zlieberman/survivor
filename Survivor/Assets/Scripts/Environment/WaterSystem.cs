using UnityEngine;
using System.Collections.Generic;

namespace Survivor.Environment
{
    public class WaterSystem : MonoBehaviour
    {
        [Header("Water Properties")]
        public float waterHeight = 0f;
        public float buoyancyForce = 3f;
        public float dragForce = 1f;

        [Header("Fish System")]
        public GameObject fishPrefab;
        public int numberOfFish = 20;
        public float fishSpawnRadius = 50f;
        public float fishMinDepth = -5f;
        public float fishMaxDepth = -1f;
        public float fishMinSpeed = 2f;
        public float fishMaxSpeed = 4f;

        [Header("Water Settings")]
        public float height = 0f;
        public float waveHeight = 0.5f;
        public float waveSpeed = 1f;
        public float waveScale = 1f;

        private List<Fish> activeFish = new List<Fish>();

        private void Start()
        {
            SpawnFish();
            // Initialize water system
            transform.position = new Vector3(0, height, 0);
        }

        private void SpawnFish()
        {
            for (int i = 0; i < numberOfFish; i++)
            {
                Vector3 randomPosition = transform.position + new Vector3(
                    Random.Range(-fishSpawnRadius, fishSpawnRadius),
                    Random.Range(fishMinDepth, fishMaxDepth),
                    Random.Range(-fishSpawnRadius, fishSpawnRadius)
                );

                GameObject fishObject = Instantiate(fishPrefab, randomPosition, Random.rotation);
                Fish fish = fishObject.GetComponent<Fish>();
                if (fish != null)
                {
                    fish.Initialize(
                        Random.Range(fishMinSpeed, fishMaxSpeed),
                        fishSpawnRadius,
                        fishMinDepth,
                        fishMaxDepth
                    );
                    activeFish.Add(fish);
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            // Apply buoyancy to objects in water
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float submergedDepth = Mathf.Clamp01(
                    (waterHeight - other.transform.position.y) / other.bounds.size.y
                );

                // Apply buoyancy force
                Vector3 buoyancy = Vector3.up * buoyancyForce * submergedDepth;
                rb.AddForce(buoyancy, ForceMode.Acceleration);

                // Apply drag
                rb.drag = dragForce * submergedDepth;
                rb.angularDrag = dragForce * submergedDepth;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Reset drag when object exits water
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.drag = 0f;
                rb.angularDrag = 0.05f; // Default Unity value
            }
        }

        private void Update()
        {
            // Update water animation
            // This is a placeholder - you'll want to implement proper water animation
        }
    }

    public class Fish : MonoBehaviour
    {
        private float speed;
        private float boundaryRadius;
        private float minDepth;
        private float maxDepth;
        private Vector3 targetPosition;
        private float nextDirectionChange;

        public void Initialize(float speed, float boundaryRadius, float minDepth, float maxDepth)
        {
            this.speed = speed;
            this.boundaryRadius = boundaryRadius;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
            SetNewTarget();
        }

        private void Update()
        {
            if (Time.time >= nextDirectionChange)
            {
                SetNewTarget();
            }

            // Move towards target
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            // Look at movement direction
            if ((targetPosition - transform.position).sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(targetPosition - transform.position),
                    Time.deltaTime * 4f
                );
            }
        }

        private void SetNewTarget()
        {
            targetPosition = new Vector3(
                Random.Range(-boundaryRadius, boundaryRadius),
                Random.Range(minDepth, maxDepth),
                Random.Range(-boundaryRadius, boundaryRadius)
            );
            nextDirectionChange = Time.time + Random.Range(3f, 8f);
        }
    }
} 