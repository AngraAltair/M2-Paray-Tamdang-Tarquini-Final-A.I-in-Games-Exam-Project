using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    private AIAgression AIAgressionScript;
    private GameObject EnemyUnitTarget { get; set; }
    // private GameObject[] EnemyUnitTargets;

    [Header("AI NavMesh Config")]
    public float Health = 5f;
    // public float WalkSpeed = 60f;
    // public float StoppingDistance = 5f;
    private GameObject HomeBase;
    private Vector3 HomeBasePosition;

    // Behavior Bools
    // All bools ending with "Task" is for scheduling. This is what makes sure the AI heads over to the target first before the task logic triggers.
    // All bools without "Task" are the flags used for actually triggering task logic.
    public bool IsPerformingTask { get; set; }

    public bool IsReturningHome { get; set; }
    private bool IsReturningHomeTask { get; set; }

    public bool IsGathering { get; set; }
    private bool IsGatheringTask { get; set; }

    public bool IsAttackingBase { get; set; }
    private bool IsAttackingBaseTask { get; set; }
    private bool IsAttackingBaseCoroutineRunning { get; set; }

    // Behavior bools specifically for attacking other AI units.
    public bool IsAggressive { get; set; }
    public bool IsAttackingEnemy { get; set; }
    private bool IsAttackingEnemyTask { get; set; }
    private Coroutine AttackEnemyCoroutine;


    private Coroutine ReturnHomeCoroutine;

    // Task resumption variables
    private bool wasGathering = false;
    private bool wasAttackingBase = false;
    private Vector3 gatheringTarget;
    private GameObject attackingBaseTarget;

    [Header("Gathering Settings")]
    private Coroutine GatheringCoroutine;
    public float GatheringRate = 6f;
    public float GatheringDurationInSeconds = 3f;

    [Header("Attacking Settings")]
    private Coroutine AttackingTaskCoroutine;
    public float AttackDamage = 1f;
    public float AttackRate = 3f;
    public float AttackRange = 30f;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        AIAgressionScript = GetComponentInChildren<AIAgression>();
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
        // Debug.Log("Update called on " + gameObject.name);
        // Debug.Log("Is Attacking Base Task: " + IsAttackingBaseTask);
        // Debug.Log("Was Attacking Base: " + wasAttackingBase);
        // Debug.Log("AI Performing Task: " + IsPerformingTask);
        // Debug.Log("Attack Base Coroutine running: " + IsAttackingBaseCoroutineRunning);

        // RETURN HOME AFTER FINISHING A TASK, ONLY ADD TO BASE UNITS WHEN UNIT IS HOME.

        // POSITION CHECK FOR GATHERING TASK so AI will start gathering when they ARRIVE at the target
        if (IsGatheringTask && !IsAggressive && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("AI arrived at destination. Starting gathering.");
            IsGathering = true;
            IsGatheringTask = false; // Prevents re-triggering

            if (GatheringCoroutine != null)
            {
                StopCoroutine(GatheringCoroutine);
                GatheringCoroutine = StartCoroutine(GatherResourcesCoroutine());
            }
            GatheringCoroutine = StartCoroutine(GatherResourcesCoroutine());
        }

        // POSITION CHECK FOR ATTACKING TASK
        if (IsAttackingBaseTask && !IsAggressive && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && ClickManager.Instance.GetLastClickedObject() != null)
        {
            Debug.Log("AI arrived at destination. Starting attack.");
            IsAttackingBase = true;
            IsAttackingBaseTask = false; // Prevents re-triggering

            if (AttackingTaskCoroutine != null)
            {
                StopCoroutine(AttackingTaskCoroutine);
                AttackingTaskCoroutine = StartCoroutine(AttackBaseCoroutine(ClickManager.Instance.GetLastClickedObject()));
            }
            AttackingTaskCoroutine = StartCoroutine(AttackBaseCoroutine(ClickManager.Instance.GetLastClickedObject()));
        }
        // // If the AI is still set to attacking but the enemy base is already gone, navigate back home.
        // else if (IsAttackingBaseTask && ClickManager.Instance.GetLastClickedObject() == null)
        // {
        //     NavigateToTarget(HomeBasePosition);
        // }

        // AGGRO AND POSITION CHECK FOR ATTACKING ENEMY UNITS
        if (IsAggressive && !agent.pathPending && agent.remainingDistance <= AttackRange)
        {
            Debug.Log("AI attacking enemy.");

            if (AttackEnemyCoroutine != null)
            {
                StopCoroutine(AttackEnemyCoroutine);
                AttackEnemyCoroutine = StartCoroutine(AttackEnemyAICoroutine(EnemyUnitTarget));
            }
            AttackEnemyCoroutine = StartCoroutine(AttackEnemyAICoroutine(EnemyUnitTarget));
        }

        // Return home from a task.
        if (IsReturningHomeTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (ReturnHomeCoroutine != null)
            {
                StopCoroutine(ReturnHomeCoroutine);
                ReturnHomeCoroutine = StartCoroutine(ReturningHomeCoroutine());
            }
            ReturnHomeCoroutine = StartCoroutine(ReturningHomeCoroutine());
            Debug.Log("Returning home.");
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

    public void DetectEnemy(GameObject TargetUnit)
    {
        // Save previous task state
        if (IsGatheringTask || IsGathering)
        {
            wasGathering = true;
            IsGatheringTask = false;
            IsGathering = false;
            if (GatheringCoroutine != null)
            {
                StopCoroutine(GatheringCoroutine);
                GatheringCoroutine = null;
            }
        }
        if (IsAttackingBaseTask || IsAttackingBase)
        {
            wasAttackingBase = true;
            IsAttackingBaseTask = false;
            IsAttackingBase = false;
            if (AttackingTaskCoroutine != null)
            {
                StopCoroutine(AttackingTaskCoroutine);
                AttackingTaskCoroutine = null;
            }
        }

        // STOP ALL BEHAVIORS
        IsAggressive = true;
        agent.destination = TargetUnit.transform.position;
        EnemyUnitTarget = TargetUnit;
        Debug.Log("AI is aggressive.");
    }

    public void StopDetectEnemy()
    {
        IsAggressive = false;
        IsAttackingEnemy = false;
        IsAttackingEnemyTask = false;
        Debug.Log("AI no longer detects enemy.");

        // Resume previous task if not currently attacking an enemy
        if (!IsAttackingEnemy)
        {
            ResumePreviousTask();
        }
    }

    public void GatherResources(Vector3 targetPosition)
    {
        gatheringTarget = targetPosition;
        agent.destination = targetPosition;
        IsGatheringTask = true;
        NavigateToTarget(targetPosition);
        Debug.Log("Starting to gather resources at: " + targetPosition);
    }

    public void AttackBase(Vector3 targetPosition)
    {
        attackingBaseTarget = ClickManager.Instance.GetLastClickedObject();
        agent.destination = targetPosition;
        IsAttackingBaseTask = true;
        NavigateToTarget(targetPosition);
        Debug.Log("Attacking at: " + targetPosition);
    }

    private void ResumePreviousTask()
    {
        if (wasGathering)
        {
            wasGathering = false;
            IsGatheringTask = true;
            agent.destination = gatheringTarget;
            Debug.Log("Resuming gathering.");
        }
        else if (wasAttackingBase)
        {
            wasAttackingBase = false;
            IsAttackingBaseTask = true;
            if (attackingBaseTarget != null)
            {
                agent.destination = attackingBaseTarget.transform.position;
                Debug.Log("Resuming attacking base.");
            }
        }
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

    IEnumerator AttackBaseCoroutine(GameObject target)
    {
        IsAttackingBaseCoroutineRunning = true;

        if (target != null)
        {
            if (target.GetComponent<EnemyBaseManager>() != null)
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

            IsAttackingBase = false;
            IsPerformingTask = false;
            IsAttackingBaseTask = false;
            IsAttackingBaseCoroutineRunning = false;
            Debug.Log("Attacking complete.");

            if (HomeBase != null)
            {
                ReturnHome();
            }
        }
    }

    IEnumerator AttackEnemyAICoroutine(GameObject target)
    {
        if (target.GetComponent<EnemyUnitAI>() != null)
        {
            IsPerformingTask = true;
            EnemyUnitAI enemyUnit = target.GetComponent<EnemyUnitAI>();

            float elapsed = 0f;
            float nextAttackTime = AttackRate;

            while (enemyUnit.Health > 0)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextAttackTime)
                {
                    enemyUnit.TakeDamage(AttackDamage);
                    nextAttackTime += AttackRate;
                    Debug.Log($"Unit attacked enemy at: {elapsed:0.00}s, next attack at {nextAttackTime:0.00}s");
                }
                agent.destination = target.transform.position;
                yield return null;
            }
        }
        else
        {
            Debug.Log("Target has no EnemyUnitAI script.");
            IsAttackingEnemy = false;
            IsPerformingTask = false;
            IsAttackingEnemyTask = false;
            IsAggressive = false; // Enemy defeated, no longer aggressive
        }

        IsAttackingEnemy = false;
        IsPerformingTask = false;
        IsAttackingEnemyTask = false;
        IsAggressive = false; // Enemy defeated, no longer aggressive
        Debug.Log("Attacking enemy complete.");

        // Resume previous task
        ResumePreviousTask();
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
        if (HomeBase != null) ReturnHome();
    }
}

