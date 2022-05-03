using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class WeaponController : MonoBehaviour{
    //esta parte de aqui esta basatnte mal -> arreglarla 
    // REVISAR LOS NOMBRES DE LAS VARIABLES (empiezan mayúsculas)
    
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
    public int MagSize = 30;
    [SerializeField]
    public int CurrentMag = 30;
    [SerializeField]
    public int Bullets = 3000;

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

    private void start(){
        originPosition = transform.localPosition;
    }


    private void Awake(){
        animator = GetComponent<Animator>();
    }


    public void Shoot(){
        if ((LastShootTime + ShootDelay < Time.time) && (CurrentMag > 0) && !isReloading) {
            //animator.SetBool("Shooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();
            // Si el raycast impacta, el trail se renderiza hasta el punto de impacto
            if (Physics.Raycast(GameObject.FindWithTag("MainCamera").transform.position, direction, out RaycastHit hit, float.MaxValue, Mask)){
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit));
                LastShootTime = Time.time;
            // Si no impacta, lo renderizamos desde la boquilla en línea recta + dispersión una determinada distancia
            } else{
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, direction));
                LastShootTime = Time.time;
            }
            CurrentMag -= 1;
            // Fuerza de retroceso en el arma
            AddRecoil();
        } else if (CurrentMag == 0)
            Reload();
    }


    /*           FÍSICAS DE DISPARO                */
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection() {
        Vector3 direction = transform.forward;
        if (AddBulletSpread) {
            // Añadimos una dispersión aleatoria al vector de dirección, que sera 
            // menor o mayor según si si estamos apuntando o no
            if (isAiming) {
                direction += new Vector3(
                    Random.Range(-BulletSpreadVariance.x/2, BulletSpreadVariance.x/2),
                    Random.Range(-BulletSpreadVariance.y/2, BulletSpreadVariance.y/2),
                    Random.Range(-BulletSpreadVariance.z/2, BulletSpreadVariance.z/2)
                );
            } else {
                direction += new Vector3(
                    Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                    Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                    Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
                );
            }
            direction.Normalize();
        }
        return direction;
    }

    // Spawnear el trail desde el origen al punto de impacto
    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit) {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;
        float distance = Hit.distance;
        // un impacto q este a BulletSpeed m -> 1 segundo
        // distancia/velocidad = tiempo
        // spawneamos la bala un determinado tiempo
        while (time < distance/BulletSpeed) {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime;
            yield return null;
        }
        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));
        Destroy(Trail.gameObject, Trail.time);
    }

    // Spawnear el trail desde el origen una determinada distancia hacia delante
    // para dar sensación de disparo hasta el infinito
    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 direction) {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;
        Vector3 endPosition = startPosition + direction * 100;
        float distance = Vector3.Distance(startPosition, endPosition);
        while (time < distance/BulletSpeed) {
            Trail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime;
            yield return null;
        }
        Trail.transform.position = endPosition;
        Destroy(Trail.gameObject, Trail.time);
    }


    /*           ANIMACIONES DE DISPARO                */

    private void AddRecoil() {
        if (isAiming)
            transform.Rotate(-RecoilCorrection, 0f, 0f);
        else 
            transform.Rotate(-recoilForce, 0f, 0f);
    }


    public void Reload() {
        if (isReloading == true || CurrentMag == MagSize || Bullets == 0)
            return;
        animator.SetTrigger("Reload");
        isReloading = true;
        StartCoroutine(AfterReload());
    }

    // esto esta mal, hay que hacerlo con animation events, pero en linux no me funciona
    private IEnumerator AfterReload() {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        CurrentMag = MagSize;
        Bullets -= MagSize;
        isReloading = false;
    }


    public void Hide() {
        animator.SetTrigger("Hide");
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


    public void Idle() {
        if (isAiming) {
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            isAiming = false;
        }
        animator.SetBool("Sprint",false);
    }


    public void Sprint() {
        animator.SetBool("Sprint",true);
        if (isAiming) {
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            isAiming = false;
        }
    }
}