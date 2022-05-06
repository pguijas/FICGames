using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuantumTek.QuantumTravel;

public class GameStatus : MonoBehaviour{

    public bool dialogLevel = false;
    public int level = 0;
    public GameObject win;
    public GameObject loose;
    public GameObject dialogAtEnd;
    public QT_Map minimap;
    public AudioManager audioManager;

    void Start(){   
        if (dialogLevel | dialogAtEnd != null)
            EventManager.instance.DialogEndEvent.AddListener(Win); 
        EventManager.instance.UpdateLifeEvent.AddListener(Loose);
        EventManager.instance.NewSoldierEvent.AddListener(NewSoldier);
        EventManager.instance.DeadSoldierEvent.AddListener(DeadSoldier);
        Debug.Log("GameStatus Inicializado");
    }   

    void OnDisable(){
        if (dialogLevel)
            EventManager.instance.DialogEndEvent.RemoveListener(Win); 
    }

    public void NewSoldier(QT_MapObject soldier){
        Debug.Log("New Soldier!");
        minimap.AddMarker(soldier,false);
    }


    //usamos la lista de makers para determinar el estado del juego
    public void DeadSoldier(QT_MapObject soldier){
        // Buscamos el Maker y destruimos su instancia y lo borramos de la lista.
        for (int i = 0; i < minimap.Markers.Count; i++){
            if (minimap.Markers[i].Object == soldier){
                Destroy(minimap.Markers[i].gameObject);
                minimap.Markers.RemoveAt(i);
                break;
            }
        }
    }

    public void Win(){
        ActiveCursorAndPause();
        win.SetActive(true);
        audioManager.Stop("Theme");
        audioManager.Play("Victory");
        PlayerPrefs.SetInt("level", level+1);
        Debug.Log("You win!");
    }   

    public void Loose(float life){
        if (life <= 0){
            ActiveCursorAndPause();
            loose.SetActive(true);
            audioManager.Stop("Theme");
            audioManager.Play("GameOver");
            Debug.Log("You lose!");
        }
    }   

    private void ActiveCursorAndPause(){
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

}
