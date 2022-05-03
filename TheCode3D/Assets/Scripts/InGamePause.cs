using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePause : MonoBehaviour{

    public static bool GameInPause = false; //esto es interesante para el audio, pero no se si realmente lo necesitaremos
    public GameObject pauseMenu;

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)){ //hardcodeada la tecla
            if (GameInPause)
                Resume();
            else
                Pause();
        }
    }

    public void Resume(){
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameInPause = false;
    }

    public void Pause(){
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameInPause = true;
    }



}
