using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Int2Event : UnityEvent<int, int, int> { }

public class EventManager : MonoBehaviour{
    #region Singleton

    public static EventManager instance;
    private void Awake(){
        if (instance == null){
            instance = this;
        }
        else if (instance != this){
            Destroy(gameObject);
        }
    }

    #endregion

    public Int2Event UpdateBulletsEvent = new Int2Event();

    public UnityEvent DialogEndEvent = new UnityEvent();

}
