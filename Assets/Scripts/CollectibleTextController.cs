using UnityEngine;
using System.Collections;

public class CollectibleTextController : MonoBehaviour
{
    private const float DeathAnimationDuration = 0.2f;
    private const float DeathAnimationScaleMultiplier = 6f;

    private TextMesh textMesh;
    private Rigidbody2D rigidBody;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        rigidBody = GetComponent<Rigidbody2D>();
        GetComponent<MeshRenderer>().sortingLayerName = "Collectible";
    }

    IEnumerator DeathAnimation()
    {
        Color startColor = textMesh.color;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = startScale * DeathAnimationScaleMultiplier;
        float elapsed = 0f;

        while (elapsed < DeathAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / DeathAnimationDuration);

            Color color = startColor;
            color.a = 1f - t;
            textMesh.color = color;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        Destroy(gameObject);
    }

    public void Setup(float angle, float speed, int orderInLayer, string letter)
    {
        transform.RotateAround(transform.position, Vector3.forward, angle);
        rigidBody.linearVelocity = Vector3.left * speed;
        GetComponent<MeshRenderer>().sortingOrder = orderInLayer;
        textMesh.text = letter;
    }

    void Update()
    {
        Color myColor = textMesh.color;
        Color playerColor = GameManager.instance.player.Renderer.color;
        myColor.r = playerColor.r;
        myColor.g = playerColor.g;
        myColor.b = playerColor.b;
        textMesh.color = myColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (textMesh.color == Color.black)
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
