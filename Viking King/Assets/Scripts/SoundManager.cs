using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

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
        if (bgmSource.clip == newClip) return; // 이미 같은 곡이면 무시
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
