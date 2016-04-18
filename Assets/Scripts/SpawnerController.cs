using UnityEngine;
using System.Collections;


public class SpawnParameters
{
    public float spawnTime;
    public int railIndex;
    public float railLength;

    public SpawnParameters(float time, int index, float length)
    {
        spawnTime = time;
        railIndex = index;
        railLength = length;
    }
}

public class SpawnerController : MonoBehaviour {

    public ObstacleController obstaclePrefab;

    public Transform[] rails;

    public float speed;

    private SpawnParameters[] spawnParameters;
    private int spawnIndex = 0;

    [HideInInspector]
    public float time = 0.0f;

	// Use this for initialization
	void OnEnable ()
    {
        //Test code
        spawnIndex = 0;
        time = 0;
        spawnParameters =  new SpawnParameters[3];
        spawnParameters[0] = new SpawnParameters(1f, 2, 3f);
        spawnParameters[1] = new SpawnParameters(1f, 1, 4f);
        spawnParameters[2] = new SpawnParameters(7f, 3, 6f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        time += Time.deltaTime;
        while (spawnIndex < spawnParameters.Length && spawnParameters[spawnIndex].spawnTime <= time)
        {
            int railIndex = spawnParameters[spawnIndex].railIndex;
            float height;
            float offset;
            if (railIndex == 0)
            {
                height = Mathf.Abs(rails[1].position.y - rails[0].position.y);
                offset = 0f;
            }
            else if (railIndex == rails.Length - 1)
            {
                height = Mathf.Abs(rails[rails.Length - 1].position.y - rails[rails.Length - 2].position.y);
                offset = 0f;
            }
            else
            {
                float toNext = Mathf.Abs(rails[railIndex + 1].position.y - rails[railIndex].position.y);
                float toPrevious = Mathf.Abs(rails[railIndex - 1].position.y - rails[railIndex].position.y);
                height = 0.5f * (toPrevious + toNext);
                offset = 0.5f * (toPrevious - toNext);
            }
            Vector3 obstaclePosition = new Vector3(transform.position.x + spawnParameters[spawnIndex].railLength * 0.5f, rails[railIndex].position.y + offset, 0f);
            ObstacleController obstacle = (ObstacleController)Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
            obstacle.Setup(height, spawnParameters[spawnIndex].railLength, speed);
            spawnIndex++;
        }
	}
}
