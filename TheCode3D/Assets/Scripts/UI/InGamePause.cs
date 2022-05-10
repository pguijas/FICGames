using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGamePause : MonoBehaviour{

    public static bool GameInPause = false; //esto es interesante para el audio, pero no se si realmente lo necesitaremos
    public static bool Win = false;
    public GameObject pauseMenu;
    [HideInInspector]
    public AudioManager audioManager; 

    void Awake(){
        audioManager = GameObject.Find("Levels_AudioManager").GetComponent<AudioManager>();
    }

        void Update(){
        // Al pulsar ESC
        if (Input.GetKeyDown(KeyCode.Escape)){
            if (GameInPause)
                Resume();
            else
                Pause();
        }
    }

    public void Resume(){
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameInPause = false;
        audioManager.Stop("Pause");
        if (!Win)
            audioManager.Play("Theme");
        else
            audioManager.Play("Victory");
    }

    public void Pause(){
        Cursor.lockState = CursorLockMode.None;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        GameInPause = true;
        audioManager.Pause("Theme");
        if (audioManager.isPlaying("Victory")) {
            audioManager.Pause("Victory");
            Win = true;
        }
        audioManager.Play("Pause");
    }

}
