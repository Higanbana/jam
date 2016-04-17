using UnityEngine;
using System.Collections;

public class ObstacleController : MonoBehaviour {
    Rigidbody2D rb;
    
    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(float height, float length, Vector2 position, float speed)
    {
        transform.position = position;
        Vector3 scale = transform.localScale;
        scale.x = length;
        scale.y = height;
        transform.localScale = scale;
        rb.velocity = Vector3.left * speed;
    }


}
