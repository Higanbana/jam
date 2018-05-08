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