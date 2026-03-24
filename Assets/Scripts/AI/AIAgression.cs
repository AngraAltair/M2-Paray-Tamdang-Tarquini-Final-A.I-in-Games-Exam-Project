using UnityEngine;

// This script is for detecting if there is an enemy nearby and simulating battling behavior with other AI units.
public class AIAgression : MonoBehaviour
{
    private AIBehavior AIBehavior;

    [Header("AI Aggro Settings")]
    public float AggroRadius;
    public LayerMask layerMask;
    [Tooltip("Number of max detectable units that the overlap sphere can detect.")]
    public int MaxDetectableUnits;

    private Collider[] HitColliders;
    private int AmountHit;

    // BUG B FIX: Each AIAgression instance now tracks which enemy it has locked onto.
    // Previously, ReturnTarget() always returned the FIRST hit collider in the array.
    // When 8 player units all detected the same 1 enemy, all 8 called
    // TakeDamage() on the same target simultaneously, killing it in one frame
    // and leaving all 8 coroutines in a broken cleanup state.
    // Now each aggro component locks onto a specific target and only changes it
    // when that target is gone.
    private GameObject LockedTarget;

    void Awake()
    {
        AIBehavior = GetComponentInParent<AIBehavior>();
        HitColliders = new Collider[MaxDetectableUnits];
    }

    void Start() { }

    void Update()
    {
        AmountHit = Physics.OverlapSphereNonAlloc(gameObject.transform.position, AggroRadius, HitColliders, layerMask);

        // Clear the unused portion of the array
        for (int i = AmountHit; i < HitColliders.Length; i++)
        {
            HitColliders[i] = null;
        }

        // BUG B FIX: If our locked target is gone (destroyed or out of range), release the lock
        // so we can acquire a new one. Using the Unity-overloaded == null check here to
        // catch destroyed GameObjects correctly.
        if (LockedTarget == null || !IsInHitColliders(LockedTarget))
        {
            LockedTarget = null;
        }

        int enemiesHit = ReturnEnemyUnitsHit();

        if (enemiesHit > 0)
        {
            // Only lock onto a target if we don't already have one
            if (LockedTarget == null)
            {
                LockedTarget = ReturnBestAvailableTarget();
            }

            if (LockedTarget != null)
            {
                AIBehavior.DetectEnemy(LockedTarget);
            }
        }
        else
        {
            LockedTarget = null;
            AIBehavior.StopDetectEnemy();
        }
    }

    public int ReturnEnemyUnitsHit()
    {
        int unitsHit = 0;
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.GetComponent<EnemyUnitAI>() != null)
            {
                unitsHit++;
            }
        }
        return unitsHit;
    }

    /// <summary>
    /// Returns the first available enemy target. Prefers targets that are not
    /// already the locked target of other nearby AIAgression components, to
    /// spread damage across multiple enemies instead of piling on one.
    /// </summary>
    public GameObject ReturnBestAvailableTarget()
    {
        GameObject fallback = null;

        foreach (Collider collider in HitColliders)
        {
            if (collider == null) continue;
            EnemyUnitAI enemy = collider.GetComponent<EnemyUnitAI>();
            if (enemy == null) continue;

            // BUG B FIX: Check if any sibling AIAgression scripts (other player units)
            // have already locked onto this target. If so, prefer a different one.
            // This spreads player units across available enemies instead of all piling
            // onto the same one and killing it in a single frame.
            if (!IsTargetLockedByAnother(collider.gameObject))
            {
                return collider.gameObject;
            }

            // Keep as fallback in case all targets are already locked
            if (fallback == null)
            {
                fallback = collider.gameObject;
            }
        }

        return fallback;
    }

    // Legacy method kept for any external callers
    public GameObject ReturnTarget()
    {
        return LockedTarget != null ? LockedTarget : ReturnBestAvailableTarget();
    }

    public GameObject[] ReturnTargets()
    {
        int count = ReturnEnemyUnitsHit();
        GameObject[] targets = new GameObject[count];
        int idx = 0;

        for (int i = 0; i < HitColliders.Length; i++)
        {
            if (HitColliders[i] != null && HitColliders[i].GetComponent<EnemyUnitAI>() != null)
            {
                targets[idx++] = HitColliders[i].gameObject;
                if (idx >= count) break;
            }
        }

        return targets;
    }

    /// <summary>
    /// Checks whether this enemy GameObject is inside our current HitColliders.
    /// Used to verify a locked target is still in range.
    /// </summary>
    private bool IsInHitColliders(GameObject target)
    {
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.gameObject == target)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if another AIAgression component on a sibling unit has already locked
    /// onto this target, so we can preferentially pick a different one.
    /// </summary>
    private bool IsTargetLockedByAnother(GameObject target)
    {
        // Look for other AIAgression scripts in the same UnitParent hierarchy
        AIAgression[] allAggro = transform.parent?.parent?.GetComponentsInChildren<AIAgression>();
        if (allAggro == null) return false;

        foreach (AIAgression other in allAggro)
        {
            if (other == this) continue;
            if (other.LockedTarget == target) return true;
        }
        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = LockedTarget != null ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(gameObject.transform.position, AggroRadius);
    }
}