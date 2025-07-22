using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnCollisionEnter(Collision collision)
    {
        //debug
        print("Bullet collided with: " + collision.gameObject.name);
        if (collision.gameObject.CompareTag("Zombie"))
        {
            print("Target hit!" + collision.gameObject.name);
            Destroy(gameObject);
        }
    }
}
