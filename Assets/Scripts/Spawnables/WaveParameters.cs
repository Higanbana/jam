using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
