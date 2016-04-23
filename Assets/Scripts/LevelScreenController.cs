using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelScreenController : MonoBehaviour {

    private Camera mainCamera;
    private LayoutElement layout;
    public Text title;
    public Text score;
    public RectTransform difficultyPrefab;

	void Awake () {
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        layout = GetComponent<LayoutElement>();
	}

	void Update () {
        layout.minWidth = mainCamera.pixelWidth;
	}

    public void Setup(string name, int highScore, int maxScore, int difficulty)
    {
        title.text = name;
        score.text = highScore + " / " + maxScore;

    }

}
