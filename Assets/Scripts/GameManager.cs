using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState { GameOn, GamePaused, GameOff };

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    [Header("Main")]

    public PlayerController player;
    public SpawnerController spawner;
    public Transform rails;
    public Camera mainCamera;

    public GameState gameState = GameState.GameOff;

    [Header("UI")]

    public Text scoreText;
    public Text timeText;
    public Text msgText;
    public Slider timeSlider;
    public GameObject pauseButton;

    [Header("Canvas")]

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public GameObject gameCanvas;
    public GameObject levelClearCanvas;
    public GameObject achievementListPanel;

    [Header("Levels")]

    public ShiftScreenController levelSelector;
    private List<Level> levels;
    public float startTime;
    public float endLevelDelay;

    private int levelIndex = 0;

    [Header("Statistics")]

    public PlayerStatistics stats;
    private int blackCollected;
    private int whiteCollected;

    private const char lineSeparator = '\n';
    private const char fieldSeparator = ',';

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

        levels = new List<Level>();
        Load();
        SetLevel("Come And Find Me");
	}

    void OnDestroy ()
    {
        Save();
    }

    public void ResetProgress()
    {
        stats.Reset();
        AchievementManager.instance.Reset();

    }

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/achievements.dat");
        bf.Serialize(file, stats);
        file.Close();
    }

    void LoadLevel(TextAsset levelAsset)
    {
        //init colored blocks
        string[] spawns = levelAsset.text.Split(lineSeparator);
        if(spawns.Length >= 1)
        {
            Level level = new Level
            {
                name = levelAsset.name
            };

            string[] spawnParameters = spawns[0].Split(fieldSeparator);

            // Init level metadata
            if(spawnParameters.Length >= 5)
            {
                level.music = spawnParameters[0];
                bool initOkay = true;
                initOkay &= int.TryParse(spawnParameters[1], out level.index);
                initOkay &= int.TryParse(spawnParameters[2], out level.difficulty);
                initOkay &= float.TryParse(spawnParameters[3], out level.speed);
                initOkay &= float.TryParse(spawnParameters[4], out level.beat);
                if (initOkay)
                {
                    stats.InitHighScore(level.name);
                    level.deltaTime = (spawner.gameObject.transform.position.x - rails.position.x + player.GetComponent<CircleCollider2D>().radius) / level.speed;
                    for (int lineIndex = 1; lineIndex < spawns.Length; lineIndex++)
                    {
                        level.LoadSpawn(spawns[lineIndex].Split(fieldSeparator), level.deltaTime);
                    }

                    level.duration += level.deltaTime;

                    levels.Add(level);

                    if (level.name.Equals("Credits"))
                    {
                        level.deathEnabled = false;
                    }
                    
                }
            }
        }
    }

    void Load()
    {
        // Load achievements
        if (File.Exists(Application.persistentDataPath + "/achievements.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/achievements.dat", FileMode.Open);
            stats = (PlayerStatistics)bf.Deserialize(file);
            stats.RegisterAchievements();
            stats.CheckAchievements();
            file.Close();
        }
        else
        {
            stats = new PlayerStatistics();
        }

        // Load levels
        TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels");
        foreach (TextAsset levelAsset in levelAssets)
        {
            LoadLevel(levelAsset);
        }

        // Sort Levels
        levels.Sort(delegate(Level fst, Level snd)
        {
            if (fst.index == snd.index)
            {
                return 0;
            }
            else if (fst.index > snd.index)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        });

        // Create related UI
        foreach (Level level in levels)
        {
            if (level.deathEnabled)
            {
                levelSelector.AddLevel(level);
            }
        }
    }
    

    public void SetLevel(string name)
    {
        levelIndex = 0;     
        while (levelIndex < levels.Count && !levels[levelIndex].name.Equals(name))
        {
            levelIndex++;
        }
        if (levelIndex == levels.Count)
        {
            levelIndex = 0;
        }
        levels[levelIndex].ResetCheckPoint();
    }

    public void StartGame()
	{
        // Modify global player stats
        stats.plays.Increment();

        // Start new game
        player.gameObject.SetActive(true);
        player.pulseInterval = levels[levelIndex].beat;
        player.EnableDeath(levels[levelIndex].deathEnabled);

        // Put Camera to White
        mainCamera.backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);
        Level currentLevel = levels[levelIndex];

        startTime = currentLevel.GetLastCheckPoint();

        timeSlider.maxValue = currentLevel.duration;
        timeSlider.value = startTime;

        RestoreSavedState(currentLevel.savedState);

        if (levelIndex != 0) // The tutorial (level 0) uses the menu background music
        {
            SoundManager.instance.ChangeBackgroundMusic(currentLevel.music, startTime, false);
        }

        spawner.StartLevel(currentLevel, startTime);
    }

    public void RestoreSavedState(CheckPointSave savedState)
    {
        blackCollected = savedState.blackScoreAtCheckPoint;
        whiteCollected = savedState.whiteScoreAtCheckPoint;
        player.SetRail(savedState.railIndex);
        player.SetColor(savedState.playerColor);
        mainCamera.backgroundColor = savedState.backgroundColor;
       
    }

    public void StartGame(string levelName)
    {
        SetLevel(levelName);
        StartGame();
    }

    // Stop Game
    public void EndGame() 
    {
        // Stop time
        SetPauseState(GameState.GameOff);
        spawner.gameObject.SetActive(false);

        // Hide Player
        player.gameObject.SetActive(false);

        // Put Back Camera to White
        mainCamera.backgroundColor = Color.white;

        // Destroy every obstacle
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (GameObject item in obstacles)
        {
            Destroy(item);
        }

        // Destroy every collectible
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
        UpdateGameOverText();
        EndGame();
        gameOverCanvas.SetActive(true);
    }

    public IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(endLevelDelay);

        EndGame();
		stats.totalScore.value += GetScore();
        stats.successPlays.Increment();

        UpdateLevelClearText();
        levelClearCanvas.SetActive(true);

        // Modify global player stats
        if (GetScore() >= levels[levelIndex].maxScore) 
        {
            stats.perfectPlays.Increment();
        } 
    }

    public void ShowText(string text, float time)
    {
        StartCoroutine(PrintText(text, time));
    }

    IEnumerator PrintText(string text, float time)
    {
        if (!msgText.gameObject.activeSelf)
        {
            msgText.text = text;
            msgText.gameObject.SetActive(true);
            Color msgColor = msgText.color;
            msgColor.a = 1f;
            msgText.color = msgColor;
            yield return new WaitForSeconds(time);
            while (msgColor.a > 0f)
            {
                msgColor.a -= 0.01f;
                msgText.color = msgColor;
                yield return null;
            }
            msgText.gameObject.SetActive(false);
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
			TogglePause ();
        }

        // Update score display
        UpdateUI();

        // Check check points
        if (gameState == GameState.GameOn) {
            UpdateCheckPoint();
        }
    }

	public void TogglePause()
	{
		if (gameState == GameState.GameOn)
		{
			SetPauseState(GameState.GamePaused);

		}
		else if (gameState == GameState.GamePaused && achievementListPanel.activeSelf == false)
		{
			SetPauseState(GameState.GameOn);
		} 
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
			pauseButton.SetActive (false);
        }
        else if (pauseState == GameState.GameOn)
        {
            Time.timeScale = 1;
            gameState = GameState.GameOn;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.UnPause();
			pauseButton.SetActive (true);
            gameCanvas.SetActive(true);
        }
        else // Game off !
        {
            Time.timeScale = 0;
            gameState = GameState.GameOff;
            pauseCanvas.SetActive(false);
            SoundManager.instance.musicSource.Pause();
			pauseButton.SetActive (false);
            gameCanvas.SetActive(false);
        }
    }

    int GetScore ()
    {
        return blackCollected + whiteCollected;
    }

    void UpdateUI()
    {
        scoreText.text = GetScore().ToString();
        timeText.text = string.Format("{0:0.00}", spawner.time);
        timeSlider.value = spawner.time;
    }

    void UpdateCheckPoint()
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        levels[levelIndex].UpdateCheckPoint(spawner.time, blackCollected, whiteCollected, controller.GetRail(), controller.GetColor(), mainCamera.backgroundColor);
    }

    void UpdateLevelClearText()
    {
        Text score = levelClearCanvas.gameObject.transform.Find("Score").GetComponent<Text>();
        Text highScore = levelClearCanvas.gameObject.transform.Find("High Score").GetComponent<Text>();
   
        UpdateHighScoreText(score, highScore);
    }

    void UpdateGameOverText()
    {
        Text score = gameOverCanvas.gameObject.transform.Find("Score").GetComponent<Text>();
        Text highScore = gameOverCanvas.gameObject.transform.Find("High Score").GetComponent<Text>();

        UpdateHighScoreText(score, highScore);
    }

    void UpdateHighScoreText(Text score, Text highScore)
    {
        // Update high score
        bool newHighScore = false;
        string name = levels[levelIndex].name;
        if (GetScore() > stats.highScores.GetValue(name).value)
        {
            stats.highScores.GetValue(name).value = GetScore();
            newHighScore = true;
        }

        // Display high score text
        score.text = "SCORE " + GetScore().ToString() + " / " + levels[levelIndex].maxScore.ToString();
        highScore.text = "HIGH SCORE " + stats.highScores.GetValue(name).value;
        if (newHighScore)
        {
            highScore.text = "NEW " + highScore.text;
        }
    }

    public void BlackCollected()
    {
        blackCollected++;
    }

    public void WhiteCollected()
    {
        whiteCollected++;
    }
}


