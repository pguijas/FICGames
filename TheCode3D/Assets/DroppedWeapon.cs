using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DroppedWeapon : MonoBehaviour{

    [SerializeField]
    public float distance=4;
    public WeaponController weaponPrefab;
    private GameObject player;

    void Start(){
        player = GameObject.FindWithTag("Player");
    }

    void Update(){
        Vector3 distanceToPlayer = GameObject.FindWithTag("Player").transform.position - transform.position;
        // Si se esta lo suficiente cerca se podrá pickear el arma
        if (distanceToPlayer.magnitude < distance){
            GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "Press E to pick up the weapon";
            //Pick
            if (Input.GetKeyDown(KeyCode.E)){
                player.GetComponent<WeaponManager>().PickWeapon(weaponPrefab);
                GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "";
                Destroy(gameObject);
            }
        }


        else{
            GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    public void SetBullets(int mag, int totalBullets){
        weaponPrefab.currentMag = mag;
        weaponPrefab.bullets = totalBullets;
    }
}
