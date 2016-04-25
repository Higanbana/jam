using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public enum GameState { GameOn, GamePaused, GameOff };

public class Level
{
    public string name = "";
    public string music = "";
    public int difficulty = 0;
    public int maxScore = 0;
    public float duration = 0f;
    public float speed = 0f;
    public float beat = 0f;
    public List<SpawnParameters> spawns = new List<SpawnParameters>();

    static Color GetColor (string colorName)
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

    public void LoadSpawn (string[] spawnParameters)
    {
        if (spawnParameters.Length >= 5)
        {
            string type = spawnParameters[0];
            float startTime;
            if (float.TryParse(spawnParameters[1], out startTime))
            {
                if (startTime > duration)
                {
                    duration = startTime;
                }

                bool ok = true;

                // Collectibles
                if (type.Contains("C"))
                {
                    int railIndex;
                    float angle;
                    ok &= int.TryParse(spawnParameters[2], out railIndex);
                    ok &= float.TryParse(spawnParameters[3], out angle);
                    string shape = spawnParameters[4];
                    if (ok)
                    {
                        spawns.Add(new CollectibleParameters(startTime, railIndex, angle, shape));
                        maxScore++;
                    }
                }
                // Wave
                else if (type.Contains("W") && spawnParameters.Length >= 6)
                {
                    float X;
                    float Y;
                    float speed;
                    ok &= float.TryParse(spawnParameters[2], out X);
                    ok &= float.TryParse(spawnParameters[3], out Y);
                    Color color = GetColor(spawnParameters[4]);
                    ok &= float.TryParse(spawnParameters[5], out speed);
                    if (ok)
                    {
                        Vector2 position = new Vector2(X, Y);
                        spawns.Add(new WaveParameters(startTime, position, speed, color));
                    }

                }
                // Triggers
                else if (type.Contains("S"))
                {
                    int railIndex;
                    float endTime;
                    ok &= int.TryParse(spawnParameters[2], out railIndex);
                    ok &= float.TryParse(spawnParameters[3], out endTime);
                    Color color = GetColor(spawnParameters[4]);
                    if (ok)
                    {
                        spawns.Add(new TriggerParameters(startTime, endTime, railIndex, color));
                    }
                }
                // Obstacles
                else
                {
                    int railIndex;
                    float endTime;
                    ok &= int.TryParse(spawnParameters[2], out railIndex);
                    ok &= float.TryParse(spawnParameters[3], out endTime);
                    Color color = GetColor(spawnParameters[4]);
                    if (ok)
                    {
                        spawns.Add(new ObstacleParameters(startTime, endTime, railIndex, color));
                    }
                }
            }
        }
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
    public GameObject levelSelectCanvas;
    public GameObject achievementListPanel;

    public RectTransform levelSelector;
    public LevelScreenController levelScreenPrefab;
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
        string[] spawns = levelAsset.text.Split(lineSeparator);
        if(spawns.Length >= 1)
        {
            Level level = new Level();
            level.name = levelAsset.name;

            string[] spawnParameters = spawns[0].Split(fieldSeparator);

            if(spawnParameters.Length >= 4)
            {
                level.music = spawnParameters[0];
                bool ok = true;
                ok &= int.TryParse(spawnParameters[1], out level.difficulty);
                ok &= float.TryParse(spawnParameters[2], out level.speed);
                ok &= float.TryParse(spawnParameters[3], out level.beat);
                if (ok)
                {
                    for (int lineIndex = 1; lineIndex < spawns.Length; lineIndex++)
                    {
                        level.LoadSpawn(spawns[lineIndex].Split(fieldSeparator));
                    }

                    levels.Add(level);

                    if (!level.name.Equals("Credits"))
                    {
                        LevelScreenController levelScreen = (LevelScreenController)Instantiate(levelScreenPrefab, Vector3.zero, Quaternion.identity);
                        levelScreen.levelSelectCanvas = levelSelectCanvas;
                        levelScreen.Init(level.name, 0, level.maxScore, level.difficulty);
                        levelScreen.gameObject.transform.SetParent(levelSelector, false);
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
    }

    public void StartGame()
	{
        // Modify global player stats
        stats.plays.Increment();

        // Start new game
        player.SetActive(true);
        player.GetComponent<PlayerController>().touchTrigger = false;
        player.GetComponent<PlayerController>().pulseInterval = levels[levelIndex].beat;

        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);

        timeSlider.maxValue = levels[levelIndex].duration;
        timeSlider.value = 0f;

        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.ChangeBackgroundMusic(levelIndex + 1, false);
        SoundManager.instance.SetMusicAtTime(startTime);

        spawner.SetActive(true);
        SpawnerController controller = spawner.GetComponent<SpawnerController>();
        controller.SetSpawns(levels[levelIndex].spawns.ToArray());
        controller.speed = levels[levelIndex].speed;
        controller.time = startTime;

        StartCoroutine(TutorialText());
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
		stats.totalScore.value += GetScore();

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
        Text score = levelClearCanvas.gameObject.transform.Find("Score").GetComponent<Text>();
        Text highScore = levelClearCanvas.gameObject.transform.Find("High Score").GetComponent<Text>();

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
    }

    public void WhiteCollected()
    {
        whiteCollected++;
    }
}


