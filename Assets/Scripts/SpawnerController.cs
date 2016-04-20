using UnityEngine;
using System.Collections;

public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;
    public CollectibleController collectiblePrefab;

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

	void FixedUpdate ()
    {
        time += Time.fixedDeltaTime;// Time.deltaTime;
        while (spawnIndex < spawns.Length && spawns[spawnIndex].spawnTime <= time)
        {
            if (spawns[spawnIndex].spawnType.Contains("O"))
            {
                SpawnObstacle();
            }
            else if (spawns[spawnIndex].spawnType.Contains("C"))
            {
                SpawnCollectible();
            }
            else if (spawns[spawnIndex].spawnType.Contains("S"))
            {
                SpawnTrigger();
            }
            spawnIndex++;
        }
        if(spawnIndex == spawns.Length)
        {
            StartCoroutine(GameManager.instance.EndLevel());
        }
	}

    GameObject SpawnObstacle()
    {
        int railIndex = spawns[spawnIndex].railIndex;
        float height;
        Vector3 offset = Vector3.zero;
        offset.x = spawns[spawnIndex].railLength * 0.5f + speed * (spawns[spawnIndex].spawnTime - time);
        if (railIndex == 0)
        {
            height = Mathf.Abs(rails[1].position.y - rails[0].position.y);
        }
        else if (railIndex == rails.Length - 1)
        {
            height = Mathf.Abs(rails[rails.Length - 1].position.y - rails[rails.Length - 2].position.y);
        }
        else
        {
            float toNext = Mathf.Abs(rails[railIndex + 1].position.y - rails[railIndex].position.y);
            float toPrevious = Mathf.Abs(rails[railIndex - 1].position.y - rails[railIndex].position.y);
            height = 0.5f * (toPrevious + toNext);
            offset.y = 0.5f * (toPrevious - toNext);
        }
        Vector3 obstaclePosition = new Vector3(transform.position.x, rails[railIndex].position.y, 0f) + offset;
        Color obstacleColor;
        if(spawns[spawnIndex].spawnColor.Contains("W"))
        {
            obstacleColor = Color.white;
        }
        else
        {
            obstacleColor = Color.black;
        }
        ObstacleController obstacle = (ObstacleController)Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
        obstacle.Setup(height, spawns[spawnIndex].railLength, speed, obstacleColor);
        return obstacle.gameObject;
    }

    GameObject SpawnCollectible()
    {
        int railIndex = spawns[spawnIndex].railIndex;
        Vector3 collectiblePosition = new Vector3(transform.position.x, rails[railIndex].position.y , 0f);
        CollectibleController collectible = (CollectibleController)Instantiate(collectiblePrefab, collectiblePosition, Quaternion.identity);
        collectible.Setup(spawns[spawnIndex].spawnAngle, speed);
        return collectible.gameObject;
    }

    void SpawnTrigger()
    {
        SpawnObstacle().AddComponent<TriggerObstacleController>();
    }
}
