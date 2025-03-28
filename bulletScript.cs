using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public int damage;
    public int speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(Vector2.right * Time.deltaTime * speed);
    }

     
    void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log("Hit something");
        if (other.gameObject.CompareTag("Wall"))
        {
            Cube wall = other.gameObject.GetComponent<Cube>();
            wall.TakeDamage(damage);
        }
        // Destroy the bullet
        Destroy(gameObject);
    }
}
