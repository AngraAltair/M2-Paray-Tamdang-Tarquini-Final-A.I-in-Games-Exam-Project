using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIBehavior : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("AI NavMesh Config")]
    public float Health = 5f;
    // public float WalkSpeed = 60f;
    // public float StoppingDistance = 5f;
    private GameObject HomeBase;
    private Vector3 HomeBasePosition;

    // Behavior Bools
    public bool IsPerformingTask { get; set; }

    public bool IsReturningHome { get; set; }
    private bool IsReturningHomeTask { get;set; }

    public bool IsGathering { get; set; }
    private bool IsGatheringTask { get; set; }

    public bool IsAttacking { get; set; }
    private bool IsAttackingTask { get; set; }

    private Coroutine ReturnHomeCoroutine;

    [Header("Gathering Settings")]
    private Coroutine GatheringCoroutine;
    public float GatheringRate = 6f;
    public float GatheringDurationInSeconds = 3f;

    [Header("Attacking Settings")]
    private Coroutine AttackingTaskCoroutine;
    public float AttackDamage = 1f;
    public float AttackRate = 3f;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        // agent.speed = WalkSpeed;
        // agent.stoppingDistance = StoppingDistance;
    }

    void Start()
    {
        HomeBase = GameObject.Find("Base (DEBUG)");
        HomeBasePosition = HomeBase.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("AI Performing Task: " + IsPerformingTask);

        // RETURN HOME AFTER FINISHING A TASK, ONLY ADD TO BASE UNITS WHEN UNIT IS HOME.

        // POSITION CHECK FOR GATHERING TASK so AI will start gathering when they ARRIVE at the target
        if (IsGatheringTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("AI arrived at destination. Starting gathering.");
            IsGathering = true;
            IsGatheringTask = false; // Prevents re-triggering

            if (GatheringCoroutine != null)
            {
                StopCoroutine(GatherResourcesCoroutine());
            }
            GatheringCoroutine = StartCoroutine(GatherResourcesCoroutine());
        }

        // POSITION CHECK FOR ATTACKING TASK
        if (IsAttackingTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && ClickManager.Instance.GetLastClickedObject() != null)
        {
            Debug.Log("AI arrived at destination. Starting attack.");
            IsAttacking = true;
            IsAttackingTask = false; // Prevents re-triggering

            if (AttackingTaskCoroutine != null)
            {
                StopCoroutine(AttackEnemyCoroutine(ClickManager.Instance.GetLastClickedObject()));
            }
            AttackingTaskCoroutine = StartCoroutine(AttackEnemyCoroutine(ClickManager.Instance.GetLastClickedObject()));
        }
        // If the AI is still set to attacking but the enemy base is already gone, navigate back home.
        else if (IsAttackingTask && ClickManager.Instance.GetLastClickedObject() == null)
        {
            NavigateToTarget(HomeBasePosition);
        }

        if (IsReturningHomeTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (ReturnHomeCoroutine != null)
            {
                StopCoroutine(ReturningHomeCoroutine());
            }
            ReturnHomeCoroutine = StartCoroutine(ReturningHomeCoroutine());
        }
    }

    public void NavigateToTarget(Vector3 targetPosition)
    {
        if (targetPosition != null)
        {
            Debug.Log("Navigating to target: " + targetPosition);

            agent.destination = targetPosition;
            agent.SetDestination(targetPosition);
        }
        else
        {
            Debug.Log("Received value is null.");
        }
    }

    public void ReturnHome()
    {
        agent.destination = HomeBasePosition;
        IsReturningHomeTask = true;
        NavigateToTarget(HomeBasePosition);
        Debug.Log("Returning home.");
    }

    public void GatherResources(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
        IsGatheringTask = true;
        NavigateToTarget(targetPosition);
        Debug.Log("Starting to gather resources at: " + targetPosition);
    }

    public void AttackEnemy(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
        IsAttackingTask = true;
        NavigateToTarget(targetPosition);
        Debug.Log("Attacking at: " + targetPosition);
    }

    IEnumerator ReturningHomeCoroutine()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            BaseBehavior.Instance.BaseUnits++;
            GUIManager.Instance.SetResourceCountText();

            IsReturningHome = false;
            IsReturningHomeTask = false;
            IsPerformingTask = false;

            yield return new WaitForEndOfFrame();

            Destroy(gameObject);
        }
    }

    IEnumerator AttackEnemyCoroutine(GameObject target)
    {
        if (target != null)
        {
            EnemyBaseManager enemyBase = target.GetComponent<EnemyBaseManager>();
            float elapsed = 0f;
            float nextAttackTime = AttackRate;

            while (enemyBase.IsBaseAlive)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextAttackTime)
                {
                    enemyBase.ReduceHealth(AttackDamage);
                    nextAttackTime += AttackRate;
                    Debug.Log($"Unit attacked at: {elapsed:0.00}s, next attack at {nextAttackTime:0.00}s");
                }
                yield return null;
            }
        }

        IsAttacking = false;
        IsPerformingTask = false;
        IsAttackingTask = false;
        Debug.Log("Attacking complete.");

        if (HomeBase != null)
        {
            ReturnHome();
        }
    }

    IEnumerator GatherResourcesCoroutine()
    {
        float elapsed = 0f;
        float nextGatherTime = GatheringRate; // first gather after GatheringRate seconds

        while (elapsed < GatheringDurationInSeconds)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextGatherTime)
            {
                // gather once
                BaseBehavior.Instance.WoodResourceCount += 1;
                BaseBehavior.Instance.SteelResourceCount += 1;
                BaseBehavior.Instance.FoodResourceCount += 1;
                GUIManager.Instance.SetResourceCountText();

                nextGatherTime += GatheringRate;
                Debug.Log($"Gathered at {elapsed:0.00}s, next at {nextGatherTime:0.00}s");
            }

            yield return null;
        }

        // done collecting
        IsGathering = false;
        IsPerformingTask = false;
        IsGatheringTask = false;
        Debug.Log("Gathering complete.");

        // return home
        if (HomeBase != null)
            ReturnHome();
    }
}
