using UnityEngine;
using TMPro;

public class Hud : MonoBehaviour{

    public TMP_Text currentBullets;
    public TMP_Text totalBullets;

    private void Start(){
        EventManager.instance.UpdateBulletsEvent.AddListener(UpdateBullets); // No necesario disable puesto que el hud se desactiva al salir de la escena
    }

    public void UpdateBullets(int current, int total){
        // Current
        if (current == -1)
            currentBullets.text = "--";
        else
            currentBullets.text = current.ToString();

        // Total
        if (total == 0)
            totalBullets.color = Color.red;
        else
            totalBullets.color = Color.white;

        totalBullets.text = total.ToString();
    }

}
