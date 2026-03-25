using UnityEngine;

public class Enemy : MonoBehaviour
{
    private float unitHealth;
    public float unitMaxHealth;
    public int goldReward = 10;

    public HealthTracker healthTracker;


    void Start()
    {
        // UnitSelectionManager.Instance.allUnitsList.Add(gameObject);

        unitHealth = unitMaxHealth;
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        // UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
        ResourceManager.Instance.AddMoney(goldReward);
    }

    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(unitHealth, unitMaxHealth);

        if (unitHealth <= 0)
        {

            Destroy(gameObject);
        }
    }

    internal void TakeDamage(int damageToInflict)
    {
        unitHealth -= damageToInflict;
        UpdateHealthUI();
    }
}
