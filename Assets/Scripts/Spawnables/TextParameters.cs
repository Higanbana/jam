using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
