using UnityEngine;
using System.Collections;

public class ObstacleController : MonoBehaviour {
    Rigidbody2D rigidBody;
    SpriteRenderer spriteRenderer;
    
    public void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D> ();
        spriteRenderer = GetComponent<SpriteRenderer> ();
    }

    public void Setup(float height, float length, float speed, Color color)
    {
        Vector3 scale = transform.localScale;
        scale.x = length;
        scale.y = height;
        transform.localScale = scale;
        rigidBody.velocity = Vector3.left * speed;
        spriteRenderer.color = color;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Deleter"))
        {
            Destroy(gameObject);
        }
    }
}
