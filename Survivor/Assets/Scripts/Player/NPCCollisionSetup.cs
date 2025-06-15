using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class NPCCollisionSetup : MonoBehaviour
{
    private void Awake()
    {
        // Ensure the NPC has the proper tag
        gameObject.tag = "NPC";

        // Get or add CharacterController
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            // Configure CharacterController for proper collision
            controller.radius = 0.5f;
            controller.height = 2f;
            controller.stepOffset = 0.3f;
            controller.skinWidth = 0.08f;
            controller.center = new Vector3(0, 1f, 0);
        }

        // Get or add NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            // Configure NavMeshAgent for proper collision avoidance
            agent.radius = 0.5f;
            agent.height = 2f;
            agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = Random.Range(0, 100);
        }
    }
} 