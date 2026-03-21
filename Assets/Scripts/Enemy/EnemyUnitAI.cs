using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnitAI : MonoBehaviour
{
    private NavMeshAgent agent;

    public float Health = 5f;
    public float WalkSpeed = 15f;
    // private GameObject baseTarget;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = WalkSpeed;
    }

    // Start is called before the first frame update
    void Start()
    {
        // NavigateToTarget()
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

}
