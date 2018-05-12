using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSave
{

    public int blackScoreAtCheckPoint = 0;
    public int whiteScoreAtCheckPoint = 0;
    public int railIndex = 2;
    public Color playerColor = Color.black;
    public Color backgroundColor = Color.white;

    public void Save(int blackScore, int whiteScore, int railIndex, Color playerColor, Color backgroundColor)
    {
        this.blackScoreAtCheckPoint = blackScore;
        this.whiteScoreAtCheckPoint = whiteScore;
        this.railIndex = railIndex;
        this.playerColor = playerColor;
        this.backgroundColor = backgroundColor;
    }
}

public class Level
{
    public string name = "";
    public string music = "";
    public int index = 0;
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

    public bool UpdateCheckPoint(float time, int blackScore, int whiteScore, int railIndex, Color playerColor, Color backgroundColor)
    {
        if (lastCheckPointIndex < checkPoints.Count - 1 && time > checkPoints[lastCheckPointIndex + 1])
        {
            lastCheckPointIndex++;
            savedState.Save(blackScore, whiteScore, railIndex, playerColor, backgroundColor);
            return true;
        }
        return false;
    }

    public float GetLastCheckPoint()
    {
        if (lastCheckPointIndex >= 0)
        {
            return checkPoints[lastCheckPointIndex];
        }
        else
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
    public void LoadSpawn(string[] spawnParameters, float deltaTime)
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
                        checkPoints.Add(time + deltaTime);
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
