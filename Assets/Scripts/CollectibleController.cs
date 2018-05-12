using UnityEngine;
using System.Collections;

public class CollectibleController : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

	void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        rigidBody = GetComponent<Rigidbody2D> ();
	}

    IEnumerator DeathAnimation ()
    {
        while (spriteRenderer.color.a > 0)
        {
            Color color = spriteRenderer.color ;
            transform.localScale = transform.localScale * 1.2f;
            color.a = color.a - 0.1f;
            spriteRenderer.color = color;          
            yield return null;

        }
        Destroy(gameObject);
    }

    public void Setup(float angle, float speed, int orderInLayer)
    {
        transform.RotateAround(transform.position, Vector3.forward, angle - 90.0f);
        rigidBody.velocity = Vector3.left * speed;
        spriteRenderer.sortingOrder = orderInLayer;
    }

    void Update ()
    {
        Color myColor = spriteRenderer.color;
        Color playerColor = GameManager.instance.player.Renderer.color;
        myColor.r = playerColor.r;
        myColor.g = playerColor.g;
        myColor.b = playerColor.b;
        spriteRenderer.color = myColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(spriteRenderer.color == Color.black)
            {
                GameManager.instance.BlackCollected();
            }
            else
            {
                GameManager.instance.WhiteCollected();
            }
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(DeathAnimation());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Deleter"))
        {
            Destroy(gameObject);
        }
    }
}
