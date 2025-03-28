using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cube : MonoBehaviour
{
    public float life = 100f;
    public Sprite halfDestroyed; // Add a public variable for the new sprite
    public Sprite almostDestroyed;
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component
    }

    // Update is called once per frame
    void Update()
    {
        if (life < 75f && life > 25f) // Check if life is below the threshold
        {
            spriteRenderer.sprite = halfDestroyed; // Change the sprite
        }
        else if (life <= 25f){
            spriteRenderer.sprite = almostDestroyed;
        }
    }

    public void TakeDamage(float damage)
    {
        life -= damage;
        if (life <= 0)
        {
            Destroy(gameObject);
        }
    }
}
