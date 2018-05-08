using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
