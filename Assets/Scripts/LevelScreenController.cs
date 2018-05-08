using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;

public class LevelScreenController : MonoBehaviour {

    private Camera mainCamera;
    private LayoutElement layout;
    public Button startButton;
    public EventTrigger trigger;
    public Text title;
    public Text score;
    public int maxScore;
    public int highScore;
    public string levelID;
    public RectTransform difficultyMode;
    public RectTransform difficultyPrefab;
    public AudioClip selectionSound;
    public AudioClip mouseOverSound;
    public GameObject levelSelectCanvas;

	void Awake () {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        layout = GetComponent<LayoutElement>();
	}

	void Update () {
        layout.minWidth = mainCamera.pixelWidth;
        SetScore((int) GameManager.instance.stats.highScores.GetValue(levelID).value);
	}

    public void Init(string name, int highScore, int maxScore, int difficulty)
    {
        title.text = name.ToUpper();
        this.levelID = name;
        this.maxScore = maxScore;
        this.highScore = highScore;
        SetScore(highScore);
        startButton.onClick.AddListener(() => levelSelectCanvas.SetActive(false));
        startButton.onClick.AddListener(() => SoundManager.instance.PlaySound(selectionSound));
        startButton.onClick.AddListener(() => GameManager.instance.StartGame(name));
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => SoundManager.instance.PlaySound(mouseOverSound));
        trigger.triggers.Add(entry);

        for (int index = 0; index < difficulty; index++)
        {
            Vector3 position = Vector3.right * (index - 0.5f * (difficulty - 1f)) * difficultyPrefab.rect.width;
            RectTransform difficultyInstance = (RectTransform)Instantiate(difficultyPrefab, position, Quaternion.identity);
            difficultyInstance.SetParent(difficultyMode, false);
        }
    }

    private void SetScore(int highScore)
    {
        score.text = highScore + " / " + maxScore;
    }
}
