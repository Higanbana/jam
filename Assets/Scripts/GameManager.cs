using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;

	public static GameManager instance = null;

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
		player.SetActive(true);
        spawner.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
