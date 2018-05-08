using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
