using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum GameState { GameOn, GamePaused, GameOff };

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public Text scoreText;
    public Text timeText;

    public GameState gameState = GameState.GameOff;

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public float startTime;
	public GameObject gameCanvas;
    public int blackCollected;
    public int whiteCollected;

	public static GameManager instance = null;

	// Use this for initialization
	void Start ()
	{
		if (instance == null)
        {
			instance = this;
		}
		else if (instance != this) 
		{
			Destroy (gameObject);
		}
	}

	public void StartGame()
	{
        player.SetActive(true);
        player.GetComponent<PlayerController>().touchTrigger = false;

        spawner.SetActive(true);

        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);
        SoundManager.instance.ChangeBackgroundMusic(2);

        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.setMusicAtTime(startTime);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        controller.SetTime(startTime);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GameOver()
    {
        EndGame();
        gameOverCanvas.SetActive(true);
    }

    public void RestartGame()
    {
        EndGame();
        StartGame();
    }

    void EndGame()
    {
        SetPauseState(GameState.GameOff);
        spawner.SetActive(false);

        //Hide Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(false);
        }

        //Destroy every obstacle
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject item in obstacles)
        {
            Destroy(item);
        }

        //Destroy every collectible
        GameObject[] collectibles = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (GameObject item in collectibles)
        {
            Destroy(item);
        }
    }

    void Update()
    {
        // Pause game with esc button
        if (Input.GetButtonDown("Cancel"))
        {
            if (gameState == GameState.GameOn)
            {
                SetPauseState(GameState.GamePaused);
            }
            else if (gameState == GameState.GamePaused)
            {
                SetPauseState(GameState.GameOn);
            }           
        }

        //update score display
        UpdateScoreText();
    }

    public bool IsPaused()
    {
        if (gameState == GameState.GamePaused)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetPauseState(GameState pauseState)
    {
        if (pauseState == GameState.GamePaused)
        {
            Time.timeScale = 0;
            gameState = GameState.GamePaused;
            pauseCanvas.SetActive(true);
            SoundManager.instance.musicSource.Pause();

        }
        else if (pauseState == GameState.GameOn)
        {
            Time.timeScale = 1;
            gameState = GameState.GameOn;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.Play();
            gameCanvas.SetActive(true);
        }
        else // Game off !
        {
            Time.timeScale = 0;
            gameState = GameState.GameOff;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.Pause();
            gameCanvas.SetActive(false);
        }
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score : " + (blackCollected + whiteCollected);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        timeText.text = "Time : " + string.Format("{0:0.00}", controller.time);
    }
}
