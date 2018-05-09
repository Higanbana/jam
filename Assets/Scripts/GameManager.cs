using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState { GameOn, GamePaused, GameOff };

public class CheckPointSave
{
    
    public int blackScoreAtCheckPoint = 0;
    public int whiteScoreAtCheckPoint = 0;
    public int railIndex = 2;
    public Color playerColor = Color.black;

    public void Save(int blackScore, int whiteScore, int railIndex, Color playerColor)
    {
        this.blackScoreAtCheckPoint = blackScore;
        this.whiteScoreAtCheckPoint = whiteScore;
        this.railIndex = railIndex;
        this.playerColor = playerColor;
    }
}

public class Level
{
    public string name = "";
    public string music = "";
    public int difficulty = 0;
    public int maxScore = 0;
    public float duration = 0f;
    public float speed = 0f;
    public float beat = 0f;
    public bool deathEnabled = true;
    public float deltaTime; // Time needed for a spawned object to reach player's position

    // Check points
    public List<float> checkPoints = new List<float>();
    public int lastCheckPointIndex = -1;
    public CheckPointSave savedState = new CheckPointSave();


    // Spawns
    public List<SpawnParameters> spawns = new List<SpawnParameters>();  

    public bool UpdateCheckPoint(float time, int blackScore, int whiteScore, int railIndex, Color playerColor)
    {
        if (lastCheckPointIndex < checkPoints.Count-1 && time > checkPoints[lastCheckPointIndex + 1])
        {
            lastCheckPointIndex++;
            savedState.Save(blackScore, whiteScore, railIndex, playerColor);
            return true;
        }
        return false;
    }

    public float GetLastCheckPoint()
    {
        if (lastCheckPointIndex >= 0)
        {
            return checkPoints[lastCheckPointIndex];
        } else
        {
            return 0;
        }
    }

    public void ResetCheckPoint()
    {
        lastCheckPointIndex = -1;
        savedState = new CheckPointSave();
    }

    // Delta time : time for a spawned object to reach player
    public void LoadSpawn (string[] spawnParameters, float deltaTime)
    {
        if (spawnParameters.Length > 0 && spawnParameters[0].Length > 0)
        {
            char type = spawnParameters[0][0];
            SpawnParameters spawn = null;
            switch (type)
            {
                case 'C':
                    spawn = CollectibleParameters.UnstreamCollectible(this, spawnParameters);
                    break;
                case 'O':
                    spawn = ObstacleParameters.UnstreamObstacle(this, spawnParameters);
                    break;
                case 'W':
                    spawn = WaveParameters.UnstreamWave(this, spawnParameters);
                    break;
                case 'S':
                    spawn = TriggerParameters.UnstreamTrigger(this, spawnParameters);
                    break;
                case 'T':
                    spawn = TextParameters.UnstreamText(this, spawnParameters);
                    break;
                case 'P':
                    float time;
                    if (spawnParameters.Length >= 2 & float.TryParse(spawnParameters[1], out time))
                    {
                        checkPoints.Add(time+deltaTime);
                    }
                    break;
            }
            if (spawn != null)
            {
                spawns.Add(spawn);
            }
        }
    }
}

public class GameManager : MonoBehaviour {

    public GameObject player;
    public SpawnerController spawner;
    public Transform rails;
    public Camera mainCamera;

    public GameState gameState = GameState.GameOff;

    public Text scoreText;
    public Text timeText;
    public Text msgText;
    public Slider timeSlider;

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public GameObject gameCanvas;
    public GameObject levelClearCanvas;
    public GameObject achievementListPanel;
	public GameObject pauseButton;

    public ShiftScreenController levelSelector;
    private List<Level> levels;

    private int levelIndex = 0;

    public PlayerStatistics stats;

    private const char lineSeparator = '\n';
    private const char fieldSeparator = ',';

    public float startTime;
    public float endLevelDelay;
    private int blackCollected;
    private int whiteCollected;

	public static GameManager instance = null;

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
            if(spawnParameters.Length >= 4)
            {
                level.music = spawnParameters[0];
                bool initOk = true;
                initOk &= int.TryParse(spawnParameters[1], out level.difficulty);
                initOk &= float.TryParse(spawnParameters[2], out level.speed);
                initOk &= float.TryParse(spawnParameters[3], out level.beat);
                if (initOk)
                {
                    stats.InitHighScore(level.name);
                    level.deltaTime = (spawner.gameObject.transform.position.x - rails.position.x + player.GetComponent<CircleCollider2D>().radius) / level.speed;
                    for (int lineIndex = 1; lineIndex < spawns.Length; lineIndex++)
                    {
                        level.LoadSpawn(spawns[lineIndex].Split(fieldSeparator), level.deltaTime);
                    }

                    level.duration += level.deltaTime;

                    levels.Add(level);

                    if (!level.name.Equals("Credits"))
                    {
                        levelSelector.AddLevel(level
                            
                            
                            );
                    }
                    else
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

    }
    

    public void SetLevel(string name)
    {
        levelIndex = 0;     
        while(levelIndex < levels.Count && !levels[levelIndex].name.Equals(name))
        {
            levelIndex++;
        }
        if(levelIndex == levels.Count)
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
        player.SetActive(true);
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.pulseInterval = levels[levelIndex].beat;
        playerController.EnableDeath(levels[levelIndex].deathEnabled);

        // Put Camera to White
        mainCamera.backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);
        Level currentLevel = levels[levelIndex];

        startTime = currentLevel.GetLastCheckPoint();

        timeSlider.maxValue = currentLevel.duration;
        timeSlider.value = startTime;

        RestoreSavedState(currentLevel.savedState);

        SoundManager.instance.ChangeBackgroundMusic(currentLevel.music, false);
        SoundManager.instance.SetMusicAtTime(startTime);

        spawner.StartLevel(currentLevel, startTime);

    }

    public void RestoreSavedState(CheckPointSave savedState)
    {
        blackCollected = savedState.blackScoreAtCheckPoint;
        whiteCollected = savedState.whiteScoreAtCheckPoint;
        PlayerController playerController = player.GetComponent<PlayerController>();
        playerController.SetRail(savedState.railIndex);
        playerController.SetColor(savedState.playerColor);
    }

    public void StartGame(string levelName)
    {
        SetLevel(levelName);
        StartGame();
    }

    // Stop Game
    public void EndGame() 
    {
        SetPauseState(GameState.GameOff);
        spawner.gameObject.SetActive(false);

        // Hide Player
        player.SetActive(false);

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
            SoundManager.instance.musicSource.Play();
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
        levels[levelIndex].UpdateCheckPoint(spawner.time, blackCollected, whiteCollected, controller.GetRail(), controller.GetColor());
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


