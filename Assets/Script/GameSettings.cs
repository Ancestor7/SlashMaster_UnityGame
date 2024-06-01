using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettings : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private Slider musicSlider = null;
    [SerializeField] private float defaultMusicVolume = 5f;
    [SerializeField] private Slider soundSlider = null;
    [SerializeField] private float defaultSoundVolume = 5f;
    [SerializeField] private Toggle notificationToggle = null;


    void Start()
    {
        DontDestroyOnLoad(this);

        /*musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        soundSlider = GameObject.Find("SoundSlider").GetComponent<Slider>();
        notificationToggle = GameObject.Find("SendNotificationToggle").GetComponent<Toggle>();

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        SetMusicVolume(defaultMusicVolume);

        soundSlider.onValueChanged.AddListener(SetSoundVolume);
        SetSoundVolume(defaultSoundVolume);*/
    }

    public void SetMusicVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("musicVolume", volume);
    }

    public void SetSoundVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("soundVolume", volume);
    }
}
