using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStatus : MonoBehaviour{

    public bool dialogLevel = false;
    public int level = 0;
    public GameObject win;

    void Start(){
        if (dialogLevel)
            EventManager.instance.DialogEndEvent.AddListener(Win); 
        }   

    void OnDisable(){
        if (dialogLevel)
            EventManager.instance.DialogEndEvent.RemoveListener(Win); 
    }
    
    public void Win(){
        Time.timeScale = 0f;
        win.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        PlayerPrefs.SetInt("level", level+1);
        Debug.Log("You win!");
    }   

}
