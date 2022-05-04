using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SoldierIA : MonoBehaviour {
    
    [SerializeField]
    public Animator anim;

    private GameObject MP40;
    private GameObject STG44;
    // Start is called before the first frame update
    void Start() {
        if (gameObject.tag == "wehrmacht_b") {
            GameObject WeaponPath = GameObject.Find(gameObject.name + "/Wehrmacht_soilder_B");
            MP40 = WeaponPath.transform.Find("MP40").gameObject;
        }
        if (gameObject.tag == "schutzstaffel_b") {
            GameObject WeaponPath = GameObject.Find(gameObject.name + "/SchutzStaffel_B");
            STG44 = WeaponPath.transform.Find("STG44").gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
