using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SC_Settings : MonoBehaviour
{
    public Slider _musicVol;
    public Slider _sfxVol;

    private AudioSource _music;

    private void Start()
    {
        // Adding event listener to the slider
        _music = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<AudioSource>();
        _musicVol.onValueChanged.AddListener(SetMusicVol);
    }

    // Changes the music volume when the slider is changed
    private void SetMusicVol(float value)
    {
        _music.volume = value / 10f;
    }
}
