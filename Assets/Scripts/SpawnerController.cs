using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;
    public CollectibleController collectiblePrefab;
    public CollectibleTextController collectibleTextPrefab;
    public WaveController wavePrefab;

    public Transform[] rails;
    public SpawnParameters[] spawns;

    public float speed;

    private int spawnIndex = 0;

    [HideInInspector]
    public float time = 0.0f;

    void OnEnable ()
    {
        spawnIndex = 0;
        time = 0f;
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

    public int GetOrderInLayer ()
    {
        return spawnIndex;
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
