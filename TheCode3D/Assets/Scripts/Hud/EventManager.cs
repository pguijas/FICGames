using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Int2Event : UnityEvent<int, int> { }

public class EventManager : MonoBehaviour{
    #region Singleton

    public static EventManager instance;
    private void Awake(){
        Debug.Log("EventManager.Awake()");
        if (instance == null){
            instance = this;
        }
        else if (instance != this){
            Destroy(gameObject);
        }
    }

    #endregion

    public Int2Event UpdateBulletsEvent = new Int2Event();

}
