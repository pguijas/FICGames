using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour{

    public Slider volumeSlider;
    public Toggle fsToggle;

    public void Start(){ 
        // Volume
        volumeSlider.value = PlayerPrefs.GetFloat("volumeAudio", .5f);
        AudioListener.volume = volumeSlider.value;
        // FS
        fsToggle.isOn = Screen.fullScreen;
    }

    public void ChangeVolume(float value){
        Debug.Log(value);
        PlayerPrefs.SetFloat("volumenAudio", value);
        AudioListener.volume = volumeSlider.value;
    }

    public void ChangeFs(bool value){
        Debug.Log(value);
        Screen.fullScreen = value;
    }
}