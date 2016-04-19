﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public Text scoreText;
    public Text timeText;

    public int gameState = (int) GameState.GameOff; // -1 = not started, 0 = started, 1 = paused

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public float testStartTime;

    public int blackCollected;
    public int whiteCollected;

    enum GameState { GameOn, GamePaused, GameOff};

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
        if (testStartTime == 0)
        {
            player.SetActive(true);
            spawner.SetActive(true);
            SetPauseState((int)GameState.GameOn);
            SoundManager.instance.ChangeBackgroundMusic(2);
        }
        else
        {
            StartGameAtTime(testStartTime);
        }
		
    }

    public void StartGameAtTime(float time)
    {
        player.SetActive(true);
        spawner.SetActive(true);
        SetPauseState((int)GameState.GameOn);
        SoundManager.instance.ChangeBackgroundMusic(2);
        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.setMusicAtTime(time);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        controller.SetTime(time);

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
        SetPauseState((int)GameState.GameOff);
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
            if (gameState == (int)GameState.GameOn)
            {
                SetPauseState((int)GameState.GamePaused);
            }
            else if (gameState == (int)GameState.GamePaused)
            {
                SetPauseState((int)GameState.GameOn);
            }           
        }

        //update score display
        UpdateScoreText();
    }

    public bool IsPaused()
    {
        if (gameState == (int) GameState.GamePaused)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SetPauseState(int pauseState)
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

    void UpdateScoreText()
    {
        scoreText.text = "Score : " + (blackCollected + whiteCollected);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        timeText.text = "Time : " + string.Format("{0:0.00}", controller.time);
    }
}
