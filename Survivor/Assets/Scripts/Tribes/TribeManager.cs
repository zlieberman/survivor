using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Survivor.Generation;

namespace Survivor.Tribes
{
    public class TribeManager : MonoBehaviour
    {
        private bool isInitialized = false;
        private GameObject playerPrefab;
        private GameObject npcPrefab;

        public void Initialize(string playerTribeName, string opposingTribeName, Color playerTribeColor, Color opposingTribeColor)
        {
            isInitialized = true;
        }

        public IEnumerator InitializeTribes(GameObject playerPrefab, GameObject npcPrefab)
        {
            if (!isInitialized)
            {
                Debug.LogError("TribeManager not initialized!");
                yield break;
            }

            this.playerPrefab = playerPrefab;
            this.npcPrefab = npcPrefab;
            Debug.Log("Tribe initialization complete");
        }

        public IEnumerator SpawnTribeMembers()
        {
            if (!isInitialized)
            {
                Debug.LogError("TribeManager not initialized!");
                yield break;
            }

            // Find camp spawn point
            CampGenerator campGenerator = FindObjectOfType<CampGenerator>();
            if (campGenerator == null || !campGenerator.CampPlaced)
            {
                Debug.LogError("Camp not placed yet!");
                yield break;
            }

            Transform spawnPoint = campGenerator.CampSpawnPoint;
            if (spawnPoint == null)
            {
                Debug.LogError("Camp spawn point not found!");
                yield break;
            }

            // Spawn main player
            yield return StartCoroutine(SpawnPlayer(spawnPoint));
        }

        private IEnumerator SpawnPlayer(Transform spawnPoint)
        {
            if (spawnPoint == null) yield break;

            Vector3 spawnPosition = spawnPoint.position;
            Terrain terrain = FindObjectOfType<Terrain>();
            
            if (terrain != null)
            {
                float terrainHeight = terrain.SampleHeight(spawnPosition);
                spawnPosition.y = terrainHeight + 1f;
            }

            GameObject playerObject = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            if (playerObject != null)
            {
                SetupPlayerComponents(playerObject);
                SetupPlayerCamera(playerObject);
            }

            yield return null;
        }

        private void SetupPlayerComponents(GameObject playerObject)
        {
            CharacterController controller = playerObject.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = playerObject.AddComponent<CharacterController>();
                controller.height = 2f;
                controller.radius = 0.5f;
                controller.stepOffset = 0.3f;
            }

            ThirdPersonController thirdPersonController = playerObject.GetComponent<ThirdPersonController>();
            if (thirdPersonController == null)
            {
                thirdPersonController = playerObject.AddComponent<ThirdPersonController>();
            }
        }

        private void SetupPlayerCamera(GameObject playerObject)
        {
            GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCamera != null)
            {
                // Create a simple camera follow script if it doesn't exist
                CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
                if (cameraFollow == null)
                {
                    cameraFollow = mainCamera.AddComponent<CameraFollow>();
                }

                cameraFollow.target = playerObject.transform;
                cameraFollow.offset = new Vector3(0, 2, -5);
                cameraFollow.smoothSpeed = 0.125f;

                mainCamera.transform.position = playerObject.transform.position + cameraFollow.offset;
                mainCamera.transform.LookAt(playerObject.transform.position + Vector3.up * 1.5f);
            }
        }
    }
} 