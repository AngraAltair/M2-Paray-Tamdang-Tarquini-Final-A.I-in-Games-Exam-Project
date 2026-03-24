using System.Collections;
using UnityEngine;

// Handles aggro detection and combat for enemy units against player units (AIBehavior).
// Mirrors the structure of AIAgression on the player side.
[RequireComponent(typeof(EnemyUnitAI))]
public class EnemyUnitAggro : MonoBehaviour
{
    private EnemyUnitAI EnemyUnitAI;

    [Header("Aggro Settings")]
    public float AggroRadius = 20f;
    public LayerMask PlayerLayerMask; // Set this to the layer your player units are on
    public int MaxDetectableUnits = 10;

    private Collider[] HitColliders;
    private int AmountHit;

    // Target lock — same approach as AIAgression to prevent all enemies piling on one player unit
    private GameObject LockedTarget;

    // Combat state
    private bool IsAttackingPlayer { get; set; }
    private Coroutine AttackCoroutine;

    [Header("Combat Settings")]
    public float AttackDamage = 1f;
    public float AttackRate = 2f;
    public float AttackRange = 20f;

    void Awake()
    {
        EnemyUnitAI = GetComponent<EnemyUnitAI>();
        HitColliders = new Collider[MaxDetectableUnits];
    }

    void Start() { }

    void Update()
    {
        // Don't fight if already dead
        if (EnemyUnitAI.Health <= 0) return;

        AmountHit = Physics.OverlapSphereNonAlloc(transform.position, AggroRadius, HitColliders, PlayerLayerMask);

        // Clear stale entries
        for (int i = AmountHit; i < HitColliders.Length; i++)
        {
            HitColliders[i] = null;
        }

        // Release lock if target gone or out of range
        if (LockedTarget == null || !IsInHitColliders(LockedTarget))
        {
            LockedTarget = null;
        }

        int playerUnitsHit = ReturnPlayerUnitsHit();

        if (playerUnitsHit > 0)
        {
            if (LockedTarget == null)
            {
                LockedTarget = ReturnBestAvailableTarget();
            }

            if (LockedTarget != null && !IsAttackingPlayer)
            {
                IsAttackingPlayer = true;
                EnemyUnitAI.PauseMovement(); // Stop marching toward base while fighting

                if (AttackCoroutine != null) StopCoroutine(AttackCoroutine);
                AttackCoroutine = StartCoroutine(AttackPlayerCoroutine(LockedTarget));
            }
        }
        else
        {
            // No players in range — resume movement toward base if we were fighting
            if (IsAttackingPlayer)
            {
                IsAttackingPlayer = false;
                LockedTarget = null;
                EnemyUnitAI.ResumeMovement();
            }
        }
    }

    public int ReturnPlayerUnitsHit()
    {
        int count = 0;
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.GetComponent<AIBehavior>() != null)
                count++;
        }
        return count;
    }

    private GameObject ReturnBestAvailableTarget()
    {
        GameObject fallback = null;

        foreach (Collider collider in HitColliders)
        {
            if (collider == null) continue;
            AIBehavior player = collider.GetComponent<AIBehavior>();
            if (player == null) continue;

            if (!IsTargetLockedByAnother(collider.gameObject))
                return collider.gameObject;

            if (fallback == null) fallback = collider.gameObject;
        }

        return fallback;
    }

    IEnumerator AttackPlayerCoroutine(GameObject target)
    {
        if (target == null)
        {
            CleanupAttack();
            yield break;
        }

        AIBehavior playerUnit = target.GetComponent<AIBehavior>();
        if (playerUnit == null)
        {
            CleanupAttack();
            yield break;
        }

        float elapsed = 0f;
        float nextAttackTime = AttackRate;

        while (true)
        {
            // Safe null check for destroyed objects
            if (target == null || playerUnit == null || playerUnit.Health <= 0)
                break;

            // If target moved out of range, break off
            if (Vector3.Distance(transform.position, target.transform.position) > AggroRadius * 1.5f)
                break;

            elapsed += Time.deltaTime;

            if (elapsed >= nextAttackTime)
            {
                if (target != null && playerUnit != null && playerUnit.Health > 0)
                {
                    playerUnit.Health -= AttackDamage;
                    Debug.Log($"Enemy attacked player unit for {AttackDamage}. Player HP: {playerUnit.Health}");

                    if (playerUnit.Health <= 0)
                    {
                        Debug.Log("Player unit killed by enemy.");
                        // Let the player unit's own death handling fire (you can add an OnDeath in AIBehavior later)
                        Destroy(target);
                    }
                }
                nextAttackTime += AttackRate;
            }

            // Track toward the target while fighting
            if (target != null)
            {
                EnemyUnitAI.NavigateToTarget(target.transform.position);
            }

            yield return null;
        }

        CleanupAttack();
    }

    private void CleanupAttack()
    {
        IsAttackingPlayer = false;
        LockedTarget = null;
        if (EnemyUnitAI != null) EnemyUnitAI.ResumeMovement();
    }

    private bool IsInHitColliders(GameObject target)
    {
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.gameObject == target)
                return true;
        }
        return false;
    }

    private bool IsTargetLockedByAnother(GameObject target)
    {
        // Check sibling EnemyUnitAggro components under the same parent
        EnemyUnitAggro[] allAggro = transform.parent?.GetComponentsInChildren<EnemyUnitAggro>();
        if (allAggro == null) return false;

        foreach (EnemyUnitAggro other in allAggro)
        {
            if (other == this) continue;
            if (other.LockedTarget == target) return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = IsAttackingPlayer ? Color.red : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, AggroRadius);
    }
}