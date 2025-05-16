using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public AudioClip BGM;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    void Start()
    {
        Sound.Instance.PlayBGM(BGM, bgmVolume);
    }
}
