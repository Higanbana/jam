using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	public Transform[] rails;
	public GameObject playerPrefab;
	private GameObject player;
	public int maxRailNumber = 3;
	private int currentRail = 2;

	// Use this for initialization
	void Awake ()
	{
		if (instance == null) {
			instance = this;
		}
		else if (instance != this) 
		{
			Destroy (gameObject);
		}
	}

	public void StartGame()
	{
		player = Instantiate (playerPrefab, rails[currentRail].position, Quaternion.identity) as GameObject;
	}

    public void QuitGame()
    {
        Application.Quit();
    }

	// Update is called once per frame
	void Update () 
	{
		if (player) {
			if (Input.GetButtonDown("Up"))
			{
				int targetRail = Mathf.Max (currentRail - 1, (5 - maxRailNumber) / 2);
				player.transform.position = rails [targetRail].position;
				currentRail = targetRail;
			}
			if (Input.GetButtonDown("Down")) 
			{
				int targetRail = Mathf.Min (currentRail + 1, 3 + (maxRailNumber - 3)/2);
				player.transform.position = rails [targetRail].position;
				currentRail = targetRail;
			}
			if (Input.GetButtonDown ("Swap"))
			{
				player.GetComponent<PlayerController> ().SwapColor ();
			}
		}

	}
}
