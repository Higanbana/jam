using UnityEngine;
using System.Collections;

public abstract class SpawnParameters
{
    public float spawnTime;

    public abstract GameObject Spawn(SpawnerController spawner);

    protected static Color GetColor(string colorName)
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
}

public class ObstacleParameters : SpawnParameters
{
    private float stopTime;
    private Color spawnColor;
    private int railIndex;

    public ObstacleParameters(float startTime, float endTime, int index, Color color)
    {
        spawnTime = startTime;
        stopTime = endTime;
        spawnColor = color;
        railIndex = index;
    }

    public static SpawnParameters UnstreamObstacle(Level level, string[] parameters)
    {
        if (parameters.Length >= 5)
        {
            float startTime;
            float endTime;
            int railIndex;
            if (float.TryParse(parameters[1], out startTime) && int.TryParse(parameters[2], out railIndex) && float.TryParse(parameters[3], out endTime))
            {
                if (level.duration < startTime)
                {
                    level.duration = startTime;
                }
                return new ObstacleParameters(startTime, endTime, railIndex, GetColor(parameters[4]));
            }
        }
        return null;
    }

    public override GameObject Spawn(SpawnerController spawner)
    {
        float height;
        float length = spawner.level.speed * (stopTime - spawnTime);
        Vector3 offset = Vector3.zero;
        float deltaTime = spawnTime - spawner.time;
        offset.x = 0.5f * length + spawner.level.speed * deltaTime;
        if (railIndex == 0)
        {
            height = Mathf.Abs(spawner.rails[1].position.y - spawner.rails[0].position.y);
        }
        else if (railIndex == spawner.rails.Length - 1)
        {
            height = Mathf.Abs(spawner.rails[spawner.rails.Length - 1].position.y - spawner.rails[spawner.rails.Length - 2].position.y);
        }
        else
        {
            float toNext = Mathf.Abs(spawner.rails[railIndex + 1].position.y - spawner.rails[railIndex].position.y);
            float toPrevious = Mathf.Abs(spawner.rails[railIndex - 1].position.y - spawner.rails[railIndex].position.y);
            height = 0.5f * (toPrevious + toNext);
            offset.y = 0.5f * (toPrevious - toNext);
        }
        Vector3 obstaclePosition = new Vector3(spawner.transform.position.x, spawner.rails[railIndex].position.y, 0f) + offset;
        ObstacleController obstacle = (ObstacleController)Object.Instantiate(spawner.obstaclePrefab, obstaclePosition, Quaternion.identity);
        obstacle.Setup(height, length, spawner.level.speed, spawnColor, spawner.GetOrderInLayer());
        return obstacle.gameObject;
    }
}

public class TriggerParameters : ObstacleParameters
{
    public TriggerParameters(float startTime, float endTime, int index, Color color) : base(startTime, endTime, index, color)
    {
    }

    public static SpawnParameters UnstreamTrigger(Level level, string[] parameters)
    {
        if (parameters.Length >= 5)
        {
            float startTime;
            float endTime;
            int railIndex;
            if (float.TryParse(parameters[1], out startTime) && int.TryParse(parameters[2], out railIndex) && float.TryParse(parameters[3], out endTime))
            {
                if (level.duration < startTime)
                {
                    level.duration = startTime;
                }
                return new TriggerParameters(startTime, endTime, railIndex, GetColor(parameters[4]));
            }
        }
        return null;
    }

    public override GameObject Spawn(SpawnerController spawner)
    {
        GameObject trigger = base.Spawn(spawner);
        trigger.AddComponent<TriggerObstacleController>();
        return trigger;
    }
}

public class CollectibleParameters : SpawnParameters
{
    public float spawnAngle;
    public int railIndex;
    string spawnShape;

    public CollectibleParameters(float startTime, int index, float angle, string shape = "P")
    {
        spawnTime = startTime;
        railIndex = index;
        spawnAngle = angle;
        spawnShape = shape;
    }

