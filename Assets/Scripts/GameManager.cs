using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public int gameState = (int) GameState.GameOff; // -1 = not started, 0 = started, 1 = paused
    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;

    enum GameState { GameOn, GamePaused, GameOff};

	public static GameManager instance = null;

	// Use this for initialization
	void Awake ()
	{
		if (instance == null) {
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
        spawner.SetActive(true);
        setPauseState(0);
        SoundManager.instance.ChangeBackgroundMusic(2);
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

    void EndGame()
    {
        setPauseState((int)GameState.GameOff);

        //Hide Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(false);
        }

        //Destroy every obstacle
        GameObject[] names = GameObject.FindGameObjectsWithTag("Obstacle");
 
        foreach (GameObject item in names)
        {
            Destroy(item);
        }
    }

    void Update()
    {
        // Pause game with esc button
        if (Input.GetButtonDown("Cancel"))
        {
            if (gameState == (int)GameState.GameOn)
            {
                setPauseState((int)GameState.GamePaused);
            }
            else if (gameState == (int)GameState.GamePaused)
            {
                setPauseState((int)GameState.GameOn);
            }
            
        }


    }

    private void setPauseState(int pauseState)
    {
        if (pauseState == (int)GameState.GamePaused)
        {
            Time.timeScale = 0;
            gameState = (int)GameState.GamePaused;
            pauseCanvas.SetActive(true);
            SoundManager.instance.musicSource.Pause();

        }

        else if (pauseState == (int)GameState.GameOn)
        {
            Time.timeScale = 1;
            gameState = (int)GameState.GameOn;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.Play();
        }
        else
        {
            Time.timeScale = 0;
            gameState = (int)GameState.GameOff;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.Pause();
        }

    }
}
