using System.Collections;
using UnityEngine;

public class EnemyBaseAI : MonoBehaviour
{
    private bool IsBaseAlive = true;
    private Coroutine SpawnUnitsCoroutine;

    public float UnitsPerSpawn = 4f;
    public Transform EnemyUnitParent;
    public float SpawnCooldown = 3f;
    public GameObject baseTarget;
    // Start is called before the first frame update
    
    void Start()
    {
        if (SpawnUnitsCoroutine != null)
        {
            StopCoroutine(SpawnUnitsCoroutine);
            SpawnUnitsCoroutine = StartCoroutine(SpawnUnits());
        } else
        {
            SpawnUnitsCoroutine = StartCoroutine(SpawnUnits());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnUnits()
    {
        while (IsBaseAlive)
        {
            for (int i = 1; i <= UnitsPerSpawn; i++)
            {
                Instantiate(Resources.Load("Prefabs/EnemyUnit (DEBUG)"), transform.position, Quaternion.identity, EnemyUnitParent);
            }
            Debug.Log("Spawned " + UnitsPerSpawn + " units.");
            SetUnitPath();
            yield return new WaitForSeconds(SpawnCooldown);
        }
    }

    void SetUnitPath()
    {
        foreach(Transform child in EnemyUnitParent.transform)
        {
            child.gameObject.GetComponent<EnemyUnitAI>().NavigateToTarget(baseTarget.transform.position);
        }
    }
}
