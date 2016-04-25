using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;
    public CollectibleController collectiblePrefab;
    public CollectibleTextController collectibleTextPrefab;
    public WaveController wavePrefab;

    public Transform[] rails;
    public SpawnParameters[] spawns;

    [HideInInspector]
    public float speed = 1f;

    [HideInInspector]
    public float time = 0f;

    private int spawnIndex = 0;

    void OnEnable ()
    {
        spawnIndex = 0;
        time = 0f;
    }

    public int GetOrderInLayer ()
    {
        return spawnIndex;
    }

    public void SetSpawns(SpawnParameters[] newSpawns)
    {
        spawns = newSpawns;
    }

    public void SetTime (float newTime)
    {
        time = newTime;
        int newIndex = 0;
        while (newIndex < spawns.Length && spawns[newIndex].spawnTime <= time)
        {
            newIndex++;
        }
        spawnIndex = newIndex;
    }

	void FixedUpdate ()
    {
        time += Time.fixedDeltaTime;
        while (spawnIndex < spawns.Length && spawns[spawnIndex].spawnTime <= time)
        {
            spawns[spawnIndex].Spawn(this);
            spawnIndex++;
        }
        if(spawnIndex == spawns.Length)
        {
            StartCoroutine(GameManager.instance.EndLevel());
        }
	}
}
