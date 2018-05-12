using UnityEngine;
using System.Collections;


public class SpawnerController : MonoBehaviour {

    [Header("Prefabs")]

    public ObstacleController obstaclePrefab;
    public CollectibleController collectiblePrefab;
    public CollectibleTextController collectibleTextPrefab;
    public WaveController wavePrefab;

    [Header("Rails")]

    public Transform[] rails;

    [HideInInspector]
    public Level level;

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

    public void StartLevel(Level newLevel, float startTime)
    {
        transform.gameObject.SetActive(true);
        level = newLevel;
        time = startTime;
        spawnIndex = 0;

        // Spawn objects that should be displayed on screen at startTime
        while (spawnIndex < level.spawns.Count && level.spawns[spawnIndex].spawnTime < time-level.deltaTime-5)
        {
            spawnIndex++;
        }
        FixedUpdate();
    }

	void FixedUpdate ()
    {
        time += Time.deltaTime;

        while (spawnIndex < level.spawns.Count && level.spawns[spawnIndex].spawnTime <= time)
        {
            level.spawns[spawnIndex].Spawn(this);
            spawnIndex++;
        }
        if (time >= level.duration)
        {
            StartCoroutine(GameManager.instance.EndLevel());
        }
    }
}