    public static SpawnParameters UnstreamCollectible(Level level, string[] parameters)
    {
        if (parameters.Length >= 5)
        {
            float startTime;
            int railIndex;
            float angle;
            if (float.TryParse(parameters[1], out startTime) && int.TryParse(parameters[2], out railIndex) && float.TryParse(parameters[3], out angle))
            {
                if (level.duration < startTime)
                {
                    level.duration = startTime;
                }
                level.maxScore++;
                return new CollectibleParameters(startTime, railIndex, angle, parameters[4]);
            }
        }
        return null;
    }

    public override GameObject Spawn(SpawnerController spawner)
    {

        if (spawnShape.StartsWith("L"))
        {
            return SpawnCollectibleText(spawner, spawnShape.Substring(1));
        }
        else
        {
            return SpawnCollectible(spawner);
        }
    }

    GameObject SpawnCollectible(SpawnerController spawner)
    {
        float offset = spawner.level.speed * (spawnTime - spawner.time);
        Vector3 collectiblePosition = new Vector3(spawner.transform.position.x + offset, spawner.rails[railIndex].position.y, 0f);
        CollectibleController collectible = (CollectibleController)Object.Instantiate(spawner.collectiblePrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawnAngle, spawner.level.speed, spawner.GetOrderInLayer());

        return collectible.gameObject;
    }

    GameObject SpawnCollectibleText(SpawnerController spawner, string letter)
    {
        float offset = spawner.level.speed * (spawnTime - spawner.time);
        Vector3 collectiblePosition = new Vector3(spawner.transform.position.x + offset, spawner.rails[railIndex].position.y, 0f);
        CollectibleTextController collectible = (CollectibleTextController) Object.Instantiate(spawner.collectibleTextPrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawnAngle, spawner.level.speed, spawner.GetOrderInLayer(), letter);

        return collectible.gameObject;
    }
}

public class WaveParameters : SpawnParameters
{
    public Vector3 spawnPosition;
    public Color spawnColor;
    public float scaleFactor;

    public WaveParameters(float startTime, Vector2 position, float expansion, Color color)
    {
        spawnTime = startTime;
        spawnPosition = position;
        scaleFactor = expansion;
        spawnColor = color;
    }

    public static SpawnParameters UnstreamWave(Level level, string[] parameters)
    {
        if (parameters.Length >= 6)
        {
            float startTime;
            float X;
            float Y;
            float speed;
            if (float.TryParse(parameters[1], out startTime) && float.TryParse(parameters[2], out X) && float.TryParse(parameters[3], out Y) && float.TryParse(parameters[5], out speed))
            {
                if (level.duration < startTime)
                {
                    level.duration = startTime;
                }
                Vector2 position = new Vector2(X, Y);
                return new WaveParameters(startTime, position, speed, GetColor(parameters[4]));
            }
        }
        return null;
    }

    public override GameObject Spawn(SpawnerController spawner)
    {
        WaveController wave = (WaveController)Object.Instantiate(spawner.wavePrefab, spawnPosition, Quaternion.identity);
        wave.Setup(scaleFactor, spawnColor, spawner.GetOrderInLayer());
        return wave.gameObject;
    }
}

public class TextParameters : SpawnParameters
{
    public string content;
    public float duration;

    public TextParameters(float startTime, float endTime, string text)
    {
        spawnTime = startTime;
        content = text;
        duration = endTime - startTime;
    }

    public static SpawnParameters UnstreamText(Level level, string[] parameters)
    {
        if (parameters.Length >= 4)
        {
            float startTime;
            float endTime;
            if (float.TryParse(parameters[1], out startTime) && float.TryParse(parameters[2], out endTime))
            {
                if (level.duration < startTime)
                {
                    level.duration = startTime;
                }
                return new TextParameters(startTime, endTime, parameters[3]);
            }
        }
        return null;
    }

    public override GameObject Spawn(SpawnerController spawner)
    {
        GameManager.instance.ShowText(content, duration);
        return null;
    }
}