using UnityEngine;
using System.Collections;

public abstract class SpawnParameters
{
    public float spawnTime;

    public abstract GameObject Spawn(SpawnerController spawner);
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

    public override GameObject Spawn(SpawnerController spawner)
    {
        float height;
        float length = spawner.speed * (stopTime - spawnTime);
        Vector3 offset = Vector3.zero;
        float deltaTime = spawnTime - spawner.time;
        offset.x = 0.5f * length + spawner.speed * deltaTime;
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
        obstacle.Setup(height, length, spawner.speed, spawnColor, spawner.GetOrderInLayer());
        return obstacle.gameObject;
    }
}

public class TriggerParameters : ObstacleParameters
{
    public TriggerParameters(float startTime, float endTime, int index, Color color) : base(startTime, endTime, index, color)
    {
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
        float offset = spawner.speed * (spawnTime - spawner.time);
        Vector3 collectiblePosition = new Vector3(spawner.transform.position.x + offset, spawner.rails[railIndex].position.y, 0f);
        CollectibleController collectible = (CollectibleController)Object.Instantiate(spawner.collectiblePrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawnAngle, spawner.speed, spawner.GetOrderInLayer());

        return collectible.gameObject;
    }

    GameObject SpawnCollectibleText(SpawnerController spawner, string letter)
    {
        float offset = spawner.speed * (spawnTime - spawner.time);
        Vector3 collectiblePosition = new Vector3(spawner.transform.position.x + offset, spawner.rails[railIndex].position.y, 0f);
        CollectibleTextController collectible = (CollectibleTextController) Object.Instantiate(spawner.collectibleTextPrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawnAngle, spawner.speed, spawner.GetOrderInLayer(), letter);

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

    public override GameObject Spawn(SpawnerController spawner)
    {
        WaveController wave = (WaveController)Object.Instantiate(spawner.wavePrefab, spawnPosition, Quaternion.identity);
        wave.Setup(scaleFactor, spawnColor, spawner.GetOrderInLayer());
        return wave.gameObject;
    }
}
