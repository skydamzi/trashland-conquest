using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    public static Sound Instance;

    public AudioSource bgmSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(AudioClip newClip, float volume = 1f)
    {
        if (bgmSource.clip == newClip) return; // Prevent reloading the same clip
        bgmSource.clip = newClip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip, float volume = 0.5f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }
    public void PlaySFXWithPitch(AudioClip clip, float pitch = 1f, float volume = 0.5f)
    {
        sfxSource.pitch = pitch;
        sfxSource.clip = clip;
        sfxSource.volume = volume;
        sfxSource.Play();
    }
}
