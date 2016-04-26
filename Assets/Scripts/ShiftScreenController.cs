using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShiftScreenController : MonoBehaviour {

    public RectTransform panel;
    private ScrollRect scrollRect;
    private Camera mainCamera;
    private int screenCount;
    public float fallbackIntensity = 0.1f;
    public float snapThreshold = 0.1f;

    void Awake ()
    {
        scrollRect = GetComponent<ScrollRect>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        screenCount = panel.childCount;
    }
    
    public void Increment()
    {
        panel.position += Vector3.left * mainCamera.pixelWidth;
    }

    public void Decrement()
    {
        panel.position += Vector3.right * mainCamera.pixelWidth;
    }

    void Update ()
    {
        float target = Mathf.Min(Mathf.Max(Mathf.RoundToInt(panel.position.x / mainCamera.pixelWidth), 1 - screenCount), 0) * mainCamera.pixelWidth;
        if(Mathf.Abs(target - panel.position.x) < snapThreshold * mainCamera.pixelWidth)
        {
            Vector3 panelPosition = panel.position;
            panelPosition.x = target;
            panel.position = panelPosition;
            scrollRect.velocity = Vector2.zero;
        }
        else
        {
            scrollRect.velocity += Vector2.left * fallbackIntensity * (panel.position.x - target);
        }
    }
}
