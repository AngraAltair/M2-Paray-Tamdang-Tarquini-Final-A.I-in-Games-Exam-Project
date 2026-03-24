using System.Linq;
using UnityEngine;

// [RequireComponent(typeof(AIBehavior))]
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

    void Awake()
    {
        AIBehavior = GetComponentInParent<AIBehavior>();
        HitColliders = new Collider[MaxDetectableUnits];
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        AmountHit = Physics.OverlapSphereNonAlloc(gameObject.transform.position, AggroRadius, HitColliders, layerMask);

        // Debug.Log(AmountHit);

        // Clear the unused portion of the array
        for (int i = AmountHit; i < HitColliders.Length; i++)
        {
            HitColliders[i] = null;
        }

        if (ReturnEnemyUnitsHit() > 0) {
            AIBehavior.DetectEnemy(ReturnTarget());
        }

        if (ReturnEnemyUnitsHit() <= 0) {
            AIBehavior.StopDetectEnemy();
        }
        // AIBehavior.StopDetectEnemy();
    }

    public int ReturnEnemyUnitsHit() {
        int UnitsHit = 0;
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.GetComponent<EnemyUnitAI>())
            {
                UnitsHit++;
            }
        }
        return UnitsHit;
    }

    public GameObject ReturnTarget()
    {
        foreach (Collider collider in HitColliders)
        {
            if (collider != null && collider.GetComponent<EnemyUnitAI>())
            {
                return collider.gameObject;
            }
        }
        return null;
    }

    public GameObject[] ReturnTargets() {
        GameObject[] targets = new GameObject[ReturnEnemyUnitsHit()];

        for (int i = 0; i < HitColliders.Length; i++) {
            if (HitColliders[i] != null && HitColliders[i].GetComponent<EnemyUnitAI>()) {
                GameObject gameObj = HitColliders[i].gameObject;
                targets[i] = gameObj;
            }
        }

        return targets;
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     // AIBehavior.DetectEnemy(other);
    //     if (other.gameObject.CompareTag("EnemyUnit"))
    //     {
    //         Gizmos.color = Color.red;
    //     }
    //     // if (other.gameObject.CompareTag("EnemyUnit"))
    //     // {
    //     //     Debug.Log("AI is aggressive.");
    //     //     AIBehavior.IsAggressive = true;
    //     // }
    // }

    // void OnTriggerExit(Collider other)
    // {
    //     // AIBehavior.StopDetectEnemy();
    //     Gizmos.color = Color.yellow;

    //     // Debug.Log("AI is not aggressive.");
    //     // AIBehavior.IsAggressive = false;
    // }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(gameObject.transform.position, AggroRadius);

    }
}
