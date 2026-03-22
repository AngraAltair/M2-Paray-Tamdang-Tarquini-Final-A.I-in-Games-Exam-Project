using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseBehavior : MonoBehaviour
{
    // Private References
    private static BaseBehavior instance;
    private List<GameObject> SpawnedInUnits;
    // private AIBehavior AIBehavior;

    // Base Stats and Resources
    public bool IsBaseAlive { get; set; } = true;
    public float BaseHealth { get; set; } = 100f;
    public float WoodResourceCount { get; set; } = 10f;
    public float SteelResourceCount { get; set; } = 10f;
    public float FoodResourceCount { get; set; } = 10f;
    public float BaseUnits { get; set; } = 10f;

    // Public References
    public GameObject UnitParent;

    // Public Class Initializer
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
        // AIBehavior = GetComponent<AIBehavior>();

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

    public void SendUnits(int unitsToSend)
    {
        // If the base is alive and the units at the base are more than or equal to the units to send (there are units in base available to do the job.), send unit to job
        if (IsBaseAlive && BaseUnits >= unitsToSend)
        {
            float ActiveAndIdleUnits = ReturnCurrentlyActiveAndIdleUnits();

            // Prioritize sending active and idle units first if there are enough to cover the units to send.
            if (ActiveAndIdleUnits > unitsToSend)
            {
                SetUnitBehavior(unitsToSend, "Gather");
                GUIManager.Instance.SetResourceCountText();
            }

            // If there aren't enough active units, spawn remaining units (as long as base has enough to send.)
            if (ActiveAndIdleUnits < unitsToSend)
            {
                float UnitsNeeded = unitsToSend - ActiveAndIdleUnits;
                SpawnUnits(UnitsNeeded);
                SetUnitBehavior(unitsToSend, "Gather");
                GUIManager.Instance.SetResourceCountText();
            }
        }

        // Let's start simple by just spawning units corresponding to the amount we have to send. Then we take from the number of existing units in SpawnedInUnits and set all their behaviors to a certain behavior.
        // SpawnUnits(unitsToSend);

        // foreach (Transform child in UnitParent.transform)
        // {
        //     Vector3 targetPos = ClickManager.Instance.GetLastClickedPosition();
        //     child.gameObject.GetComponent<AIBehavior>().NavigateToTarget(targetPos);
        // }
    }

    // Function for CREATING UNITS.
    public void SpawnUnits(float unitsToSpawn)
    {
        for (int i = 0; i < unitsToSpawn; i++)
        {
            BaseUnits--;
            Instantiate(Resources.Load("Prefabs/Unit (DEBUG)"), transform.position, Quaternion.identity, UnitParent.transform);
        }
    }

    public void SetUnitBehavior(int UnitsNeeded, string behavior)
    {
        int totalChildren = UnitParent.transform.childCount;
        int startIndex = totalChildren - UnitsNeeded;
        
        for (int i = startIndex; i < totalChildren; i++)
        {
            switch (behavior)
                {
                    case "Gather":
                        UnitParent.transform.GetChild(i).GetComponent<AIBehavior>().IsPerformingTask = true;
                        UnitParent.transform.GetChild(i).GetComponent<AIBehavior>().GatherResources(ClickManager.Instance.GetLastClickedPosition());
                        break;
                    default:
                        break;
                }
        }
    }

    public float ReturnCurrentlyActiveAndIdleUnits()
    {
        float ActiveAndIdleUnits = 0f;
        foreach (Transform child in UnitParent.transform)
        {
            AIBehavior AIBehavior = child.gameObject.GetComponent<AIBehavior>();
            if (!AIBehavior.IsPerformingTask)
            {
                ActiveAndIdleUnits++;
            }
        }
        return ActiveAndIdleUnits;
    }
}
