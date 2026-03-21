using UnityEngine;
using TMPro;

public class GUIManager : MonoBehaviour
{
    private static GUIManager instance;
    public static GUIManager Instance
    {
        get { return instance; }
    }
    // private GameObject UnitUI;
    public GameObject UnitUI;
    public TMP_InputField unitInputField;



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

        // UnitUI = GameObject.Find("UnitUI");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateUnitUI()
    {
        if (UnitUI != null)
        {
            UnitUI.SetActive(true);
        }
    }

    public void DeactivateUnitUI()
    {
        if (UnitUI != null)
        {
            UnitUI.SetActive(false);
        }
    }

    public void SendUnitsButton()
    {
        float unitsToSend = unitInputField != null ? float.Parse(unitInputField.text) : 0f;
        BaseBehavior.Instance.SendUnits(unitsToSend);
        Debug.Log("Send Units button clicked!");

        DeactivateUnitUI();
        // Close Window right after
        // You can add logic here to send units from the base to a target location
    }
}
