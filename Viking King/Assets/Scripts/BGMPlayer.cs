using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public AudioClip titleMusic;

    void Start()
    {
        SoundManager.Instance.PlayBGM(titleMusic);
    }
}
