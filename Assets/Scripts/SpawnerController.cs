using UnityEngine;
using System.Collections;


public class SpawnParameters
{
    public float spawnTime;
    public int railIndex;
    public float railLength;

    public SpawnParameters(float time, int index, float length)
    {
        spawnTime = time;
        railIndex = index;
        railLength = length;
    }
}

public class Level
{
    public SpawnParameters[] spawns;

    public Level(SpawnParameters[] spawnParameters)
    {
        spawns = spawnParameters;
    }
}

public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;

    public Transform[] rails;

    public float speed;

    private Level[] levels;
    private int levelIndex = 0;
    private int spawnIndex = 0;

    [HideInInspector]
    public float time = 0.0f;

    private char lineSeparator = '\n';
    private char fieldSeparator = ',';
	
    void Start ()
    {
        LoadLevels();
    }

    void OnEnable()
    {
        spawnIndex = 0;
        time = 0f;
    }

    public void setTime(float newTime)
    {
        time = newTime;
        int newIndex = 0;
        while (newIndex < spawnParameters.Length && spawnParameters[newIndex].spawnTime <= time)
        {
            newIndex++;
        }
        spawnIndex = newIndex;
    }	

    void LoadLevels ()
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
                if(spawnParameters.Length >= 3)
                {
                    float spawnTime = float.Parse(spawnParameters[0]);
                    int railIndex = int.Parse(spawnParameters[1]);
                    float railLength = float.Parse(spawnParameters[2]);

                    levelParameters[lineIndex] = new SpawnParameters(spawnTime, railIndex, railLength);
                }
                else
                {
                    levelParameters[lineIndex] = new SpawnParameters(0f, 2, 0f);
                }
            }

            levels[levelIndex] = new Level(levelParameters);
        }
    }

	// Update is called once per frame
	void Update ()
    {
        time += Time.deltaTime;
        while (spawnIndex < levels[levelIndex].spawns.Length && levels[levelIndex].spawns[spawnIndex].spawnTime <= time)
        {
            int railIndex = levels[levelIndex].spawns[spawnIndex].railIndex;
            float height;
            float offset;
            if (railIndex == 0)
            {
                height = Mathf.Abs(rails[1].position.y - rails[0].position.y);
                offset = 0f;
            }
            else if (railIndex == rails.Length - 1)
            {
                height = Mathf.Abs(rails[rails.Length - 1].position.y - rails[rails.Length - 2].position.y);
                offset = 0f;
            }
            else
            {
                float toNext = Mathf.Abs(rails[railIndex + 1].position.y - rails[railIndex].position.y);
                float toPrevious = Mathf.Abs(rails[railIndex - 1].position.y - rails[railIndex].position.y);
                height = 0.5f * (toPrevious + toNext);
                offset = 0.5f * (toPrevious - toNext);
            }
            Vector3 obstaclePosition = new Vector3(transform.position.x + levels[levelIndex].spawns[spawnIndex].railLength * 0.5f, rails[railIndex].position.y + offset, 0f);
            ObstacleController obstacle = (ObstacleController)Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
            obstacle.Setup(height, levels[levelIndex].spawns[spawnIndex].railLength, speed);
            spawnIndex++;
        }
	}
}
