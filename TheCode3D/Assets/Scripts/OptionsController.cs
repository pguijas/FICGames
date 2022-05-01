using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Revisar Tema de PlayerPrefs
/// </summary>


public class OptionsController : MonoBehaviour{

    public Slider volumeSlider;
    public Toggle fsToggle;
    public TMP_Dropdown resDropdown;
    private Resolution[] resolutions;
    public TMP_Dropdown qualityDropdown;
    public int quality = 3;

    public void Start(){ 
        // Volume
        volumeSlider.value = PlayerPrefs.GetFloat("volumeAudio", .5f);
        AudioListener.volume = volumeSlider.value;
        // FS
        fsToggle.isOn = Screen.fullScreen;
        // Resolution
        CheckResolution();
        // Quality
        quality = PlayerPrefs.GetInt("quality", quality);
        qualityDropdown.value = quality;
        ChangeQuality(quality);
    }

    public void ChangeVolume(float value){
        Debug.Log(value);
        PlayerPrefs.SetFloat("volumenAudio", value);
        AudioListener.volume = volumeSlider.value;
    }


    ///////////////////////
    //    Resolutions    //
    ///////////////////////

    private void CheckResolution(){
        resolutions = Screen.resolutions;
        resDropdown.ClearOptions();
        List<string> stringResList = new List<string>();

        int actualRes = 0;
        bool foundActualRes = false;
        foreach (Resolution res in resolutions) { 
            string strRes = res.width +  " x " + res.height;
            stringResList.Add(strRes);

            if (!foundActualRes)
                if (Screen.fullScreen && res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                    foundActualRes = true;
                else
                    actualRes++;

        }

        // Adding to DropDown
        resDropdown.AddOptions(stringResList);
        resDropdown.value = actualRes;
        resDropdown.RefreshShownValue();

        // Cargamos Resolución (NO SE GUARDAN LOS PLAYER PREFS!)
        int prefRes = PlayerPrefs.GetInt("resolution", -1);
        if (prefRes!=-1){
            ChangeResolution(prefRes);
            resDropdown.value = prefRes;
        } 

    }

    public void ChangeResolution(int index){
        PlayerPrefs.SetInt("resolution", index);
        Resolution resolucion = resolutions[index];
        Screen.SetResolution(resolucion.width, resolucion.height, Screen.fullScreen);
    }

    public void ChangeFs(bool value){
        Screen.fullScreen = value;
    }


    ///////////////////
    //    Quality    //
    ///////////////////
    public void ChangeQuality(int index){
        PlayerPrefs.SetInt("quality", index);
        QualitySettings.SetQualityLevel(index);
    }
}