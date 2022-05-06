using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class HealthBlood : MonoBehaviour{

    private Image bloodimg;
    private float maxHealth = 100f;


    private void Start(){
        EventManager.instance.UpdateLifeEvent.AddListener(UpdateBlood);
        bloodimg = GetComponent<Image>();
        Debug.Log("Blood Inicializado");
    }


    private void UpdateBlood(float life){
        Color color = bloodimg.color;
        color.a = 1 - (life / maxHealth);
        bloodimg.color = color;
    }
}
