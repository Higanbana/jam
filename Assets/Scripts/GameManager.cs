using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    public GameObject player;
    public GameObject spawner;
    public int gamePaused = -1; // -1 = not started, 0 = started, 1 = paused
    public GameObject pauseCanvas;
    

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
        gamePaused = 0;
        SoundManager.instance.ChangeBackgroundMusic(2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Update()
    {
        // Pause game with esc button
        if (Input.GetButtonDown("Cancel"))
        {
            if (gamePaused == 0)
            {
                Time.timeScale = 0;
                gamePaused = 1;
                pauseCanvas.SetActive(true);
                SoundManager.instance.musicSource.Pause();
            }

            else if (gamePaused == 1)
            {
                Time.timeScale = 1;
                gamePaused = 0;
                pauseCanvas.SetActive(false);
                SoundManager.instance.musicSource.Play();
            }
            
        }


    }
}
