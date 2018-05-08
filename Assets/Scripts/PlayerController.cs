using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour {

	private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;

    public Camera mainCamera;

    public Transform[] rails;
    private int currentRail;
    public int maxRailNumber = 3;

    [HideInInspector]
    public bool touchTrigger = false;
    private List<GameObject> colliders;

    private bool goUp = false;
    private bool goDown = false;

    public AudioClip dieSound;

	public int segments;
	private float radius = 1;
	private float alpha = 1f;
	public float radiusIncrement;
	public float pulseInterval;
	private float pulseCooldown = 0f;
    private bool deathEnabled = false;
	LineRenderer line;

    public void EnableDeath(bool enable)
    {
        deathEnabled = enable;
    }

	void OnEnable ()
	{
        currentRail = 2;
        transform.position = rails[currentRail].position;
		spriteRenderer = GetComponent<SpriteRenderer> ();
		circleCollider = GetComponent<CircleCollider2D> ();
		spriteRenderer.color = Color.black;

        colliders = new List<GameObject>();
        touchTrigger = false;

        line = GetComponent<LineRenderer>();
		line.SetVertexCount (segments + 1);
		line.useWorldSpace = false;
        line.sortingLayerName = "Player";
		DrawCircle ();
	}
	
    GameObject FindTopCollider ()
    {
        colliders.RemoveAll(item => item == null);

        if (colliders.Count == 0)
        {
            return null;
        }
        else
        {
            GameObject topCollider = colliders[0];
            for (int colliderIndex = 0; colliderIndex < colliders.Count; colliderIndex++)
            {
                SpriteRenderer topRenderer = topCollider.GetComponent<SpriteRenderer>();
                SpriteRenderer newRenderer = colliders[colliderIndex].GetComponent<SpriteRenderer>();
                int newSortingValue = SortingLayer.GetLayerValueFromID(newRenderer.sortingLayerID);
                int topSortingValue = SortingLayer.GetLayerValueFromID(topRenderer.sortingLayerID);
                if (newSortingValue > topSortingValue)
                {
                    topCollider = colliders[colliderIndex];
                }
                else
                {
                    int newOrderInLayer = newRenderer.sortingOrder;
                    int topOrderInLayer = topRenderer.sortingOrder;
                    if (newOrderInLayer > topOrderInLayer)
                    {
                        topCollider = colliders[colliderIndex];
                    }
                }
            }
            return topCollider;
        }
    }

	void FixedUpdate ()
    {
        GameObject topCollider = FindTopCollider();
        if (topCollider && spriteRenderer.color == topCollider.gameObject.GetComponent<SpriteRenderer>().color)
        {
            Collider2D collider2D = topCollider.gameObject.GetComponent<Collider2D>();
            Vector3 direction = (transform.position - topCollider.gameObject.transform.position).normalized;
            Vector2 far = transform.position + circleCollider.radius * direction;
            Vector2 near = transform.position - circleCollider.radius * direction;
            if (collider2D.OverlapPoint(far) && collider2D.OverlapPoint(near) && deathEnabled)
            {
                PlayerDie();
            }
        }

        if (!topCollider && spriteRenderer.color == mainCamera.backgroundColor && deathEnabled)
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
    }

    Color GetOppositeColor(Color color)
    {
        if (color == Color.black)
        {
            return Color.white;
        }
        else if(color == Color.white)
        {
            return Color.black;
        } else
        {
            return color;
        }
    }

	public void SwapColor ()
	{
        if (deathEnabled) { GameManager.instance.stats.colorChange.Increment(); }
        spriteRenderer.color = GetOppositeColor(spriteRenderer.color);

        if (touchTrigger || !deathEnabled)
        {
            mainCamera.backgroundColor = GetOppositeColor(mainCamera.backgroundColor);
        }
	}

    public void Update ()
    {
        if (!GameManager.instance.IsPaused())
        {
#if UNITY_STANDALONE || UNITY_WEBPLAYER
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
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            if (Input.touchCount > 0)
            {
                Touch touch = Input.touches[0];
                if (touch.phase == TouchPhase.Began)
                {
                    Vector2 position = touch.position;
                    if(position.x < 0.5f * mainCamera.pixelWidth)
                    {
                        if(position.y < 0.5f * mainCamera.pixelHeight)
                        {
                            goDown = true;
                        }
                        else
                        {
                            goUp = true;
                        }
                    }
                    else
                    {
                        SwapColor();
                    }
                }
            }
#endif
        }

        DrawCircle ();
    }

    void SetRail (int railIndex)
    {
        int targetRail = Mathf.Min(Mathf.Max(railIndex, (5 - maxRailNumber) / 2), 3 + (maxRailNumber - 3) / 2);
        transform.position = rails[targetRail].position;
        currentRail = targetRail;
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Obstacle"))
        {
            colliders.Add(collider.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("Obstacle"))
        {
            colliders.Remove(collider.gameObject);
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
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * radius;
			y = Mathf.Cos (Mathf.Deg2Rad * angle) * radius;
			line.SetPosition (i,new Vector3(x,y,z));
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
