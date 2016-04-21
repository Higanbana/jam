using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour {

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    private float scaleFactor;

    void Awake ()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    public void Setup(float speed, Color color, int orderInLayer)
    {
        transform.GetComponent<SpriteRenderer>().color = color;
        scaleFactor = speed;
        spriteRenderer.sortingOrder = orderInLayer;
    }
	
	void Update ()
    {
        transform.localScale += Vector3.one * scaleFactor;
        if(transform.localScale.x > 10f * mainCamera.orthographicSize)
        {
            mainCamera.backgroundColor = spriteRenderer.color;
            Destroy(gameObject);
        }
    }
}
