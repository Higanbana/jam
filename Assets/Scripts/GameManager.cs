using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState { GameOn, GamePaused, GameOff };

public class Level
{
    public string name;
    public SpawnParameters[] spawns;
    public int maxScore;
    public float duration;

    public Level(string levelName, SpawnParameters[] spawnParameters, int score, float length)
    {
        name = levelName;
        spawns = spawnParameters;
        maxScore = score;
        duration = length;
    }
}

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public Text scoreText;
    public Text timeText;
    public Text tutorialText;
    public Slider timeSlider;

    public GameState gameState = GameState.GameOff;

    public GameObject pauseCanvas;
    public GameObject gameOverCanvas;
    public GameObject gameCanvas;
    public GameObject levelClearCanvas;
    public GameObject achievementListPanel;

    private Level[] levels;
    private int levelIndex = 0;

    public PlayerStatistics stats;

    private char lineSeparator = '\n';
    private char fieldSeparator = ',';

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

        Load();
        SetLevel("ComeAndFindMe");
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

    public void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/achievements.dat");
        bf.Serialize(file, stats);
        file.Close();
    }

    void Load()
    {
        //load achievements
        if (File.Exists(Application.persistentDataPath + "/achievements.dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/achievements.dat", FileMode.Open);
            stats = (PlayerStatistics)bf.Deserialize(file);
            stats.CheckAchievements();
            file.Close();
        }
        else
        {
            stats = new PlayerStatistics();
        }

        //Load levels
        TextAsset[] levelAssets = Resources.LoadAll<TextAsset>("Levels");

        levels = new Level[levelAssets.Length];
        
        for (int levelIndex = 0; levelIndex < levelAssets.Length; levelIndex++)
        {
            string[] spawns = levelAssets[levelIndex].text.Split(lineSeparator);

            SpawnParameters[] levelParameters = new SpawnParameters[spawns.Length];
            int maxScore = 0;
            float duration = 0f;

            for (int lineIndex = 0; lineIndex < spawns.Length; lineIndex++)
            {
                string[] spawnParameters = spawns[lineIndex].Split(fieldSeparator);
                
                // If the line has normal data, create game objects
                if (spawnParameters.Length >= 5)
                {
                    string type = spawnParameters[0];
                    float startTime = float.Parse(spawnParameters[1]);

                    if(startTime > duration)
                    {
                        duration = startTime;
                    }

                    // Collectibles
                    if (type.Contains("C"))
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float angle = float.Parse(spawnParameters[3]);
                        string shape = spawnParameters[4];
                        levelParameters[lineIndex] = new CollectibleParameters(startTime, railIndex, angle, shape);
                        maxScore++;
                    }
                    // Wave
                    else if (type.Contains("W"))
                    {
                        float X = float.Parse(spawnParameters[2]);
                        float Y = float.Parse(spawnParameters[3]);
                        Vector2 position = new Vector2(X, Y);
                        Color color = GetColor(spawnParameters[4]);
                        float speed = float.Parse(spawnParameters[5]);
                        levelParameters[lineIndex] = new WaveParameters(startTime, position, speed, color);
                    }
                    // Triggers
                    else if (type.Contains("S"))
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float endTime = float.Parse(spawnParameters[3]);
                        Color color = GetColor(spawnParameters[4]);
                        levelParameters[lineIndex] = new TriggerParameters(startTime, endTime, railIndex, color);
                    }
                    // Obstacles
                    else
                    {
                        int railIndex = int.Parse(spawnParameters[2]);
                        float endTime = float.Parse(spawnParameters[3]);
                        Color color = GetColor(spawnParameters[4]);
                        levelParameters[lineIndex] = new ObstacleParameters(startTime, endTime, railIndex, color);
                    }
                }
                // Default obstacle
                else
                {
                    levelParameters[lineIndex] = new ObstacleParameters(0f, 0f, 2, Color.black);
                }
            }
            levels[levelIndex] = new Level(levelAssets[levelIndex].name, levelParameters, maxScore, duration + endLevelDelay);
        }
        
    }
    

    public void SetLevel(string name)
    {
        levelIndex = 0;
        while(levelIndex < levels.Length && !levels[levelIndex].name.Equals(name))
        {
            levelIndex++;
        }
        if(levelIndex == levels.Length)
        {
            levelIndex = 0;
        }
    }

    public void StartGame()
	{
        // Modify global player stats
        stats.plays.Increment();

        // Start new game
        player.SetActive(true);
        player.GetComponent<PlayerController>().touchTrigger = false;

        spawner.SetActive(true);

        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);
        SoundManager.instance.ChangeBackgroundMusic(2, false);

        timeSlider.maxValue = levels[levelIndex].duration;
        timeSlider.value = 0f;

        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.SetMusicAtTime(startTime);

        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        controller.SetSpawns(levels[levelIndex].spawns);
        controller.SetTime(startTime);

        StartCoroutine(TutorialText());
    }

    // Stop Game
    void EndGame() 
    {
        SetPauseState(GameState.GameOff);
        spawner.SetActive(false);

        // Hide Player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.SetActive(false);
        }

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
        EndGame();
        gameOverCanvas.SetActive(true);
    }


    public IEnumerator EndLevel()
    {
        yield return new WaitForSeconds(endLevelDelay);

        EndGame();

        // Update high score
        if (GetScore() > stats.highScore.value)
        {
            stats.highScore.value = GetScore();
            UpdateLevelClearText(true);
        }
        else
        {
            UpdateLevelClearText(false);
        }

        // Modify global player stats
        stats.succesPlays.Increment();

        levelClearCanvas.SetActive(true);

        // Modify global player stats
        if (GetScore() >= levels[levelIndex].maxScore) 
        {
            stats.perfectPlays.Increment();
        } 
    }


    IEnumerator TutorialText()
    {
        tutorialText.gameObject.SetActive(true);
        Color tutoColor = tutorialText.color;
        while (tutoColor.a > 0f)
        {
            tutoColor.a -= 0.0025f;
            tutorialText.color = tutoColor;
            yield return null;
        }
        tutorialText.gameObject.SetActive(false);
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
            else if (gameState == GameState.GamePaused && achievementListPanel.activeSelf == false)
            {
                SetPauseState(GameState.GameOn);
            }           
        }

        // Update score display
        UpdateUI();
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

    void UpdateUI()
    {
        scoreText.text = GetScore().ToString();
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        timeText.text = string.Format("{0:0.00}", controller.time);
        timeSlider.value = controller.time;
    }

    void UpdateLevelClearText(bool newHighScore)
    {
        Text title = levelClearCanvas.gameObject.transform.Find("Title").GetComponent<Text>();
        Text score = levelClearCanvas.gameObject.transform.Find("Score").GetComponent<Text>();
        Text highScore = levelClearCanvas.gameObject.transform.Find("High Score").GetComponent<Text>();

        int index = levelIndex + 1;
        score.text = "SCORE " + GetScore().ToString() + " / " + levels[levelIndex].maxScore.ToString();
        highScore.text = "HIGH SCORE " + stats.highScore.value;
        if (newHighScore)
        {
            highScore.text = "NEW " + highScore.text;
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


