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

    public Color defaultColor;
    public Color highlightColor;

    private Camera mainCamera;

    private int levelCount;
    private int levelIndex;

    public float fallbackIntensity = 0.1f;
    public float snapThreshold = 0.1f;

    void Awake ()
    {
        scrollRect = GetComponent<ScrollRect>();
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        levelIndex = 0;
        levelIconsPanel.GetChild(levelIndex).GetComponentInChildren<Image>().color = highlightColor;
    }
    
    public void AddLevel(Level level)
    {
        LevelScreenController levelScreen = (LevelScreenController)Instantiate(levelScreenPrefab, Vector3.zero, Quaternion.identity);
        levelScreen.levelSelectCanvas = levelSelectorCanvas;
        levelScreen.Init(level.name, (int) GameManager.instance.stats.highScores.GetValue(level.name).value, level.maxScore, level.difficulty);
        levelScreen.gameObject.transform.SetParent(levelSelectorPanel, false);

        RectTransform levelIcon = (RectTransform)Instantiate(levelIconPrefab, Vector3.zero, Quaternion.identity);
        levelIcon.SetParent(levelIconsPanel, false);
        levelIcon.localPosition = new Vector3(levelIcon.rect.width * (levelIconsPanel.childCount - 0.5f), 0f, 0f);
        levelIcon.GetComponentInChildren<Image>().color = defaultColor;
        levelIconsPanel.anchoredPosition = new Vector2(-0.5f * levelIcon.rect.width * levelIconsPanel.childCount, 0f);

        levelCount++;
    }

    public void Increment()
    {
        levelSelectorPanel.position += Vector3.left * mainCamera.pixelWidth;
        HighlightSelectedLevel();
    }

    public void Decrement()
    {
        levelSelectorPanel.position += Vector3.right * mainCamera.pixelWidth;
        HighlightSelectedLevel();
    }

    int CurrentLevelIndex
    {
        get
        {
            return Mathf.Min(Mathf.Max(Mathf.RoundToInt( - levelSelectorPanel.position.x / mainCamera.pixelWidth), 0), levelCount - 1);
        }
    }

    void HighlightSelectedLevel()
    {
        int oldLevelIndex = levelIndex;
        int newLevelIndex = CurrentLevelIndex;
        if (oldLevelIndex != newLevelIndex)
        {
            levelIconsPanel.GetChild(oldLevelIndex).GetComponentInChildren<Image>().color = defaultColor;
            levelIconsPanel.GetChild(newLevelIndex).GetComponentInChildren<Image>().color = highlightColor;
            levelIndex = newLevelIndex;
        }
    }

    void Update ()
    {
        float target = - CurrentLevelIndex * mainCamera.pixelWidth;
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
