using UnityEngine;

public class ClickManager : MonoBehaviour
{
    private static ClickManager instance;
    public static ClickManager Instance
    {
        get { return instance; }
    }
    public Camera mainCamera;
    private string LastClickedObjectTag;
    private Vector3 lastClickedPosition;
    private GameObject LastClickedObject;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Last Clicked Position (of important item): " + lastClickedPosition);
        
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the clicked object has the "Clickable" tag DEBUG PURPOSES
                if (hit.collider.CompareTag("Clickable"))
                {
                    // Call a method on the clicked object
                    Debug.Log(hit.collider.name + " was clicked!");
                    // hit.collider.gameObject.SendMessage("OnClicked", SendMessageOptions.DontRequireReceiver);
                }

                if (hit.collider.CompareTag("ResourceArea"))
                {
                    // Set the last clicked object tag and position for the BaseBehavior to reference when sending units. This is important for dictating their behavior.
                    LastClickedObject = hit.collider.gameObject;
                    LastClickedObjectTag = hit.collider.tag;
                    lastClickedPosition = hit.transform.position;
                    Debug.Log("Resource area clicked: " + hit.collider.name);
                    GUIManager.Instance.ActivateUnitUI();
                    // You can add logic here to gather resources or interact with the resource area
                }

                if (hit.collider.CompareTag("EnemyBase"))
                {
                    LastClickedObject = hit.collider.gameObject;
                    LastClickedObjectTag = hit.collider.tag;
                    lastClickedPosition = hit.transform.position;
                    Debug.Log("Enemy base clicked: " + hit.collider.name);
                    GUIManager.Instance.ActivateUnitUI();
                }
            }
        }
    }

    public string GetLastClickedObjectTag()
    {
        return LastClickedObjectTag;
    }

    public Vector3 GetLastClickedPosition()
    {
        return lastClickedPosition;
    }

    public GameObject GetLastClickedObject()
    {
        return LastClickedObject;
    }
}
