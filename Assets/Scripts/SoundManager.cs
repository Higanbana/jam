using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour {

    public static SoundManager instance = null;
    public AudioSource soundSource;
    public AudioSource musicSource;
    public AudioSource achievementSource;
    public AudioClip[] musicClips;

	void Awake () {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        musicSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
	}

    public void PlayAchievementSound()
    {
        achievementSource.Play();
    }

    public void PlaySound (AudioClip clip)
    {
        soundSource.PlayOneShot(clip);
    }

    public void ChangeBackgroundMusic (int musicIndex, float time = 0f, bool forceloop = true)
    {
        if (musicSource.clip != musicClips[musicIndex])
        {
            SetBackgroundMusic(musicIndex, time, forceloop);
        }
    }

    public void ChangeBackgroundMusic(string musicName, float time = 0f, bool forceloop = true)
    {
        int musicIndex = 0;
        while (musicIndex < musicClips.Length && !musicClips[musicIndex].name.Equals(musicName))
        {
            musicIndex++;
        }
        if (musicIndex < musicClips.Length)
        {
            ChangeBackgroundMusic(musicIndex, time, forceloop);
        }
    }

    public void ResetBackgroundMusic ()
	{
		SetBackgroundMusic (0);
	}

    void SetBackgroundMusic(int musicIndex, float time = 0f, bool forceloop = true)
    {
        musicSource.clip = musicClips[musicIndex];
        musicSource.loop = forceloop;
        musicSource.Play();
        musicSource.time = time;
    }

    public void SetMusicAtTime (float time)
    {
        musicSource.time = time;
    }
	
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSoundVolume(float volume)
    {
        soundSource.volume = volume;
        achievementSource.volume = volume;
    }
}
