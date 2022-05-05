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

    // Update is called once per frame
    void Update(){
        Vector3 distanceToPlayer = GameObject.FindWithTag("Player").transform.position - transform.position;

        if (distanceToPlayer.magnitude < distance){
            GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "Press E to pick up the weapon";
            if (Input.GetKeyDown(KeyCode.E)){
                player.GetComponent<WeaponManager>().AddWeapon(weaponPrefab);
                GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "";
                Destroy(gameObject);
            }
        }


        else{
            GameObject.FindWithTag("short_text").GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
