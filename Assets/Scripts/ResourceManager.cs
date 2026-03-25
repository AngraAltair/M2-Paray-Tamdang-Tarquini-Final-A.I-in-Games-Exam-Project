using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; set; }

    public int playerGold;
    public TMP_Text resourceText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        resourceText.SetText(playerGold.ToString());
    }

    public void AddMoney(int goldAmount)
    {
        playerGold += goldAmount;
        resourceText.SetText(playerGold.ToString());
    }

    public void SubtractMoney(int goldAmount)
    {
        playerGold -= goldAmount;
        resourceText.SetText(playerGold.ToString());
    }
}
