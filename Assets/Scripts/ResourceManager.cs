using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; set; }

    public int playerGold;
    public TMP_Text resourceText;

    private int activeCoins;
    private int activeEnemies;

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
        activeCoins = FindObjectsOfType<Coin>().Length;
        activeEnemies = FindObjectsOfType<Enemy>().Length;
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

    public void CoinCollected(int goldAmount)
    {
        AddMoney(goldAmount);
        activeCoins--;
        CheckLevelComplete();
    }

    public void EnemyDefeated(int goldReward)
    {
        AddMoney(goldReward);
        activeEnemies--;
        CheckLevelComplete();
    }

    private void CheckLevelComplete()
    {
        if (activeCoins <= 0 && activeEnemies <= 0)
        {
            string currentScene = SceneManager.GetActiveScene().name;

            switch (currentScene)
            {
                case "Level1":
                    SceneManager.LoadScene("Level2");
                    break;
                case "Level2":
                    SceneManager.LoadScene("Level3");
                    break;
                case "Level3":
                    SceneManager.LoadScene("Win");
                    break;
                default:
                    Debug.LogWarning($"Unknown scene '{currentScene}' reached end-of-level, loading Level1 as fallback.");
                    SceneManager.LoadScene("Level1");
                    break;
            }
        }
    }
}
