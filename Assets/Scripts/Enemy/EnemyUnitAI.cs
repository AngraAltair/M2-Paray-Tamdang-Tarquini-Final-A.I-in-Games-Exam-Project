using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyUnitAI : MonoBehaviour
{
    private NavMeshAgent agent;

    public float Health {get;set;} = 2f;
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
        if (Health <= 0)
        {
            Destroy(gameObject);
        }

    }

    public void PauseMovement()
    {
        agent.isStopped = true;
    }

    public void ResumeMovement()
    {
        agent.isStopped = false;
    }

    public void TakeDamage(float AttackDamage)
    {
        PauseMovement();
        Debug.Log("Enemy taking damage of: " + AttackDamage);
        Health -= AttackDamage;
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
