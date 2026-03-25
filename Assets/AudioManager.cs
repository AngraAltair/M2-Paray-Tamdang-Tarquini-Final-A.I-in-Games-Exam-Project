using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;
    private AudioClip enemyDeathSound;
    private AudioClip coinCollectSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        enemyDeathSound = Resources.Load<AudioClip>("grunt-kill");
        coinCollectSound = Resources.Load<AudioClip>("limbus-company-coin");

        if (enemyDeathSound == null)
        {
            Debug.LogWarning("AudioManager: Could not load 'grunt-kill' from Resources.");
        }
        if (coinCollectSound == null)
        {
            Debug.LogWarning("AudioManager: Could not load 'limbus-company-coin' from Resources.");
        }
    }

    public void PlayEnemyDeathSound(float volume = 1f)
    {
        if (enemyDeathSound != null)
        {
            audioSource.PlayOneShot(enemyDeathSound, volume);
        }
    }

    public void PlayCoinCollectSound(float volume = 1f)
    {
        if (coinCollectSound != null)
        {
            audioSource.PlayOneShot(coinCollectSound, volume);
        }
    }
}