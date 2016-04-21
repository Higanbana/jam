﻿using UnityEngine;
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
	
	}

    public void PlayAchievementSound()
    {
        achievementSource.Play();
    }

    public void PlaySound (AudioClip clip)
    {
        soundSource.clip = clip;
        soundSource.Play();
    }

    public void ChangeBackgroundMusic (int musicIndex, bool forceloop)
    {
        musicSource.clip = musicClips[musicIndex];
        musicSource.loop = forceloop;
        musicSource.Play();
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
