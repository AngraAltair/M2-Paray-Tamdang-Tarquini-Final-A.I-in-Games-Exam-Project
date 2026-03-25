using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Enemy : MonoBehaviour
{
    private float unitHealth;
    public float unitMaxHealth;
    public int goldReward = 10;

    public HealthTracker healthTracker;
    private AudioSource audioSource;

    [Range(0f, 1f)]
    public float deathSoundVolume = 0.5f;

    void Start()
    {
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
            AudioManager.Instance?.PlayEnemyDeathSound(deathSoundVolume);
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