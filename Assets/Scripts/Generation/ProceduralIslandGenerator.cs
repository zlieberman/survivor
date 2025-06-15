using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Survivor.Generation
{
    // This script handles the procedural generation of the island and its features
    [DefaultExecutionOrder(-100)] // This makes the script run before default time
    [RequireComponent(typeof(Terrain))]
    [RequireComponent(typeof(TerrainCollider))]
    public class ProceduralIslandGenerator : MonoBehaviour
    {
        private System.Collections.IEnumerator PlaceCampWithTimeout(CampGenerator campGenerator)
        {
            if (campGenerator == null)
            {
                Debug.LogError("CampGenerator is null");
                yield break;
            }

            float timeout = 5f; // 5 seconds timeout
            float elapsed = 0f;
            
            bool startedSuccessfully = false;
            try
            {
                campGenerator.SendMessage("PlaceCamp");
                startedSuccessfully = true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error starting camp placement: {e.Message}");
                yield break;
            }
            
            if (startedSuccessfully)
            {
                // Wait for camp to be placed or timeout
                while (!campGenerator.CampPlaced && elapsed < timeout)
                {
                    elapsed += Time.deltaTime;
                    yield return null;
                }
                
                if (!campGenerator.CampPlaced)
                {
                    Debug.LogError($"Camp placement timed out after {timeout} seconds");
                }
            }
        }

        private void PlaceCampDelayed()
        {
            // Implementation of PlaceCampDelayed method
        }

        private System.Collections.IEnumerator GenerateIslandFeatures()
        {
            yield return new WaitForEndOfFrame();
            
            CampGenerator campGenerator = null;
            bool generatorFound = false;
            bool shouldContinue = true;
            
            try
            {
                campGenerator = GetComponent<CampGenerator>();
                generatorFound = (campGenerator != null);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error getting CampGenerator: {e.Message}\n{e.StackTrace}");
                shouldContinue = false;
            }

            if (!shouldContinue)
            {
                yield break;
            }

            if (generatorFound)
            {
                Debug.Log("Starting camp placement...");
                yield return StartCoroutine(PlaceCampWithTimeout(campGenerator));
                
                if (campGenerator.CampPlaced)
                {
                    try
                    {
                        Debug.Log("Camp placed successfully, generating river and vegetation");
                        CreateRiver();
                        PlaceVegetation();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Error generating river and vegetation: {e.Message}\n{e.StackTrace}");
                    }
                }
                else
                {
                    Debug.LogError("Failed to place camp after timeout, skipping river and vegetation generation");
                }
            }
            else
            {
                Debug.LogWarning("No CampGenerator found - proceeding with river and vegetation");
                try
                {
                    CreateRiver();
                    PlaceVegetation();
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating river and vegetation: {e.Message}\n{e.StackTrace}");
                }
            }
        }
    }
}