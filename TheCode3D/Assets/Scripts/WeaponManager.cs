using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {
    public List<WeaponController> startingWeapons = new List<WeaponController>();
    public Transform weaponParentSocket;
    public Transform defaultWeaponPosition;
    public Transform aimingPosition;
    public int activeWeaponIndex { get; private set; }
    private WeaponController[] weaponSlots = new WeaponController[10];

    private bool running = false;
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
        /*
        Mejor asi creo yo
        if (weaponSlots[activeWeaponIndex].isAutomatic) {
            if (Input.GetButton("Fire1"))
                TryShoot();
        } else {
            if (Input.GetButtonDown("Fire1"))
                TryShoot();
        }
        */
        if (Input.GetButton("Fire1")) {
            if (running) {
                running = false;
                weaponSlots[activeWeaponIndex].Idle();
            }
            weaponSlots[activeWeaponIndex].Shoot();
        } else {
            if (Input.GetButtonDown("Sprint")) {
                running = true;
                weaponSlots[activeWeaponIndex].Sprint();
            }
            if (Input.GetButtonUp("Sprint")) {
                running = false;
                weaponSlots[activeWeaponIndex].Idle();
            }
        }

        if (Input.GetButtonDown("Fire2")) {
            if (running) {
                running = false;
                weaponSlots[activeWeaponIndex].Idle();
            }
            weaponSlots[activeWeaponIndex].Aim();
        }

        if (Input.GetButtonUp("Fire2")) {
            if (running)
                weaponSlots[activeWeaponIndex].Sprint();
            else
                weaponSlots[activeWeaponIndex].Idle();
        }

        if (Input.GetButtonDown("Reload"))
            weaponSlots[activeWeaponIndex].Reload();
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