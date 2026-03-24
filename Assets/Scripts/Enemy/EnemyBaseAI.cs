using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyBaseManager))]
public class EnemyBaseAI : MonoBehaviour
{
    private Coroutine SpawnUnitsCoroutine;

    public float UnitsPerSpawn = 4f;
    public Transform EnemyUnitParent;
    public float SpawnCooldown = 3f;
    public GameObject baseTarget;

    void Start()
    {
        if (SpawnUnitsCoroutine != null)
        {
            StopCoroutine(SpawnUnitsCoroutine);
        }
        SpawnUnitsCoroutine = StartCoroutine(SpawnUnits());
    }

    void Update() { }

    IEnumerator SpawnUnits()
    {
        while (GetComponent<EnemyBaseManager>().IsBaseAlive)
        {
            // BUG 5 FIX: Track how many children exist BEFORE spawning, so we can
            // identify only the newly spawned ones and set their path without
            // disturbing units that are already moving/fighting.
            // Previously, SetUnitPath() iterated ALL children every cooldown cycle,
            // resetting the nav destination of units already engaged in combat.
            int childCountBefore = EnemyUnitParent.childCount;

            for (int i = 1; i <= UnitsPerSpawn; i++)
            {
                Instantiate(Resources.Load("Prefabs/EnemyUnit (DEBUG)"), transform.position, Quaternion.identity, EnemyUnitParent);
            }

            Debug.Log("Spawned " + UnitsPerSpawn + " units.");
            SetPathForNewUnits(childCountBefore);

            yield return new WaitForSeconds(SpawnCooldown);
        }
    }

    /// <summary>
    /// Only assigns a path to units spawned in this batch (index >= startIndex).
    /// Existing units are left untouched so they don't get their nav state reset.
    /// </summary>
    void SetPathForNewUnits(int startIndex)
    {
        int totalChildren = EnemyUnitParent.childCount;
        for (int i = startIndex; i < totalChildren; i++)
        {
            Transform child = EnemyUnitParent.GetChild(i);
            EnemyUnitAI unitAI = child.gameObject.GetComponent<EnemyUnitAI>();
            if (unitAI != null)
            {
                unitAI.NavigateToTarget(baseTarget.transform.position);
            }
        }
    }
}