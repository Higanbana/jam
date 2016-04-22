using UnityEngine;
using System.Collections;

public class CollectibleTextController : MonoBehaviour
{

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
        while (textMesh.color.a > 0)
        {
            Color color = textMesh.color;
            transform.localScale = transform.localScale * 1.2f;
            color.a = color.a - 0.1f;
            textMesh.color = color;
            yield return null;

        }
        Destroy(gameObject);
    }

    public void Setup(float angle, float speed, int orderInLayer, string letter)
    {
        transform.RotateAround(transform.position, Vector3.forward, angle);
        rigidBody.velocity = Vector3.left * speed;
        GetComponent<MeshRenderer>().sortingOrder = orderInLayer;
        textMesh.text = letter;
    }

    void Update()
    {
        Color myColor = textMesh.color;
        Color playerColor = GameManager.instance.player.GetComponent<SpriteRenderer>().color;
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
