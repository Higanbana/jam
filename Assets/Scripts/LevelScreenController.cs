using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class LevelScreenController : MonoBehaviour {

    private Camera mainCamera;
    private LayoutElement layout;
    public Button startButton;
    public Text title;
    public Text score;
    public RectTransform difficultyMode;
    public RectTransform difficultyPrefab;
    public AudioClip selectionSound;
    public GameObject levelSelectCanvas;

	void Awake () {
        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        layout = GetComponent<LayoutElement>();
	}

	void Update () {
        layout.minWidth = mainCamera.pixelWidth;
	}

    public void Init(string name, int highScore, int maxScore, int difficulty)
    {
        title.text = name.ToUpper();
        score.text = highScore + " / " + maxScore;
        startButton.onClick.AddListener(() => levelSelectCanvas.SetActive(false));
        startButton.onClick.AddListener(() => SoundManager.instance.PlaySound(selectionSound));
        startButton.onClick.AddListener(() => GameManager.instance.StartGame(name));

        for (int index = 0; index < difficulty; index++)
        {
            Vector3 position = Vector3.right * (index - 0.5f * (difficulty - 1f)) * difficultyPrefab.rect.width;
            RectTransform difficultyInstance = (RectTransform)Instantiate(difficultyPrefab, position, Quaternion.identity);
            difficultyInstance.SetParent(difficultyMode, false);
        }
    }

}
