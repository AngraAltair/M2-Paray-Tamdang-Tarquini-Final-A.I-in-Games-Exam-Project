using UnityEngine;

public class Coin : MonoBehaviour
{
    public int goldWorth = 10;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ResourceManager.Instance.AddMoney(goldWorth);
            Destroy(gameObject);
        }
    }

}
