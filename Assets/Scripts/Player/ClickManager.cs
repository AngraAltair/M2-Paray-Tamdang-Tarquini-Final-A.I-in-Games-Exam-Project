using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public Camera mainCamera;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // Check if the clicked object has the "Clickable" tag
                if (hit.collider.CompareTag("Clickable"))
                {
                    // Call a method on the clicked object
                    Debug.Log(hit.collider.name + " was clicked!");
                    // hit.collider.gameObject.SendMessage("OnClicked", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
