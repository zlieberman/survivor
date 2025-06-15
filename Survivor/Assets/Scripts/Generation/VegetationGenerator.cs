using UnityEngine;

namespace Survivor.Generation
{
    public class VegetationGenerator : MonoBehaviour
    {
        private ProceduralIslandGenerator islandGenerator;

        private void Awake()
        {
            islandGenerator = GetComponent<ProceduralIslandGenerator>();
        }

        public void GenerateVegetation()
        {
            if (islandGenerator == null)
            {
                Debug.LogError("VegetationGenerator: IslandGenerator reference is missing!");
                return;
            }

            // Vegetation generation logic will be implemented here
            Debug.Log("Vegetation generation completed");
        }
    }
} 