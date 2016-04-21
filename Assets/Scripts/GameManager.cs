﻿using UnityEngine;
using UnityEngine.SocialPlatforms;
using System.Collections;
using UnityEngine.UI;

public enum GameState { GameOn, GamePaused, GameOff };

public class Level
{
    public SpawnParameters[] spawns;
    public int maxScore;

    public Level(SpawnParameters[] spawnParameters)
    {
        spawns = spawnParameters;
    }
}

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public Text scoreText;
    public Text timeText;

    public GameState gameState = GameState.GameOff;

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public GameObject gameCanvas;
    public GameObject levelClearCanvas;
    public GameObject gameWinCanvas;

    private Level[] levels;
    [HideInInspector]
    public int levelIndex = 0;

    public PlayerStatistics stats;

    private char lineSeparator = '\n';
    private char fieldSeparator = ',';

    public float startTime;
    private int blackCollected;
    private int whiteCollected;

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
        //Social.ShowAchievementsUI();
        stats = new PlayerStatistics();
        LoadLevels();
	}

    public void ResetProgress()
    {
        stats.Reset();
        AchievementManager.instance.Reset();

    }

    Color GetColor(string colorName)
    {
        if (colorName.Contains("W"))
        {
            return Color.white;
        }
        else
        {
            return Color.black;
        }
    }

    void LoadLevels()
    {
        TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels");

        levels = new Level[levelAssets.Length];

        for (int levelIndex = 0; levelIndex < levelAssets.Length; levelIndex++)
        {
            string[] spawns = levelAssets[levelIndex].text.Split(lineSeparator);

            SpawnParameters[] levelParameters = new SpawnParameters[spawns.Length];

            for (int lineIndex = 0; lineIndex < spawns.Length; lineIndex++)
            {
                string[] spawnParameters = spawns[lineIndex].Split(fieldSeparator);
                if (spawnParameters.Length >= 5)
                {
                    string type = spawnParameters[0];
                    float startTime = float.Parse(spawnParameters[1]);

                    if (type.Contains("C"))
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float angle = float.Parse(spawnParameters[3]);
                        levelParameters[lineIndex] = new CollectibleParameters(startTime, railIndex, angle);
                    }
                    else if (type.Contains("W"))
                    {
                        float X = float.Parse(spawnParameters[2]);
                        float Y = float.Parse(spawnParameters[3]);
                        Vector2 position = new Vector2(X, Y);
                        Color color = GetColor(spawnParameters[4]);
                        float speed = float.Parse(spawnParameters[5]);
                        levelParameters[lineIndex] = new WaveParameters(startTime, position, speed, color);
                    }
                    else if (type.Contains("S"))
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float endTime = float.Parse(spawnParameters[3]);
                        Color color = GetColor(spawnParameters[4]);
                        levelParameters[lineIndex] = new TriggerParameters(startTime, endTime, railIndex, color);
                    }
                    else
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float endTime = float.Parse(spawnParameters[3]);
                        Color color = GetColor(spawnParameters[4]);
                        levelParameters[lineIndex] = new ObstacleParameters(startTime, endTime, railIndex, color);
                    }
                }
                else
                {
                    levelParameters[lineIndex] = new ObstacleParameters(0f, 0f, 2, Color.black);
                }
            }
            levels[levelIndex] = new Level(levelParameters);
        }
        
    }

    public void StartGame()
	{
        //modify global player stats
        stats.plays.Increment();

        //start new game
        player.SetActive(true);
        player.GetComponent<PlayerController>().touchTrigger = false;

        spawner.SetActive(true);

        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);
        SoundManager.instance.ChangeBackgroundMusic(2, false);

        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.SetMusicAtTime(startTime);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        controller.SetSpawns(levels[levelIndex].spawns);
        controller.SetTime(startTime);
    }

    // Stop Game
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GameOver()
    {
        stats.death.Increment();
        EndGame();
        gameOverCanvas.SetActive(true);
    }


    public IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(8f);

        EndGame();

        UpdateLevelClearText();
        levelIndex++;

        //modify global player stats
        stats.succesPlays.Increment();

        // test if the level is the last level
        if (levelIndex < levels.Length)
        {
            levelClearCanvas.SetActive(true);

            //modify global player stats
            if (GetScore() == levels[levelIndex].maxScore) 
            {
                stats.perfectPlays.Increment();
            } 

        }
        else
        {
            gameWinCanvas.SetActive(true);
        }
    }

    public void RestartGame()
    {
        EndGame();
        StartGame();
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

    int GetScore ()
    {
        return blackCollected + whiteCollected;
    }

    void UpdateScoreText()
    {
        scoreText.text = "Score : " + GetScore();
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        timeText.text = "Time : " + string.Format("{0:0.00}", controller.time);
    }

    void UpdateLevelClearText()
    {
        if (levelIndex < levels.Length)
        {
            Text title = levelClearCanvas.gameObject.transform.Find("Title").GetComponent<Text>();
            Text score = levelClearCanvas.gameObject.transform.Find("Score").GetComponent<Text>();

            int index = levelIndex + 1;
            title.text = "Level " + index.ToString() + " Clear !";
            score.text = "Score : " + GetScore().ToString() + " / " + levels[levelIndex].maxScore.ToString();
        }
    }

    public void BlackCollected()
    {
        blackCollected++;
        stats.totalScore.Increment();
    }

    public void WhiteCollected()
    {
        whiteCollected++;
        stats.totalScore.Increment();
    }
}


