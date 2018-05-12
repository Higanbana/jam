using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayerController : MonoBehaviour {

    [Header("Camera")]

    public Camera mainCamera;

    [Header("Rails")]

    public Transform[] rails;
    private int currentRail;
    public int maxRailNumber = 3;

    [Header("UI")]

    public RectTransform upArrow;
    public RectTransform downArrow;
    public RectTransform swap;

    [HideInInspector]
    public bool touchTrigger = false;
    private List<GameObject> colliders;

    private bool goUp = false;
    private bool goDown = false;

    [Header("Sound")]

    public AudioClip dieSound;

    [Header("Pulse")]

    public int segments;
	private float radius = 1;
	private float alpha = 1f;
	public float radiusIncrement;
	public float pulseInterval;
	private float pulseCooldown = 0f;

    [Header("Warning")]
    public int warningDuration = 10;
    public float warningAmplitude = 0.2f;
    private int warningCount = 0;

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private LineRenderer lineRenderer;

    private bool deathEnabled = false;

    [HideInInspector]
    public bool colorInverted = false;

    private int safeFrames = 1;

    public void EnableDeath(bool enable)
    {
        deathEnabled = enable;
    }

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetVertexCount(segments + 1);
    }

	void OnEnable ()
	{
        currentRail = 2;
        transform.position = rails[currentRail].position;
		spriteRenderer.color = Color.black;

        colliders = new List<GameObject>();
        touchTrigger = false;

		DrawPulseCircle();

#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        upArrow.gameObject.SetActive(true);
        downArrow.gameObject.SetActive(true);
        swap.gameObject.SetActive(true);
#endif
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

    void SetWarning(bool state)
    {
        if (!state)
        {
            spriteRenderer.transform.localScale = Vector3.one;
            warningCount = 0;
        }
        else
        {
            warningCount = (warningCount + 1) % warningDuration;
            spriteRenderer.transform.localScale = Vector3.one * (1f - warningAmplitude * (float)Math.Sin(2f * Math.PI * (float)warningCount/(float)warningDuration));
        }
    }

	void FixedUpdate ()
    {
        if (deathEnabled)
        {
            GameObject topCollider = FindTopCollider();
            if (topCollider)
            {
                Collider2D collider2D = topCollider.gameObject.GetComponent<Collider2D>();
                Vector2 rightEndPoint = transform.position + circleCollider.radius * Vector3.right;
                Vector2 leftEndPoint = transform.position - circleCollider.radius * Vector3.right;
                bool rightEndPointCovered = collider2D.OverlapPoint(rightEndPoint);
                bool leftEndPointCovered = collider2D.OverlapPoint(leftEndPoint);
                Color playerColor = spriteRenderer.color;
                Color colliderColor = topCollider.GetComponent<SpriteRenderer>().color;
                if (leftEndPointCovered && rightEndPointCovered && playerColor == colliderColor)
                {
                    PlayerDie();
                }
                else if (rightEndPointCovered && !leftEndPointCovered && playerColor == colliderColor)
                {
                    SetWarning(true);
                }
                else if (!rightEndPointCovered && leftEndPointCovered && playerColor != colliderColor)
                {
                    SetWarning(true);
                }
                else
                {
                    SetWarning(false);
                }
            }
            else
            {
                SetWarning(false);
            }

            if (!topCollider && spriteRenderer.color == mainCamera.backgroundColor && deathEnabled)
            {
                PlayerDie();
            }
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
            InvertBackgroundColor();
            colorInverted = !colorInverted;
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
                    if (upArrow.rect.Contains(position))
                    {
                        goUp = true;
                    }
                    else if (downArrow.rect.Contains(position))
                    {
                        goDown = true;
                    }
                    else if (swap.rect.Contains(position))
                    {
                        SwapColor();
                    }
                }
            }
#endif
        }

        DrawPulseCircle ();
    }

    public void SetRail (int railIndex)
    {
        int targetRail = Mathf.Min(Mathf.Max(railIndex, (5 - maxRailNumber) / 2), 3 + (maxRailNumber - 3) / 2);
        transform.position = rails[targetRail].position;
        currentRail = targetRail;
    }

    public int GetRail()
    {
        return currentRail;
    }

    public Color GetColor()
    {
        return spriteRenderer.color;
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
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
        if (safeFrames <= 0)
        {
            gameObject.SetActive(false);
            SoundManager.instance.PlaySound(dieSound);
            GameManager.instance.GameOver();
            safeFrames = 1;
        } else
        {
            safeFrames--;
        }
	}

	void DrawPulseCircle()
    {
        float z = -2f;
        Color c = spriteRenderer.color;
        alpha -= radiusIncrement / 2.5f;
        radius += radiusIncrement;

        DrawCircle(radius, alpha, z, c, segments, lineRenderer);

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

    private void DrawCircle(float circleRadius, float alpha, float z, Color c, int segments, LineRenderer renderer)
    {
        float angle = 0f;
        float x;
        float y;
        for (int i = 0; i < (segments + 1); i++)
        {
            x = Mathf.Sin(Mathf.Deg2Rad * angle) * circleRadius;
            y = Mathf.Cos(Mathf.Deg2Rad * angle) * circleRadius;
            renderer.SetPosition(i, new Vector3(x, y, z));
            c.a = alpha;
            renderer.SetColors(c, c);  
            angle += 360.2f/segments;
        }

    }

    internal void RestoreInvertedColor(bool colorInverted)
    {
        this.colorInverted = colorInverted;
        if (colorInverted) {
            InvertBackgroundColor();
        }
    }

    private void InvertBackgroundColor()
    {
        mainCamera.backgroundColor = GetOppositeColor(mainCamera.backgroundColor);
    }

    public CircleCollider2D Collider
    {
        get { return circleCollider; }
    }

    public SpriteRenderer Renderer
    {
        get { return spriteRenderer; }
    }
}
