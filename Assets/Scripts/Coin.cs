using UnityEngine;

public class Coin : MonoBehaviour
{
    public int goldWorth = 10;

    [Range(0f, 1f)]
    public float coinSoundVolume = 0.5f;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance?.PlayCoinCollectSound(coinSoundVolume);
            ResourceManager.Instance.CoinCollected(goldWorth);
            Destroy(gameObject);
        }
    }
}