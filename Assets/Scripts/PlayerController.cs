using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    public Camera mainCamera;

    public Transform[] rails;
    private int currentRail;
    public int maxRailNumber = 3;

    [HideInInspector]
    public bool touchTrigger = false;
    private bool touchObstacle = false;

    private bool goUp = false;
    private bool goDown = false;
    public AudioClip dieSound;

	public int segments;
	private float radius = 1;
	private float alpha = 1f;
	public float radiusIncrement;
	public float pulseInterval;
	private float pulseCooldown = 0f;
	LineRenderer line;

	void OnEnable ()
	{
        currentRail = 2;
        transform.position = rails[currentRail].position;
		spriteRenderer = GetComponent<SpriteRenderer> ();
		circleCollider = GetComponent<CircleCollider2D> ();
		spriteRenderer.color = Color.black;

        line = GetComponent<LineRenderer>();
		line.SetVertexCount (segments + 1);
		line.useWorldSpace = false;
		DrawCircle ();
	}
	
	void FixedUpdate () {
	    if(!touchObstacle && spriteRenderer.color == mainCamera.backgroundColor)
        {
            PlayerDie();
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
            touchObstacle = false;
        }
    }

    Color GetOppositeColor(Color color)
    {
        if (color == Color.black)
        {
            return Color.white;
        }
        else
        {
            return Color.black;
        }
    }

	public void SwapColor ()
	{
        spriteRenderer.color = GetOppositeColor(spriteRenderer.color);

        if (touchTrigger)
        {
            mainCamera.backgroundColor = GetOppositeColor(mainCamera.backgroundColor);
        }
	}

    public void Update ()
    {
        if (!GameManager.instance.IsPaused())
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
        
		DrawCircle ();
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
            touchObstacle = true;
            if (spriteRenderer.color == collider.gameObject.GetComponent<SpriteRenderer>().color)
            {
                BoxCollider2D box = collider.gameObject.GetComponent<BoxCollider2D>();
                Vector2 left = transform.position + circleCollider.radius * Vector3.left;
                Vector2 right = transform.position + circleCollider.radius * Vector3.right;
                if (box.OverlapPoint(left) && box.OverlapPoint(right))
                {
                    PlayerDie();
                }
            }
        }
    }

	void PlayerDie()
	{
		gameObject.SetActive(false);
		SoundManager.instance.PlaySound(dieSound);
		GameManager.instance.GameOver();
	}

	void DrawCircle()
	{
		float x;
		float y;
		float z = -1f;

		Color c = spriteRenderer.color;
		float angle = 0f;
		alpha -= radiusIncrement/2.5f;
		radius += radiusIncrement;

		for (int i = 0; i < (segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle);
			y = Mathf.Cos (Mathf.Deg2Rad * angle);
			line.SetPosition (i,new Vector3(x,y,z) * radius);
			c.a = alpha;
			line.SetColors (c, c);
			angle += (360.25f / segments);
		}

		if (pulseCooldown <= 0)
		{
			radius = 1;
			alpha = 1;
			pulseCooldown = pulseInterval;
		}
		else
		{
			pulseCooldown -= Time.deltaTime;
		}
	}

}
