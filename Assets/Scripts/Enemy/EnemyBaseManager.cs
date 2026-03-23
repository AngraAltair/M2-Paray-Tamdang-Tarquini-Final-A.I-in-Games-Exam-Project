using UnityEngine;

public class EnemyBaseManager : MonoBehaviour
{
    public float Health {get;set;} = 10f;
    public bool IsBaseAlive {get;set;} = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Health <= 0)
        {
            IsBaseAlive = false;
            Destroy(gameObject);
        }
    }

    public void ReduceHealth(float AttackDamage)
    {
        Debug.Log("Owie!");
        Health -= AttackDamage;
    }
}
