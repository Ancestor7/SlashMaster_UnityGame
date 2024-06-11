using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    // UNDONE: Broken Settings
    [Header("Game Settings")]
    [SerializeField] private Slider musicSlider = null;
    [SerializeField] private float defaultMusicVolume = 1f;
    [SerializeField] private Slider soundSlider = null;
    [SerializeField] private float defaultSoundVolume = 1f;
    //[SerializeField] private Toggle notificationToggle = null;


    void Start()
    {
        musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(musicSlider.value); });
        SetMusicVolume(defaultMusicVolume);
        soundSlider.onValueChanged.AddListener(delegate { SetSoundVolume(soundSlider.value); });
        SetSoundVolume(defaultSoundVolume);
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSoundVolume(float volume)
    {
        PlayerPrefs.SetFloat("soundVolume", volume);
    }
}
