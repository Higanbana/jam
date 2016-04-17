using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    public Transform[] rails;
    private int currentRail = 2;
    public int maxRailNumber = 3;
    private bool collided = false;
    private bool goUp = false;
    private bool goDown = false;

	// Use this for initialization
	void Awake ()
	{
        transform.position = rails[currentRail].position;
		spriteRenderer = GetComponent<SpriteRenderer> ();
        circleCollider = GetComponent<CircleCollider2D> ();
		spriteRenderer.color = Color.black;
	}
	
	void FixedUpdate () {
	    if(!collided && spriteRenderer.color == Color.white)
        {
            Destroy(gameObject);
        }
        if (goUp)
        {
            SetRail(currentRail - 1);
            goUp = false;
        }
        else if (goDown)
        {
            SetRail(currentRail + 1);
            goDown = false;
        }
        else
        {
            collided = false;
        }
    }

	public void SwapColor ()
	{
		if (spriteRenderer.color == Color.black)
		{
			spriteRenderer.color = Color.white;
		} else {
			spriteRenderer.color = Color.black;
		}
	}

    public void Update ()
    {
        if (Input.GetButtonDown("Up"))
        {
            goUp = true;
        }
        if (Input.GetButtonDown("Down"))
        {
            goDown = true;
        }
        if (Input.GetButtonDown("Swap"))
        {
            SwapColor();
        }
    }

    void SetRail (int railIndex)
    {
        int targetRail = Mathf.Min(Mathf.Max(railIndex, (5 - maxRailNumber) / 2), 3 + (maxRailNumber - 3) / 2);
        transform.position = rails[targetRail].position;
        currentRail = targetRail;
    }

    void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Obstacle"))
        {
            collided = true;
            if (spriteRenderer.color == Color.black)
            {
                BoxCollider2D box = collider.gameObject.GetComponent<BoxCollider2D>();
                Vector2 left = transform.position + circleCollider.radius * Vector3.left;
                Vector2 right = transform.position + circleCollider.radius * Vector3.right;
                if (box.OverlapPoint(left) && box.OverlapPoint(right))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
