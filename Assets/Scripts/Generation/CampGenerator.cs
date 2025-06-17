using UnityEngine;

public class CampGenerator : MonoBehaviour
{
    private Vector3 tentPosition;

    private void Start()
    {
        tentPosition = transform.position;
    }

    private void SpawnPlayerAtCamp()
    {
        if (terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return;
        }

        // Find the player with retries
        GameObject player = null;
        int maxRetries = 10;
        int currentRetry = 0;
        
        while (player == null && currentRetry < maxRetries)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.Log($"Waiting for player to be spawned (attempt {currentRetry + 1}/{maxRetries})...");
                currentRetry++;
                System.Threading.Thread.Sleep(100); // Small delay between retries
            }
        }

        if (player == null)
        {
            Debug.LogError("No player found with 'Player' tag after multiple attempts!");
            return;
        }

        Debug.Log($"Found player at position: {player.transform.position}");

        // Calculate spawn position slightly offset from the tent
        Vector3 spawnOffset = new Vector3(-1f, 0, 5f); // by the campfire
        Vector3 spawnPosition = tentPosition + spawnOffset;
        
        // Ensure correct height using terrain SampleHeight
        float y = GetTerrainHeight(spawnPosition);
        spawnPosition.y = y + 1f; // Add 1 unit up to prevent ground clipping

        // Move player to spawn position
        player.transform.position = spawnPosition;
        
        // Make player look at tent
        Vector3 lookDirection = tentPosition - spawnPosition;
        lookDirection.y = 0; // Keep the look rotation level
        if (lookDirection != Vector3.zero)
        {
            player.transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        Debug.Log($"Player spawned at camp: {spawnPosition}, Height: {y}");
    }

    private float GetTerrainHeight(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("No Terrain component found!");
            return 0f;
        }

        return terrain.SampleHeight(position);
    }
} 