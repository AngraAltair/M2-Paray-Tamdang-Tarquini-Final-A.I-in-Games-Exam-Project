using System.Collections.Generic;
using UnityEngine;

public class BaseBehavior : MonoBehaviour
{
    private static BaseBehavior instance;
    private List<GameObject> SpawnedInUnits;

    public bool IsBaseAlive { get; set; } = true;
    public float BaseHealth { get; set; } = 100f;
    public float WoodResourceCount { get; set; } = 10f;
    public float SteelResourceCount { get; set; } = 10f;
    public float FoodResourceCount { get; set; } = 10f;
    public float BaseUnits { get; set; } = 10f;

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

    void Start() { }

    void Update()
    {
        Debug.Log("Active and Idle Units: " + ReturnCurrentlyActiveAndIdleUnits());
    }

    public void SendUnits(int unitsToSend)
    {
        if (IsBaseAlive && BaseUnits >= unitsToSend)
        {
            float activeAndIdleUnits = ReturnCurrentlyActiveAndIdleUnits();
            string behavior = ReturnBehavior();

            // BUG 2 FIX: Collect the actual idle unit GameObjects rather than relying on
            // child index math. The old approach (startIndex = totalChildren - UnitsNeeded)
            // was wrong whenever there was a mix of busy and idle units — it would pick the
            // last N children by position, not by idle status, assigning tasks to units that
            // were already mid-task or to the wrong newly-spawned units.
            List<GameObject> idleUnits = GetIdleUnits();
            int idleCount = idleUnits.Count;

            if (idleCount >= unitsToSend)
            {
                // Enough idle units exist — assign directly without spawning
                AssignBehaviorToUnits(idleUnits, unitsToSend, behavior);
                GUIManager.Instance.SetResourceCountText();
            }
            else
            {
                // Not enough idle units — spawn the remainder first, then assign
                int unitsNeeded = unitsToSend - idleCount;
                SpawnUnits(unitsNeeded);

                // Re-collect idle units (includes freshly spawned ones)
                idleUnits = GetIdleUnits();
                AssignBehaviorToUnits(idleUnits, unitsToSend, behavior);
                GUIManager.Instance.SetResourceCountText();
            }
        }
    }

    public void SpawnUnits(float unitsToSpawn)
    {
        for (int i = 0; i < unitsToSpawn; i++)
        {
            BaseUnits--;
            Instantiate(Resources.Load("Prefabs/Unit (DEBUG)"), transform.position, Quaternion.identity, UnitParent.transform);
        }
    }

    /// <summary>
    /// Returns a list of all currently idle (not performing a task) units.
    /// </summary>
    private List<GameObject> GetIdleUnits()
    {
        List<GameObject> idle = new List<GameObject>();
        foreach (Transform child in UnitParent.transform)
        {
            AIBehavior ai = child.gameObject.GetComponent<AIBehavior>();
            if (ai != null && !ai.IsPerformingTask)
            {
                idle.Add(child.gameObject);
            }
        }
        return idle;
    }

    /// <summary>
    /// Assigns the given behavior to up to 'count' units from the provided list.
    /// </summary>
    private void AssignBehaviorToUnits(List<GameObject> units, int count, string behavior)
    {
        int assigned = 0;
        foreach (GameObject unit in units)
        {
            if (assigned >= count) break;

            AIBehavior ai = unit.GetComponent<AIBehavior>();
            if (ai == null) continue;

            ai.IsPerformingTask = true;
            switch (behavior)
            {
                case "Gather":
                    ai.GatherResources(ClickManager.Instance.GetLastClickedPosition());
                    break;
                case "Attack":
                    ai.AttackBase(ClickManager.Instance.GetLastClickedPosition());
                    break;
                default:
                    ai.NavigateToTarget(ClickManager.Instance.GetLastClickedPosition());
                    break;
            }
            assigned++;
        }
    }

    // Kept for backwards compatibility — used by GUIManager and elsewhere
    public void SetUnitBehavior(int UnitsNeeded, string behavior = null)
    {
        List<GameObject> idleUnits = GetIdleUnits();
        AssignBehaviorToUnits(idleUnits, UnitsNeeded, behavior);
    }

    public float ReturnCurrentlyActiveAndIdleUnits()
    {
        float activeAndIdleUnits = 0f;
        foreach (Transform child in UnitParent.transform)
        {
            AIBehavior ai = child.gameObject.GetComponent<AIBehavior>();
            if (ai != null && !ai.IsPerformingTask)
            {
                activeAndIdleUnits++;
            }
        }
        return activeAndIdleUnits;
    }

    public string ReturnBehavior()
    {
        string behavior = ClickManager.Instance.GetLastClickedObjectTag() switch
        {
            "ResourceArea" => "Gather",
            "EnemyBase" => "Attack",
            _ => null,
        };
        return behavior;
    }
}