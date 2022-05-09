using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// no se puede dropar, solo se puede recoger

public class WeaponManager : MonoBehaviour {

    [Header ("Weapons")]
    [SerializeField]
    public WeaponController[] startingWeapons = new WeaponController[2];
    [Header ("Positions")]
    [SerializeField]
    public Transform weaponParentSocket;
    
    private int activeWeaponIndex = 0;
    private WeaponController[] weaponSlots = new WeaponController[2];


    private void Start() {
        // Agregar armas iniciales
        foreach (WeaponController startingWeapon in startingWeapons)
            AddWeapon(startingWeapon);
        // Activamos arma
        weaponSlots[activeWeaponIndex].gameObject.SetActive(true);
    }

    private void Update() {
        if (activeWeaponIndex != -1) {
            WeaponController activeWeapon = weaponSlots[activeWeaponIndex];
            // Números para cambiar de arma
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SwitchWeapon(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SwitchWeapon(1);
          
            
            if (!activeWeapon.IsReloading()) {
                // Lógica de apuntado
                // Si estamos corriendo, paramos y apuntamos
                if (Input.GetButton("Fire2")) {
                    if (activeWeapon.IsSprinting())
                        activeWeapon.Idle();
                    activeWeapon.Aim();
                } else if (Input.GetButtonUp("Fire2"))
                    activeWeapon.Idle();
                // Lógica de disparo
                // Si se dispara, no se corre
                if (activeWeapon.IsAutomatic() ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1")) {
                    if (activeWeapon.IsSprinting())
                        activeWeapon.Idle();
                    if (!activeWeapon.IsAnimPlaying("Run"))
                        activeWeapon.Shoot();
                } else {
                    // Lógica de recarga
                    // No se puede recargar mientras se corre
                    if (Input.GetButtonDown("Reload")) {
                        if (activeWeapon.IsSprinting())
                            activeWeapon.Idle();
                        activeWeapon.Reload();
                    } else { 
                        // Si no estamos apuntando ni recargando podemos correr
                        if (Input.GetButton("Sprint") && !activeWeapon.IsAiming()) 
                            activeWeapon.Sprint();
                        else if (Input.GetButtonUp("Sprint")) 
                            activeWeapon.Idle();
                    }
                }
                
            } else {
                // Si está recargando, no se puede apuntar
                if (activeWeapon.IsAiming())
                    activeWeapon.Idle();
            }
        }
    }



    // Inicializar armas
    private int AddWeapon(WeaponController p_weaponPrefab) {
        // Ponermos la posición actual como la default
        for (int i = 0; i<weaponSlots.Length; i++) {
            // Para cada arma, la instanciamos, la desactivamos y la añadimos al array de armas
            if (weaponSlots[i] == null) {
                WeaponController weaponClone = Instantiate(p_weaponPrefab, weaponParentSocket);
                weaponClone.gameObject.SetActive(false);
                weaponSlots[i] = weaponClone;
                return i;
            }
        }
        return -1;
    }
    
    
    public void PickWeapon(WeaponController p_weaponPrefab) {
        // Ponermos la posición actual como la default
        if (weaponSlots[activeWeaponIndex]==null) 
            weaponSlots[0] = Instantiate(p_weaponPrefab, weaponParentSocket);
        else {
            // Tratamos de introducirla en un slot vacío
            int slot = AddWeapon(p_weaponPrefab);
            if (slot != -1) {
                SwitchWeapon(slot);
            } else {   
                weaponSlots[activeWeaponIndex].Drop();
                weaponSlots[activeWeaponIndex] = Instantiate(p_weaponPrefab, weaponParentSocket);
            }
        }
    }


    private void SwitchWeapon(int p_weaponIndex) {
        if (p_weaponIndex != activeWeaponIndex && p_weaponIndex >= 0) {
            if (weaponSlots[p_weaponIndex] != null) {
                if (activeWeaponIndex != -1)
                    weaponSlots[activeWeaponIndex].gameObject.SetActive(false);
                weaponSlots[p_weaponIndex].gameObject.SetActive(true);
                activeWeaponIndex = p_weaponIndex;
            }
        }
    }

    public void SetMaxAmmo(){
        foreach (WeaponController weapon in weaponSlots) {
            if (weapon != null) 
                weapon.SetMaxAmmo();
        }
    }

}