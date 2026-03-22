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
    // Public References
    [Header("Unit UI References")]
    public GameObject UnitUI;
    public TMP_InputField unitInputField;

    [Header("Resource UI References")]
    public GameObject ResourceUI;
    public TMP_Text UnitCount;
    public TMP_Text WoodCount;
    public TMP_Text SteelCount;
    public TMP_Text FoodCount;

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
        SetResourceCountText();
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
        int unitsToSend = unitInputField != null ? int.Parse(unitInputField.text) : 0;
        BaseBehavior.Instance.SendUnits(unitsToSend);
        Debug.Log("Send Units button clicked!");

        DeactivateUnitUI();
        // Close Window right after
        // You can add logic here to send units from the base to a target location
    }

    public void SetResourceCountText()
    {
        UnitCount.SetText(BaseBehavior.Instance.BaseUnits.ToString());
        WoodCount.SetText(BaseBehavior.Instance.WoodResourceCount.ToString());
        SteelCount.SetText(BaseBehavior.Instance.SteelResourceCount.ToString());
        FoodCount.SetText(BaseBehavior.Instance.FoodResourceCount.ToString());
    }
}
