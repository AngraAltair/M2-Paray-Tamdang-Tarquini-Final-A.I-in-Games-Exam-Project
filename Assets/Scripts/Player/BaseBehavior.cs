using System.Collections.Generic;
using UnityEngine;

public class BaseBehavior : MonoBehaviour
{
    private static BaseBehavior instance;

    private bool IsBaseAlive = true;
    private float BaseHealth = 100f;
    private float BaseUnits = 10f;
    private List<GameObject> SpawnedInUnits;
    public GameObject UnitParent;

    public static BaseBehavior Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        SpawnedInUnits = new List<GameObject>();


        // BaseUnits = SpawnedUnits.Count;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Units in Base:" + BaseUnits);

        Debug.Log(SpawnedInUnits.Count);
        // foreach(Transform child in UnitParent.transform)
        // {
        //     SpawnedUnits.Add(child.gameObject);
        // }
    }

    public void SendUnits(float unitsToSend)
    {
        // if (IsBaseAlive && BaseUnits >= unitsToSend)
        // {
        //     // BaseUnits -= unitsToSend;
        //     Debug.Log("Sent " + unitsToSend + " units. Remaining: " + BaseUnits);

        // Check what the player last clicked. This dictates the behavior to set the AI to.
        // string lastClickedTag = ClickManager.Instance.GetLastClickedObjectTag();

        // // If there are not enough units, spawn some before sending them to the target. If there are, just send them to the target.
        // if (SpawnedInUnits.Count < unitsToSend)
        // {
        //     Debug.Log("Not enough units active. Spawning...");
        //     SpawnUnits(SpawnedInUnits.Count - unitsToSend);
        // }

        // // If there are no active units, spawn some.
        // if (SpawnedInUnits.Count == 0)
        // {
        //     Debug.Log("No units available to send. Spawning...");
        //     SpawnUnits(unitsToSend);
        // }

        // foreach(Transform child in UnitParent.transform)
        // {
        //     SpawnedInUnits.Add(child.gameObject);
        // }

        // foreach(GameObject unit in SpawnedInUnits)
        // {
        //     Debug.Log("Setting behavior");
        //     // if (lastClickedTag == "ResourceArea")
        //     // {
        //         unit.GetComponent<AIBehavior>().GatherResources(ClickManager.Instance.GetLastClickedPosition());
        //     // }
        //     // else
        //     // {
        //     //     unit.GetComponent<AIBehavior>().NavigateToTarget(ClickManager.Instance.GetLastClickedPosition());
        //     // }
        // }

        // Let's start simple by just spawning units corresponding to the amount we have to send. Then we take from the number of existing units in SpawnedInUnits and set all their behaviors to a certain behavior.
        SpawnUnits(unitsToSend);

        foreach (Transform child in UnitParent.transform)
        {
            // child.gameObject.GetComponent<AIBehavior>().NavigateToTarget(new Vector3(0,0,0));
            // child.gameObject.GetComponent<AIBehavior>().DebugFunction();
            // child.gameObject.GetComponent<AIBehavior>().DebugFunctionTargetPos(ClickManager.Instance.GetLastClickedPosition());
            Vector3 targetPos = ClickManager.Instance.GetLastClickedPosition();
            child.gameObject.GetComponent<AIBehavior>().NavigateToTarget(targetPos);
        }

        // }
        // else
        // {
        //     Debug.Log("Not enough units to send or base is destroyed.");
        // }
    }

    public void SpawnUnits(float unitsToSpawn)
    {
        for (int i = 0; i < unitsToSpawn; i++)
        {
            Instantiate(Resources.Load("Prefabs/Unit (DEBUG)"), transform.position, Quaternion.identity, UnitParent.transform);
        }
        // BaseUnits -= unitsToSpawn;
        // Debug.Log("Spawned " + unitsToSpawn + " units. Remaining: " + BaseUnits);
    }
}
