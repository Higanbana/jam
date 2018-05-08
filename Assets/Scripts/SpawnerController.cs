using UnityEngine;
using System.Collections;


public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;
    public CollectibleController collectiblePrefab;
    public CollectibleTextController collectibleTextPrefab;
    public WaveController wavePrefab;

    public Transform[] rails;

    [HideInInspector]
    public Level level;

    [HideInInspector]
    public float time = 0f;

    private int spawnIndex = 0;

    public float invFrequency;
    private AudioSource audio;
    
    void OnEnable ()
    {
        spawnIndex = 0;
        time = 0f;
        audio = SoundManager.instance.musicSource;
        invFrequency = 1f/SoundManager.instance.musicSource.clip.frequency;
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
        while (spawnIndex < level.spawns.Count && level.spawns[spawnIndex].spawnTime <= time)
        {
            spawnIndex++;
        }
    }

	void FixedUpdate ()
    {
        //time += Time.fixedDeltaTime; //old method of calculating time
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
