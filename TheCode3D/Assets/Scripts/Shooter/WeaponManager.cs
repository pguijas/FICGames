using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WeaponManager : MonoBehaviour {

    [Header ("Weapons")]
    [SerializeField]
    public List<WeaponController> startingWeapons = new List<WeaponController>();
    [Header ("Positions")]
    [SerializeField]
    public Transform weaponParentSocket;
    [SerializeField]
    public Transform defaultWeaponPosition;
    [SerializeField]
    public Transform aimingPosition;
    
    private int activeWeaponIndex = -1;
    private WeaponController[] weaponSlots = new WeaponController[5];


    void Start() {
        foreach (WeaponController startingWeapon in startingWeapons)
            AddWeapon(startingWeapon);
        // eliminar estas dos líneas para aparecer sin armas
        activeWeaponIndex = 0;
        weaponSlots[activeWeaponIndex].gameObject.SetActive(true);
    }

    

    // Update is called once per frame
    void Update() {
        if (activeWeaponIndex != -1) {
            WeaponController activeWeapon = weaponSlots[activeWeaponIndex];
            // Números para cambiar de arma
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
            else if (Input.GetKeyDown(KeyCode.Q))
                DropWeapon();
            

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


    // Al cambiar de arma meter una sleep para Hide y luego sacar el nuevo arma
    private void SwitchWeapon(int p_weaponIndex) {
        if (p_weaponIndex != activeWeaponIndex && p_weaponIndex >= 0) {
            if (weaponSlots[p_weaponIndex] != null) {
                if (activeWeaponIndex != -1)
                    weaponSlots[activeWeaponIndex].gameObject.SetActive(false);
                weaponParentSocket.position = defaultWeaponPosition.position;
                weaponSlots[p_weaponIndex].gameObject.SetActive(true);
                activeWeaponIndex = p_weaponIndex;
            }
        }
    }

    // Inicializar armas
    public void AddWeapon(WeaponController p_weaponPrefab) {
        // Ponermos la posición actual como la default
        weaponParentSocket.position = defaultWeaponPosition.position;
        for (int i = 0; i<weaponSlots.Length; i++) {
            // Para cada arma, la instanciamos, la desactivamos y la añadimos al array de armas
            if (weaponSlots[i] == null) {
                WeaponController weaponClone = Instantiate(p_weaponPrefab, weaponParentSocket);
                weaponClone.gameObject.SetActive(false);
                weaponSlots[i] = weaponClone;
                return;
            }
        }
    }
    

    private void DropWeapon() {
        weaponSlots[activeWeaponIndex].gameObject.SetActive(false);
        weaponSlots[activeWeaponIndex].Drop();
        Destroy(weaponSlots[activeWeaponIndex]);
        weaponSlots[activeWeaponIndex] = null;
        activeWeaponIndex = -1;

        for (int i = 0; i<weaponSlots.Length; i++) {
            if (weaponSlots[i] != null) {
                SwitchWeapon(i);
                return;
            }
        }
    }
}