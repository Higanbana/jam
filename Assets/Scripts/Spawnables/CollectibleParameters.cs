using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        CollectibleTextController collectible = (CollectibleTextController)Object.Instantiate(spawner.collectibleTextPrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawnAngle, spawner.level.speed, spawner.GetOrderInLayer(), letter);

        return collectible.gameObject;
    }
}
