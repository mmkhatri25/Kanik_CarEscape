using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public Sound[] sfxSounds;

    public AudioSource sfxAudioSource;

    private void Start()
    {
        GameManager.Instance.OnCarEscape += OnCarEscape;
        GameManager.Instance.OnCarClick += OnCarClick;

        UIManager.Instance.OnSoundClicked += OnSoundClicked;
    }

    private void OnSoundClicked()
    {
        bool soundValue = GameManager.Instance.isSoundOn;

        sfxAudioSource.mute = !soundValue;
    }

    private void OnCarClick()
    {
        PlaySound("EngineStart");
    }

    private void OnCarEscape()
    {
        PlaySound("CoinEarned");
    }

    public void PlaySound(string name)
    {
        Sound s = Array.Find(sfxSounds, x => x.soundName == name);

        sfxAudioSource.clip = s.audioclip;

        sfxAudioSource.Play();
    }
}
