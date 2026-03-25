using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    private float unitHealth;
    public float unitMaxHealth;
    public int goldReward = 10;

    public HealthTracker healthTracker;
    private AudioSource audioSource;


    void Start()
    {
        // UnitSelectionManager.Instance.allUnitsList.Add(gameObject);

        unitHealth = unitMaxHealth;
        audioSource = GetComponent<AudioSource>();
        UpdateHealthUI();
    }

    private void OnDestroy()
    {
        // UnitSelectionManager.Instance.allUnitsList.Remove(gameObject);
    }

    private void UpdateHealthUI()
    {
        healthTracker.UpdateSliderValue(unitHealth, unitMaxHealth);

        if (unitHealth <= 0)
        {
            ResourceManager.Instance.EnemyDefeated(goldReward);
            Destroy(gameObject);
        }
    }

    internal void TakeDamage(int damageToInflict)
    {
        unitHealth -= damageToInflict;
        UpdateHealthUI();
    }
}
