using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShiftScreenController : MonoBehaviour {

    private ScrollRect scrollRect;

    public GameObject levelSelectorCanvas;
    public RectTransform levelSelectorPanel;
    public RectTransform levelIconsPanel;
    public RectTransform levelIconPrefab;
    public LevelScreenController levelScreenPrefab;

    private Camera mainCamera;

    private int screenCount;

    public float fallbackIntensity = 0.1f;
    public float snapThreshold = 0.1f;

    void Awake ()
    {
        scrollRect = GetComponent<ScrollRect>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        screenCount = levelSelectorPanel.childCount;
    }
    
    public void AddLevel(Level level)
    {
        LevelScreenController levelScreen = (LevelScreenController)Instantiate(levelScreenPrefab, Vector3.zero, Quaternion.identity);
        levelScreen.levelSelectCanvas = levelSelectorCanvas;
        levelScreen.Init(level.name, 0, level.maxScore, level.difficulty);
        levelScreen.gameObject.transform.SetParent(levelSelectorPanel, false);

        RectTransform levelIcon = (RectTransform)Instantiate(levelIconPrefab, Vector3.zero, Quaternion.identity);
        levelIcon.position = new Vector3(levelIcon.rect.width * levelIconsPanel.childCount, 0f, 0f);
        levelIcon.SetParent(levelIconsPanel, false);
        levelIconsPanel.offsetMin = new Vector2(-0.5f * levelIcon.rect.width * levelIconsPanel.childCount, 0f);
    }

    public void Increment()
    {
        levelSelectorPanel.position += Vector3.left * mainCamera.pixelWidth;
    }

    public void Decrement()
    {
        levelSelectorPanel.position += Vector3.right * mainCamera.pixelWidth;
    }

    void Update ()
    {
        float target = Mathf.Min(Mathf.Max(Mathf.RoundToInt(levelSelectorPanel.position.x / mainCamera.pixelWidth), 1 - screenCount), 0) * mainCamera.pixelWidth;
        if(Mathf.Abs(target - levelSelectorPanel.position.x) < snapThreshold * mainCamera.pixelWidth)
        {
            Vector3 panelPosition = levelSelectorPanel.position;
            panelPosition.x = target;
            levelSelectorPanel.position = panelPosition;
            scrollRect.velocity = Vector2.zero;
        }
        else
        {
            scrollRect.velocity += Vector2.left * fallbackIntensity * (levelSelectorPanel.position.x - target);
        }
    }
}
