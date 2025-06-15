using UnityEngine;

namespace Survivor.Generation
{
    public class RiverGenerator : MonoBehaviour
    {
        private ProceduralIslandGenerator islandGenerator;

        private void Awake()
        {
            islandGenerator = GetComponent<ProceduralIslandGenerator>();
        }

        public void GenerateRiver()
        {
            if (islandGenerator == null)
            {
                Debug.LogError("RiverGenerator: IslandGenerator reference is missing!");
                return;
            }

            // River generation logic will be implemented here
            Debug.Log("River generation completed");
        }
    }
} 