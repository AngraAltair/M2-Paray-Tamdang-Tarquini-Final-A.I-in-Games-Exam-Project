using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIBehavior : MonoBehaviour
{
    private NavMeshAgent agent;

    [Header("AI NavMesh Config")]
    public float Health = 5f;
    public float WalkSpeed = 60f;
    public float StoppingDistance = 5f;
    private GameObject HomeBase;

    // Behavior Bools
    public bool IsPerformingTask { get; set; }
    public bool IsGathering { get; set; }
    private bool IsGatheringTask { get; set; }

    [Header("Gathering Settings")]
    private Coroutine GatheringCoroutine;
    public float GatheringRate = 6f;
    public float GatheringDurationInSeconds = 3f;

    // Start is called before the first frame update
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = WalkSpeed;
        agent.stoppingDistance = StoppingDistance;
    }

    void Start() {
        HomeBase = GameObject.Find("Base (DEBUG)");
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("AI Performing Task: " + IsPerformingTask);


        if (IsGatheringTask && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            Debug.Log("AI arrived at destination. Starting gathering.");
            IsGathering = true;
            IsGatheringTask = false; // Prevents re-triggering

            if (GatheringCoroutine != null)
            {
                StopCoroutine(GatherResourcesCoroutine());
            }
            GatheringCoroutine = StartCoroutine(GatherResourcesCoroutine());
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

    public void GatherResources(Vector3 targetPosition)
    {
        agent.destination = targetPosition;
        IsGatheringTask = true;
        NavigateToTarget(targetPosition);
        Debug.Log("Starting to gather resources at: " + targetPosition);
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
        if (HomeBase != null)
            NavigateToTarget(HomeBase.transform.position);
    }
}
