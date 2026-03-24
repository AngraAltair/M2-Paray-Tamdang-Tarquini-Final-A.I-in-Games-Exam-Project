using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIBehavior : MonoBehaviour
{
    private NavMeshAgent agent;
    private AIAgression AIAgressionScript;
    private GameObject EnemyUnitTarget { get; set; }

    [Header("AI NavMesh Config")]
    public float Health = 5f;
    private GameObject HomeBase;
    private Vector3 HomeBasePosition;

    // Behavior Bools
    public bool IsPerformingTask { get; set; }

    public bool IsReturningHome { get; set; }
    private bool IsReturningHomeTask { get; set; }

    public bool IsGathering { get; set; }
    private bool IsGatheringTask { get; set; }

    public bool IsAttackingBase { get; set; }
    private bool IsAttackingBaseTask { get; set; }
    private bool IsAttackingBaseCoroutineRunning { get; set; }

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

    // Coroutine guard booleans (prevent Update() from spawning duplicate coroutines each frame)
    private bool IsGatheringCoroutineRunning { get; set; }
    private bool IsReturningHomeCoroutineRunning { get; set; }
    private bool IsAttackingEnemyCoroutineRunning { get; set; }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        AIAgressionScript = GetComponentInChildren<AIAgression>();
    }

    void Start()
    {
        HomeBase = GameObject.Find("Base (DEBUG)");
        HomeBasePosition = HomeBase.transform.position;
    }

    void Update()
    {
        // POSITION CHECK FOR GATHERING TASK
        if (IsGatheringTask && !IsAggressive && !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance && !IsGatheringCoroutineRunning)
        {
            Debug.Log("AI arrived at destination. Starting gathering.");
            IsGathering = true;
            IsGatheringTask = false;

            if (GatheringCoroutine != null) StopCoroutine(GatheringCoroutine);
            IsGatheringCoroutineRunning = true;
            GatheringCoroutine = StartCoroutine(GatherResourcesCoroutine());
        }

        // POSITION CHECK FOR ATTACKING BASE TASK
        // BUG A FIX: Removed the !IsAggressive guard. If an enemy walked into aggro range
        // right as a unit arrived at the base, IsAggressive=true used to block the attack
        // coroutine from ever starting. Base attack takes priority on arrival.
        if (IsAttackingBaseTask && !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance
            && ClickManager.Instance.GetLastClickedObject() != null
            && !IsAttackingBaseCoroutineRunning)
        {
            Debug.Log("AI arrived at destination. Starting base attack.");
            IsAttackingBase = true;
            IsAttackingBaseTask = false;

            if (AttackingTaskCoroutine != null) StopCoroutine(AttackingTaskCoroutine);
            IsAttackingBaseCoroutineRunning = true;
            AttackingTaskCoroutine = StartCoroutine(AttackBaseCoroutine(ClickManager.Instance.GetLastClickedObject()));
        }

        // AGGRO CHECK: fight enemies encountered en route OR while returning home
        // BUG A FIX: Removed !IsAttackingBase check from here — the guard is now inside
        // DetectEnemy() itself, which only blocks interruption when already mid-assault
        // (IsAttackingBase=true), not during travel (IsAttackingBaseTask=true).
        if (IsAggressive && !agent.pathPending
            && agent.remainingDistance <= AttackRange
            && !IsAttackingEnemyCoroutineRunning
            && !IsAttackingBase)
        {
            Debug.Log("AI attacking enemy unit.");
            if (AttackEnemyCoroutine != null) StopCoroutine(AttackEnemyCoroutine);
            IsAttackingEnemyCoroutineRunning = true;
            AttackEnemyCoroutine = StartCoroutine(AttackEnemyAICoroutine(EnemyUnitTarget));
        }

        // RETURN HOME CHECK
        if (IsReturningHomeTask && !agent.pathPending
            && agent.remainingDistance <= agent.stoppingDistance
            && !IsReturningHomeCoroutineRunning)
        {
            if (ReturnHomeCoroutine != null) StopCoroutine(ReturnHomeCoroutine);
            IsReturningHomeCoroutineRunning = true;
            ReturnHomeCoroutine = StartCoroutine(ReturningHomeCoroutine());
        }
    }

    public void NavigateToTarget(Vector3 targetPosition)
    {
        Debug.Log("Navigating to target: " + targetPosition);
        agent.destination = targetPosition;
        agent.SetDestination(targetPosition);
    }

    public void ReturnHome()
    {
        agent.destination = HomeBasePosition;
        IsReturningHomeTask = true;
        IsReturningHomeCoroutineRunning = false;
        NavigateToTarget(HomeBasePosition);
        Debug.Log("Returning home.");
    }

    public void DetectEnemy(GameObject TargetUnit)
    {
        // BUG A FIX: Only block on IsAttackingBase (mid-assault), NOT IsAttackingBaseTask
        // (still travelling). This lets units fight enemies they pass through on the way
        // to the base, while still protecting an ongoing base-damage loop from interruption.
        if (IsAttackingBase)
        {
            return;
        }

        if (IsGatheringTask || IsGathering)
        {
            wasGathering = true;
            IsGatheringTask = false;
            IsGathering = false;
            IsGatheringCoroutineRunning = false;
            if (GatheringCoroutine != null)
            {
                StopCoroutine(GatheringCoroutine);
                GatheringCoroutine = null;
            }
        }

        if (IsAttackingBaseTask)
        {
            // Save the base attack task so we resume it after the skirmish
            wasAttackingBase = true;
            attackingBaseTarget = ClickManager.Instance.GetLastClickedObject();
        }

        IsAggressive = true;
        agent.destination = TargetUnit.transform.position;
        EnemyUnitTarget = TargetUnit;
        Debug.Log("AI is aggressive.");
    }

    public void StopDetectEnemy()
    {
        // Only run cleanup if we were actually aggressive (prevents 60x/sec spam from AIAgression)
        if (!IsAggressive) return;

        IsAggressive = false;
        IsAttackingEnemy = false;
        IsAttackingEnemyTask = false;
        IsAttackingEnemyCoroutineRunning = false;
        Debug.Log("AI no longer detects enemy.");

        if (AIAgressionScript.ReturnEnemyUnitsHit() <= 0 && !IsAttackingEnemy)
        {
            ResumePreviousTask();
        }
    }

    public void GatherResources(Vector3 targetPosition)
    {
        gatheringTarget = targetPosition;
        agent.destination = targetPosition;
        IsGatheringTask = true;
        IsGatheringCoroutineRunning = false;
        NavigateToTarget(targetPosition);
        Debug.Log("Starting to gather resources at: " + targetPosition);
    }

    public void AttackBase(Vector3 targetPosition)
    {
        attackingBaseTarget = ClickManager.Instance.GetLastClickedObject();
        agent.destination = targetPosition;
        IsAttackingBaseTask = true;
        IsAttackingBaseCoroutineRunning = false;
        NavigateToTarget(targetPosition);
        Debug.Log("Attacking base at: " + targetPosition);
    }

    private void ResumePreviousTask()
    {
        if (wasGathering)
        {
            wasGathering = false;
            IsGatheringTask = true;
            IsGatheringCoroutineRunning = false;
            agent.destination = gatheringTarget;
            Debug.Log("Resuming gathering.");
        }
        else if (wasAttackingBase)
        {
            wasAttackingBase = false;
            if (attackingBaseTarget != null
                && attackingBaseTarget.GetComponent<EnemyBaseManager>() != null
                && attackingBaseTarget.GetComponent<EnemyBaseManager>().IsBaseAlive)
            {
                IsAttackingBaseTask = true;
                IsAttackingBaseCoroutineRunning = false;
                agent.destination = attackingBaseTarget.transform.position;
                Debug.Log("Resuming attacking base.");
            }
            else
            {
                ReturnHome();
                Debug.Log("Cannot resume attacking base, returning home.");
            }
        }
    }

    IEnumerator ReturningHomeCoroutine()
    {
        yield return null; // Let remainingDistance update after pathfinding

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            BaseBehavior.Instance.BaseUnits++;
            GUIManager.Instance.SetResourceCountText();

            IsReturningHome = false;
            IsReturningHomeTask = false;
            IsPerformingTask = false;
            IsReturningHomeCoroutineRunning = false;

            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }
        else
        {
            // Not there yet — reset guard so Update() can retry next frame
            IsReturningHomeCoroutineRunning = false;
        }
    }

    IEnumerator AttackBaseCoroutine(GameObject target)
    {
        if (target != null && target.GetComponent<EnemyBaseManager>() != null)
        {
            EnemyBaseManager enemyBase = target.GetComponent<EnemyBaseManager>();
            float elapsed = 0f;
            float nextAttackTime = AttackRate;

            while (enemyBase != null && enemyBase.IsBaseAlive)
            {
                elapsed += Time.deltaTime;

                // BUG FIX: Check distance before attacking - units should only damage when close enough
                float distanceToBase = Vector3.Distance(transform.position, target.transform.position);
                if (distanceToBase > AttackRange + 5f)  // Add buffer to account for stopping distance
                {
                    // Too far - move closer instead of attacking
                    agent.SetDestination(target.transform.position);
                    yield return null;
                    continue;
                }

                if (elapsed >= nextAttackTime)
                {
                    enemyBase.ReduceHealth(AttackDamage);
                    nextAttackTime += AttackRate;
                    Debug.Log($"Unit attacked base at: {elapsed:0.00}s, next at {nextAttackTime:0.00}s");
                }
                yield return null;
            }
        }

        IsAttackingBase = false;
        IsPerformingTask = false;
        IsAttackingBaseTask = false;
        IsAttackingBaseCoroutineRunning = false;
        Debug.Log("Base attack complete.");

        if (HomeBase != null) ReturnHome();
    }

    IEnumerator AttackEnemyAICoroutine(GameObject target)
    {
        // BUG C FIX: Cache the component reference immediately and use Unity's overloaded
        // == null check (which catches destroyed GameObjects, not just C# null).
        // Previously, target != null was true even on the frame Unity called Destroy(),
        // causing GetComponent<>() and Health access to throw MissingReferenceException.
        if (target == null)
        {
            CleanupEnemyAttack();
            yield break;
        }

        EnemyUnitAI enemyUnit = target.GetComponent<EnemyUnitAI>();
        if (enemyUnit == null)
        {
            Debug.Log("Target has no EnemyUnitAI.");
            CleanupEnemyAttack();
            yield break;
        }

        float elapsed = 0f;
        float nextAttackTime = AttackRate;

        while (true)
        {
            // BUG C FIX: Unity's == null correctly detects destroyed objects here.
            // We check this at the TOP of each loop iteration, before any access.
            if (target == null || enemyUnit == null || enemyUnit.Health <= 0)
            {
                break;
            }

            elapsed += Time.deltaTime;

            if (elapsed >= nextAttackTime)
            {
                // BUG C + BUG B FIX: Re-check right before dealing damage. Because multiple
                // player units each run their own coroutine against the same enemy (before
                // AIAgression's target-locking spreads them out), one unit may have already
                // killed the target between this frame's yield and this damage call.
                if (target != null && enemyUnit != null && enemyUnit.Health > 0)
                {
                    enemyUnit.TakeDamage(AttackDamage);
                }
                nextAttackTime += AttackRate;
            }

            if (target != null)
            {
                agent.destination = target.transform.position;
            }

            yield return null;
        }

        Debug.Log("Enemy unit attack complete.");
        CleanupEnemyAttack();
        ResumePreviousTask();
    }

    /// <summary>
    /// Resets all enemy-combat state flags. Does NOT touch IsPerformingTask —
    /// enemy combat is an interruption, not the primary task.
    /// </summary>
    private void CleanupEnemyAttack()
    {
        IsAttackingEnemy = false;
        IsAttackingEnemyTask = false;
        IsAttackingEnemyCoroutineRunning = false;
        IsAggressive = false;
    }

    IEnumerator GatherResourcesCoroutine()
    {
        float elapsed = 0f;
        float nextGatherTime = GatheringRate;

        while (elapsed < GatheringDurationInSeconds)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextGatherTime)
            {
                BaseBehavior.Instance.WoodResourceCount += 1;
                BaseBehavior.Instance.SteelResourceCount += 1;
                BaseBehavior.Instance.FoodResourceCount += 1;
                GUIManager.Instance.SetResourceCountText();

                nextGatherTime += GatheringRate;
                Debug.Log($"Gathered at {elapsed:0.00}s, next at {nextGatherTime:0.00}s");
            }

            yield return null;
        }

        IsGathering = false;
        IsPerformingTask = false;
        IsGatheringTask = false;
        IsGatheringCoroutineRunning = false;
        Debug.Log("Gathering complete.");

        if (HomeBase != null) ReturnHome();
    }
}