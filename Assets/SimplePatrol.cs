using UnityEngine;
 
public class SimplePatrol : MonoBehaviour
{
    public float speed = 5.0f; // Adjust the speed of movement
    public float switchDirectionTime = 20.0f; // Time to switch direction
 
    private float timer = 0.0f;
 
    void Update()
    {
        timer += Time.deltaTime;
 
        if (timer >= switchDirectionTime)
        {
            transform.Rotate(Vector3.up, 180f);
            timer = 0.0f;
        }
 
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}