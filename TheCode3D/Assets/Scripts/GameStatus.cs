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
    private bool dialogDone = true;

    void Start(){   
        if (dialogLevel | dialogAtEnd != null){
            dialogDone = false;
            EventManager.instance.DialogEndEvent.AddListener(Dialog); 
            // Nos aseguramos de que el diálogo esté desactivado
            if (dialogAtEnd != null)
                dialogAtEnd.SetActive(false);
        }
        EventManager.instance.UpdateLifeEvent.AddListener(Loose);
        EventManager.instance.NewSoldierEvent.AddListener(NewSoldier);
        EventManager.instance.DeadSoldierEvent.AddListener(DeadSoldier);
    }   

    void OnDisable(){
        if (dialogLevel)
            EventManager.instance.DialogEndEvent.RemoveListener(Win); 
    }


    public void Dialog(){
        dialogDone = true;
        Win();
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

        // Si no quedan soldados, ganamos
        if (minimap.Markers.Count == 0){
            Win();
        }
    }

    public void Win(){
        if (dialogDone){
            // Victoria
            ActiveCursorAndPause();
            win.SetActive(true);
            audioManager.Stop("Theme");
            audioManager.Play("Victory");
            if (PlayerPrefs.GetInt("level",-1) < level+1)
                PlayerPrefs.SetInt("level", level+1);
            Debug.Log("You win!");
        } else {
            // Para los niveles con diálogo al final
            if (dialogAtEnd != null){
                dialogAtEnd.SetActive(true);
            }
        }
    }   

    public void Loose(float life){
        if (life <= 0){
            /*ActiveCursorAndPause();
            loose.SetActive(true);
            audioManager.Stop("Theme");
            audioManager.Play("GameOver");
            */Debug.Log("You lose!");
        }
    }   

    private void ActiveCursorAndPause(){
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
    }

}
