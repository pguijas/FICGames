using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class WeaponController : MonoBehaviour{
    //esta parte de aqui esta basatnte mal -> arreglarla 
    // REVISAR LOS NOMBRES DE LAS VARIABLES (empiezan mayúsculas)

    public AudioSource shootSound;
    public AudioSource reloadSound;

    [Header ("Model Dopeable")]
    [SerializeField]
    public GameObject dropeableModel;
    [Header ("Weapon Animations")]
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    
    [Header ("Gun Settings")]
    [SerializeField]
    public int typegun = 1;
    [SerializeField]
    public float Damage = 30f;
    [SerializeField]
    public int MagSize = 30;
    [SerializeField]
    public int currentMag = 30;
    [SerializeField]
    public int bullets = 3000;
    [SerializeField]
    private bool isAutomatic = true;

    [Header ("Shoot Settings")]
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    public float recoilForce = 4f;
    [SerializeField]
    public float RecoilCorrection = 2f;
    [SerializeField]
    private float BulletSpeed = 350f;
    [SerializeField]
    public Vector3 aimcorrection = new Vector3(0f, 0f, 0f);
    
    [Header ("Bullet Spreading")]
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    
    [SerializeField]
    private LayerMask Mask;
    
    private Animator animator;
    private float LastShootTime;
    private Vector3 originPosition;

    // flags
    private bool isReloading = false;
    private bool isAiming = false;
    private bool isSprinting = false;


    private void Start(){
        originPosition = transform.localPosition;
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,currentMag,bullets);
    }


    private void Awake(){
        animator = GetComponent<Animator>();
    }

    private void OnEnable(){
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,currentMag,bullets);
    }


    public bool IsAutomatic() {
        return isAutomatic;
    }


    public void Shoot() {
        if ((LastShootTime + ShootDelay < Time.time) && (currentMag > 0) && !isReloading) {
            StartCoroutine(ShootCorroutine());
        } else if (currentMag == 0)
            Reload();
    }

    // cambiar a sphere cast + raycast
    private IEnumerator ShootCorroutine(){
        //animator.SetBool("Shooting", true);
        ShootingSystem.Play();
        shootSound.Play();
        Vector3 direction = GetDirection();

        //Collider[] rangeChecks = Physics.OverlapSphere(transform.position, 90, Mask);
        //if (rangeChecks.Length != 0) {
        // Si el raycast impacta, el trail se renderiza hasta el punto de impacto
        if (Physics.Raycast(GameObject.FindWithTag("MainCamera").transform.position, direction, out RaycastHit hit, float.MaxValue, Mask)){
            StartCoroutine(SpawnTrail(hit));
        // Si no impacta, lo renderizamos desde la boquilla en línea recta + dispersión una determinada distancia 
        } else {
            StartCoroutine(SpawnTrail(direction));
        }
        LastShootTime = Time.time;
        currentMag -= 1;
        //Actualizamos Hud
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,currentMag,bullets);
        // Fuerza de retroceso en el arma
        AddRecoil();
        yield return null;
    }


    /*           FÍSICAS DE DISPARO                */
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection() {
        Vector3 direction = transform.forward;
        if (AddBulletSpread) {
            // Añadimos una dispersión aleatoria al vector de dirección, 
            // si estamos apuntando
            if (!isAiming) {
                direction += new Vector3(
                    Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                    Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                    Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
                );
                direction.Normalize();
            }
        }
        return direction;
    }

    // Spawnear el trail desde el origen al punto de impacto
    private IEnumerator SpawnTrail(RaycastHit hit) {
        float time = 0;
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        float distance = hit.distance;
        // un impacto q este a BulletSpeed m -> 1 segundo
        // distancia/velocidad = tiempo
        // spawneamos la bala un determinado tiempo
        while (time < distance/BulletSpeed) {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += BulletSpeed*Time.deltaTime/distance;
            yield return null;   
        }
        trail.transform.position = hit.point;
        SoldierPart soldier = hit.transform.GetComponent<SoldierPart>();
        if (soldier != null)
            soldier.DoDamage(Damage);
        else
            Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        if (hit.rigidbody != null)
            hit.rigidbody.AddForce(-hit.normal * 60f);
        Destroy(trail.gameObject, trail.time);
    }

    // Spawnear el trail desde el origen una determinada distancia hacia delante
    // para dar sensación de disparo hasta el infinito
    private IEnumerator SpawnTrail(Vector3 direction) {
        float time = 0;
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        Vector3 endPosition = startPosition + direction * 100;
        float distance = Vector3.Distance(startPosition, endPosition);
        while (time < distance/BulletSpeed) {
            trail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += BulletSpeed*Time.deltaTime/distance;
            yield return null;   
        }
        trail.transform.position = endPosition;
        Destroy(trail.gameObject, trail.time);
    }


    /*           ANIMACIONES DE DISPARO                */

    public bool IsAnimPlaying(string animation) {
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animation);
    }

    private void AddRecoil() {
        if (isAiming)
            transform.Rotate(-RecoilCorrection, 0f, 0f);
        else 
            transform.Rotate(-recoilForce, 0f, 0f);
    }


    public void Reload() {
        if (isReloading == true || currentMag == MagSize || bullets == 0)
            return;
        reloadSound.Play();
        animator.SetTrigger("Reload");
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,-1,bullets);
        isReloading = true;
        StartCoroutine(AfterReload());
    }

    // esto esta mal, hay que hacerlo con animation events, pero en linux no me funciona
    private IEnumerator AfterReload() {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        currentMag = MagSize;
        bullets -= MagSize;
        isReloading = false;
        EventManager.instance.UpdateBulletsEvent.Invoke(typegun,currentMag,bullets);
    }

    public bool IsReloading() {
        return isReloading;
    }

    public void Aim() {
        if (!isAiming){
            StartCoroutine(AimAnimation(originPosition, originPosition + aimcorrection));
            isAiming = true;
        }
    }


    //revisar xq lo hice pero sin entenderlo al 100
    private IEnumerator AimAnimation(Vector3 origin, Vector3 target){
        float desiredDuration = .1f; //meterlo en otro lado
        float time = 0;
        while (time < desiredDuration) {
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, target, time/desiredDuration);
            yield return null;
        }
    }

    public bool IsAiming() {
        return isAiming;
    }

    public void Idle() {
        if (isAiming) {
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            isAiming = false;
        }
        animator.SetBool("Sprint",false);
        isSprinting = false;
        GameObject player =GameObject.FindWithTag("Player");
        player.GetComponent<PlayerController>().StopSprint();
    }

    public void Sprint() {
        animator.SetBool("Sprint",true);
        isSprinting = true;
        GameObject player = GameObject.FindWithTag("Player");
        player.GetComponent<PlayerController>().Sprint();
    }

    public bool IsSprinting() {
        return isSprinting;
    }

    public void Drop() {
        // Actualizamos las balas en el objeto a dropear
        dropeableModel.GetComponent<DroppedWeapon>().SetBullets(currentMag,bullets);
        // Drop
        GameObject drop = Instantiate(dropeableModel, transform.position, Quaternion.identity); 
        drop.GetComponent<Rigidbody>().AddForce(GameObject.FindWithTag("Player").transform.forward * 10f, ForceMode.Impulse);
        drop.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 10f, ForceMode.Impulse);
        Destroy(gameObject);
    }
}