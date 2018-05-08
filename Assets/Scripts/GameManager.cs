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

    public void LoadSpawn (string[] spawnParameters)
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

                    level.duration += (spawner.gameObject.transform.position.x - rails.position.x) / level.speed;

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
        player.GetComponent<PlayerController>().pulseInterval = levels[levelIndex].beat;

        // Put Camera to White
        mainCamera.backgroundColor = Color.white;

        SetPauseState(GameState.GameOn);

        timeSlider.maxValue = levels[levelIndex].duration;
        timeSlider.value = 0f;

        blackCollected = 0;
        whiteCollected = 0;

        SoundManager.instance.ChangeBackgroundMusic(levels[levelIndex].music, false);
        SoundManager.instance.SetMusicAtTime(startTime);

        spawner.StartLevel(levels[levelIndex], startTime);
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
            yield return new WaitForSeconds(time);
            Color msgColor = msgText.color;
            while (msgColor.a > 0f)
            {
                msgColor.a -= 0.0025f;
                msgText.color = msgColor;
                yield return null;
            }
            msgText.gameObject.SetActive(false);
            msgColor.a = 1f;
            msgText.color = msgColor;
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
        timeText.text = string.Format("{0:0.00}", spawner.time);
        timeSlider.value = spawner.time;
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


