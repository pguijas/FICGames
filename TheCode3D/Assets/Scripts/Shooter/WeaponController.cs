using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class WeaponController : MonoBehaviour{

    // Sonidos
    public AudioSource shootSound;
    public AudioSource reloadSound;

    [Header ("Model Dopeable")]
    [SerializeField]
    // Modelo del arma que es el que se tira al suelo
    public GameObject dropeableModel;
    [Header ("Weapon Animations")]
    [SerializeField]
    private ParticleSystem shootingSystem;
    [SerializeField]
    private Transform bulletSpawnPoint;
    [SerializeField]
    private ParticleSystem impactParticleSystem;
    [SerializeField]
    private TrailRenderer bulletTrail;
    
    [Header ("Gun Settings")]
    [SerializeField]
    // Cambia el arma que muestra el hud
    public int typegun = 1;
    [SerializeField]
    public float damage = 30f;
    [SerializeField]
    public int magSize = 30;
    [SerializeField]
    public int currentMag = 30;
    [SerializeField]
    public int maxbullets = 3000;
    [SerializeField]
    public int bullets = 3000;
    [SerializeField]
    private bool isAutomatic = true;

    [Header ("Shoot Settings")]
    [SerializeField]
    private float shootDelay = 0.5f;
    [SerializeField]
    // Recoil sin apuntar
    public float recoilForce = 4f;
    [SerializeField]
    // Recoil apuntando
    public float recoilCorrection = 2f;
    [SerializeField]
    private float bulletSpeed = 350f;
    [SerializeField]
    public Vector3 aimcorrection = new Vector3(0f, 0f, 0f);
    
    [Header ("Bullet Spreading")]
    [SerializeField]
    private bool addBulletSpread = true;
    [SerializeField]
    private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    
    [SerializeField]
    private LayerMask mask;
    
    private Animator animator;
    private float lastShootTime;
    private Vector3 originPosition;

    // flags
    private bool isReloading = false;
    private bool isAiming = false;
    private bool isSprinting = false;


    private void Start(){
        originPosition = transform.localPosition;
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun, currentMag, bullets);
    }


    private void Awake(){
        animator = GetComponent<Animator>();
    }

    private void OnEnable(){
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun, currentMag, bullets);
    }


    public bool IsAutomatic() {
        return isAutomatic;
    }

    public void SetMaxAmmo(bool notify = false) {
        bullets = maxbullets;
        if (notify)
            EventManager.instance.UpdateBulletsEvent.Invoke(typegun, currentMag, bullets);
    }


    public void Shoot() {
        // comprobaciones para disparar o para recargar
        if ((lastShootTime + shootDelay < Time.time) && (currentMag > 0) && !isReloading) {
            // el disparo se realiza en una corrutina
            StartCoroutine(ShootCorroutine());
        } else if (currentMag == 0)
            Reload();
    }


    private IEnumerator ShootCorroutine(){
        // animaci??n de fogueo y sonido
        shootingSystem.Play();
        shootSound.Play();
        // c??lculo de la direcci??n de disparo del jugador
        Vector3 direction = GetDirection();
        // IMPLEMENTACI??N CON SPHERECAST -> al final ya lo hace Raycast por debajo
        /*
        // Trazamos un Spherecast para detectar colisiones y los a??adimos a la capa shooting
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, 500, Mask);
        LayerMask[] layers = new LayerMask[rangeChecks.Length];
        if (rangeChecks.Length != 0) {
            for (int i = 0; i < rangeChecks.Length; i++) {
                layers[i] = rangeChecks[i].gameObject.layer;
                rangeChecks[i].gameObject.layer = shootingLayer;
                GameObject target = rangeChecks[i].gameObject;
                Vector3 directionToTarget = (GameObject.FindWithTag("MainCamera").transform.position - target.position).normalized;
                if (Vector3.Angle(GameObject.FindWithTag("MainCamera").transform.forward, directionToTarget) < 60 / 2) {
                    target.layer = shootingLayer;
                }
            }
            // Si el raycast impacta, el trail se renderiza hasta el punto de impacto
            if (Physics.Raycast(GameObject.FindWithTag("MainCamera").transform.position, direction, out RaycastHit hit, float.MaxValue, shootingLayer)) {
                StartCoroutine(SpawnTrail(hit));
            } else {
                StartCoroutine(SpawnTrail(direction));
            }
            for (int i = 0; i < rangeChecks.Length; i++) {
                rangeChecks[i].gameObject.layer = layers[i];
            }
            // Si no impacta, lo renderizamos desde la boquilla en l??nea recta + dispersi??n una determinada distancia
        }*/
        // Trazamos un rayo desde la c??mara en la direcci??n a la que esta apuntando el jugador, en las m??scaras de enemigos y obst??culos
        if (Physics.Raycast(GameObject.FindWithTag("MainCamera").transform.position, direction, out RaycastHit hit, float.MaxValue, mask)){
            // Si el raycast impacta, el trail se renderiza hasta el punto de impacto
            StartCoroutine(SpawnTrail(hit));
        } else {
            // Si no impacta, lo renderizamos desde la boquilla en l??nea recta + dispersi??n una determinada distancia
            StartCoroutine(SpawnTrail(direction));
        }
        lastShootTime = Time.time;
        currentMag -= 1;
        //Actualizamos Hud
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun, currentMag, bullets);
        // Fuerza de retroceso en el arma
        AddRecoil();
        yield return null;
    }
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection() {
        Vector3 direction = transform.forward;
        if (addBulletSpread) {
            // A??adimos una dispersi??n aleatoria al vector de direcci??n, 
            // si estamos apuntando
            if (!isAiming) {
                direction += new Vector3(
                    Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                    Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                    Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z)
                );
                direction.Normalize();
            }
        }
        return direction;
    }

    // Spawnear el trail desde el origen al punto de impacto
    private IEnumerator SpawnTrail(RaycastHit hit) {
        float time = 0;
        // Creamos un objeto que animar?? nuestros disparos
        TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        float distance = hit.distance;
        // un impacto q este a BulletSpeed m -> 1 segundo
        // distancia/velocidad = tiempo
        // spawneamos la bala un determinado tiempo
        while (time < distance/bulletSpeed) {
            // movimiento de la bala
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += bulletSpeed*Time.deltaTime/distance;
            yield return null;   
        }
        trail.transform.position = hit.point;
        // despu??s del tiempo necesario para llegar al punto de impacto
        // comprobamos si hemos impactado con un enemigo
        SoldierPart soldier = hit.transform.GetComponent<SoldierPart>();
        if (soldier != null)
            // si impactamos con uno, le quitamos salud
            soldier.DoDamage(damage);
        else
            // si no, instanciamos un animaci??n de impacto en un obst??culo
            Instantiate(impactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(trail.gameObject, trail.time);
    }

    // Spawnear el trail desde el origen una determinada distancia hacia delante
    // para dar sensaci??n de disparo hasta el infinito
    private IEnumerator SpawnTrail(Vector3 direction) {
        float time = 0;
        TrailRenderer trail = Instantiate(bulletTrail, bulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        // precalculamos la distancia a la que llegar?? la bala
        Vector3 endPosition = startPosition + direction * 100;
        float distance = Vector3.Distance(startPosition, endPosition);
        // renderizamos el efecto un determinado tiempo y despu??s lo destruimos
        while (time < distance/bulletSpeed) {
            trail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += bulletSpeed*Time.deltaTime/distance;
            yield return null;   
        }
        trail.transform.position = endPosition;
        Destroy(trail.gameObject, trail.time);
    }

    // Esta funci??n nos servir?? para saber si se est?? ejecutando una animaci??n de disparo
    // para evitar realizar acciones que no se pueden, por ejemplo correr + apuntar.
    public bool IsAnimPlaying(string animation) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
    }

    // Funci??n para a??adir una fuerza de retroceso al arma
    private void AddRecoil() {
        if (isAiming)
            transform.Rotate(-recoilCorrection, 0f, 0f);
        else 
            transform.Rotate(-recoilForce, 0f, 0f);
    }

    // Funci??n global para realizar la recarca
    public void Reload() {
        // comprobamos precondiciones
        if (isReloading == true || currentMag == magSize || bullets == 0)
            return;
        // iniciamos la animaci??n de recarga y el sonido
        reloadSound.Play();
        animator.SetTrigger("Reload");
        // actualizamos hud y flags
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,-1,bullets);
        isReloading = true;
        // activamos una corrutina para actualizar el n??mero de balas cuando se acabe la animaci??n
        StartCoroutine(AfterReload());
    }

    // Corrutina para activar la recarga una vez terminada la animaci??n
    private IEnumerator AfterReload() {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        if (bullets >= magSize) {
            int load = magSize - currentMag%magSize;
            bullets -= load;
            currentMag += load;
        } else {
            int tmp = bullets + currentMag;
            if (tmp > magSize) {
                bullets = tmp % magSize;
                currentMag = magSize;
            } else {
                currentMag = tmp;
                bullets = 0;
            }
        }
        isReloading = false;
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun, currentMag, bullets);
    }


    public bool IsReloading() {
        return isReloading;
    }

    // Funci??n para realizar el apuntado
    public void Aim() {
        if (!isAiming){
            // la corrutina realiza toda la animaci??n
            StartCoroutine(AimAnimation(originPosition, originPosition + aimcorrection));
            isAiming = true;
        }
    }


    // Animaci??n de apuntado
    private IEnumerator AimAnimation(Vector3 origin, Vector3 target){
        float desiredDuration = .1f; //meterlo en otro lado
        float time = 0;
        // vamos moviendo el arma hasta llegar a la posici??n de apuntado
        while (time < desiredDuration) {
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, target, time/desiredDuration);
            yield return null;
        }
    }


    public bool IsAiming() {
        return isAiming;
    }

    // Desactivar cualquier animaci??n
    public void Idle() {
        // si se est?? apuntando, dejamos de apuntar
        if (isAiming) {
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            isAiming = false;
        }
        // desactivamos los flags de correr, y no dejamos correr a nuestro jugador
        animator.SetBool("Sprint",false);
        isSprinting = false;
        GameObject player = GameObject.FindWithTag("Player");
        player.GetComponent<PlayerController>().StopSprint();
    }

    // Sprint
    public void Sprint() {
        // animaci??n de sprint
        animator.SetBool("Sprint",true);
        isSprinting = true;
        GameObject player = GameObject.FindWithTag("Player");
        // derivamos el sprint en el jugador
        player.GetComponent<PlayerController>().Sprint();
    }


    public bool IsSprinting() {
        return isSprinting;
    }

    // Dropeo de un arma
    public void Drop() {
        // Actualizamos las balas en el arma a dropear
        dropeableModel.GetComponent<DroppedWeapon>().SetBullets(currentMag,bullets);
        // Instanciamos el modelo dropeado
        GameObject drop = Instantiate(dropeableModel, transform.position, Quaternion.identity); 
        drop.GetComponent<Rigidbody>().AddForce(GameObject.FindWithTag("Player").transform.forward * 10f, ForceMode.Impulse);
        drop.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 10f, ForceMode.Impulse);
        // eliminamos nuestra arma de las disponibles
        Destroy(gameObject);
    }
}