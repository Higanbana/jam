using UnityEngine;
using System.Collections;

public class ObstacleController : MonoBehaviour {
    Rigidbody2D rb;
    
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float height, float length, float speed)
    {
        Vector3 scale = transform.localScale;
        scale.x = length;
        scale.y = height;
        transform.localScale = scale;
        rb.velocity = Vector3.left * speed;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Deleter"))
        {
            Destroy(gameObject);
        }
    }
}
