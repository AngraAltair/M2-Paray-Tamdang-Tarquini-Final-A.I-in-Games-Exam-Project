using UnityEngine;
using UnityEngine.AI;

public class AIBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    public float Health = 5f;
    public float WalkSpeed = 40f;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = WalkSpeed;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void NavigateToTarget(Vector3 targetPosition)
    {
        if (targetPosition != null)
        {
            // agent.destination = targetPosition;
            Debug.Log("Navigating to target: " + targetPosition);

            agent.SetDestination(targetPosition);
            // Implement navigation logic here, such as using Unity's NavMeshAgent
        }
        else
        {
            Debug.Log("Received value is null.");
        }
    }

    public void GatherResources(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
        if (agent.transform.position != agent.destination)
        {
            NavigateToTarget(targetPosition);
            Debug.Log("Gathering resources at: " + targetPosition);
            // Implement resource gathering logic here, such as checking for resource nodes and collecting resources
        }
    }
}
