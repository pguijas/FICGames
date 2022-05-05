using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SoldierIA : MonoBehaviour {
    
    [SerializeField]
    public Animator anim;
    [SerializeField]
    public float health = 100f;
    [SerializeField]
    public EnemyWeaponController weapon;
    [SerializeField]
    public PlayerController player;

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
        anim.SetInteger("Status_walk", 0);
    }

    // Update is called once per frame
    void Update() {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Attacking"))
            weapon.Shoot();
    }


    public void TakeDamage(float damage) {
        if (health - damage <= 0) {
            health = 0;
            Die();
        } else {
            health -= damage;
            anim.SetInteger("Status_walk", 2);
            anim.SetInteger("Status_stg44", 2);
        }
    }


    private void Die() {
        Destroy(gameObject);
    }
}
