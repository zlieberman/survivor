using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using Survivor.Shared;

namespace Survivor.Generation
{
    public class IslandManager : MonoBehaviour
    {
        private IIslandGenerator islandGenerator;
        private CampGenerator campGenerator;
        private bool isGenerating = false;

        private void Awake()
        {
            // Create and initialize island generator
            GameObject islandGeneratorObj = new GameObject("ProceduralIslandGenerator");
            islandGeneratorObj.transform.parent = transform;
            islandGenerator = islandGeneratorObj.AddComponent<ProceduralIslandGenerator>();

            // Create and initialize camp generator
            GameObject campGeneratorObj = new GameObject("CampGenerator");
            campGeneratorObj.transform.parent = transform;
            campGenerator = campGeneratorObj.AddComponent<CampGenerator>();
        }

        public IEnumerator GenerateIsland()
        {
            if (isGenerating)
            {
                Debug.LogWarning("Island generation already in progress!");
                yield break;
            }

            isGenerating = true;
            Debug.Log("Starting island generation sequence");

            // Step 1: Generate terrain
            yield return StartCoroutine(GenerateTerrain());
            Debug.Log("Terrain generation complete");

            // Step 2: Place camp
            yield return StartCoroutine(PlaceCamp());
            Debug.Log("Camp placement complete");

            // Step 3: Build NavMesh
            yield return StartCoroutine(BuildNavMesh());
            Debug.Log("NavMesh build complete");

            isGenerating = false;
            Debug.Log("Island generation sequence complete");
        }

        private IEnumerator GenerateTerrain()
        {
            if (islandGenerator == null)
            {
                Debug.LogError("IslandGenerator not found!");
                yield break;
            }

            islandGenerator.GenerateIsland();
            while (!islandGenerator.IsGenerationComplete())
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator PlaceCamp()
        {
            if (campGenerator == null)
            {
                Debug.LogError("CampGenerator not found!");
                yield break;
            }

            campGenerator.PlaceCamp();
            while (!campGenerator.CampPlaced)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }

        private IEnumerator BuildNavMesh()
        {
            // Wait for NavMesh to be ready
            float timeout = 10f;
            float elapsed = 0f;
            bool navMeshReady = false;

            while (elapsed < timeout && !navMeshReady)
            {
                if (campGenerator.transform != null)
                {
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(campGenerator.transform.position, out hit, 1.0f, NavMesh.AllAreas))
                    {
                        navMeshReady = true;
                        Debug.Log("NavMesh is ready at camp position");
                        break;
                    }
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (!navMeshReady)
            {
                Debug.LogError("NavMesh is not ready at camp position!");
            }
        }
    }
} 