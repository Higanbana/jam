using UnityEngine;
using System.Collections;

public class CollectibleController : MonoBehaviour {
    private const float DeathAnimationDuration = 0.2f;
    private const float DeathAnimationScaleMultiplier = 6f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

	void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer> ();
        rigidBody = GetComponent<Rigidbody2D> ();
	}

    IEnumerator DeathAnimation ()
    {
        Color startColor = spriteRenderer.color;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * DeathAnimationScaleMultiplier;
        float elapsed = 0f;

        while (elapsed < DeathAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / DeathAnimationDuration);

            Color color = startColor;
            color.a = 1f - t;
            spriteRenderer.color = color;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void Setup(float angle, float speed, int orderInLayer)
    {
        transform.RotateAround(transform.position, Vector3.forward, angle - 90.0f);
        rigidBody.linearVelocity = Vector3.left * speed;
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
