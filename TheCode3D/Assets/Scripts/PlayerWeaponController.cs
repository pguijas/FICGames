using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour {
    public List<WeaponController> startingWeapons = new List<WeaponController>();
    public Transform weaponParentSocket;
    public Transform defaultWeaponPosition;
    public Transform aimingPosition;
    public int activeWeaponIndex { get; private set; }
    private WeaponController[] weaponSlots = new WeaponController[10];


    //comentar más el código



    // Start is called before the first frame update
    void Start() {
        activeWeaponIndex = -1;
        foreach (WeaponController startingWeapon in startingWeapons)
            AddWeapon(startingWeapon);
        // eliminar estas dos líneas para aparecer sin armas
        activeWeaponIndex = 0;
        weaponSlots[activeWeaponIndex].gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchWeapon(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SwitchWeapon(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            SwitchWeapon(4);

        // DIFERENCIAR AUTOMÄTICO / MANUAL
        //if (Input.GetButtonDown("Fire1")){
        //            TryShoot();
        //}
        if (Input.GetButton("Fire1"))
            weaponSlots[activeWeaponIndex].Shoot();

        if (Input.GetButtonDown("Fire2"))
            weaponSlots[activeWeaponIndex].Aim();

        if (Input.GetButtonUp("Fire2"))
            weaponSlots[activeWeaponIndex].Idle();

        if (Input.GetButtonDown("Reload"))
            weaponSlots[activeWeaponIndex].Reload();

        if (Input.GetButtonDown("Sprint"))
            weaponSlots[activeWeaponIndex].Sprint();
        
        if (Input.GetButtonUp("Sprint"))
            weaponSlots[activeWeaponIndex].Idle();
    }


    // Al cambiar de arma meter una sleep para Hide y luego sacar el nuevo arma

    //esto cascaría si se tienen dos armas solo, comprobar que el índice sea válido
    private void SwitchWeapon(int p_weaponIndex) {
        if (p_weaponIndex != activeWeaponIndex && p_weaponIndex >= 0) {
            if (activeWeaponIndex != -1) //esto x ejemplo xq está, por si no hayarmas?
                weaponSlots[activeWeaponIndex].gameObject.SetActive(false);
            weaponParentSocket.position = defaultWeaponPosition.position;
            weaponSlots[p_weaponIndex].gameObject.SetActive(true);
            activeWeaponIndex = p_weaponIndex;
        }
    }

    private void AddWeapon(WeaponController p_weaponPrefab) {
        weaponParentSocket.position = defaultWeaponPosition.position;
        for (int i = 0; i<weaponSlots.Length; i++) {
            if (weaponSlots[i] == null) {
                WeaponController weaponClone = Instantiate(p_weaponPrefab, weaponParentSocket);
                weaponClone.gameObject.SetActive(false);
                weaponSlots[i] = weaponClone;
                return;
            }
        }
    }
}