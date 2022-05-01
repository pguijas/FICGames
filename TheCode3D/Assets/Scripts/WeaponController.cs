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
    
    [Header ("Shoot Settings")]
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    public float recoilForce = 4f;
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


    private void start(){
        originPosition = transform.localPosition;
    }


    private void Awake(){
        animator = GetComponent<Animator>();
    }


    public void Shoot(){
        // 
        if (LastShootTime + ShootDelay < Time.time){
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
            // Fuerza de retroceso en el arma
            AddRecoil(recoilForce);
            AddRecoil(-recoilForce);
        }
    }


    /*           FÍSICAS DE DISPARO                */
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection() {
        Vector3 direction = transform.forward;
        if (AddBulletSpread) {
            // Añadimos una dispersión aleatoria al vector de dirección
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );
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

    private void AddRecoil(float recoil) {
        transform.Rotate(-recoilForce, 0f, 0f);
    }


    private bool isReoading = false;
    public void Reload() {
        animator.SetTrigger("Reload");
        Debug.Log("Reloading");
    }


    public void Hide() {
        animator.SetTrigger("Hide");
    }


    private bool aim = false;
    public void Aim() {
        if (!aim){
            StartCoroutine(AimAnimation(originPosition, originPosition + aimcorrection));
            aim = true;
        } 
    }

    //revisar xq lo hice pero sin entenderlo al 100
    private IEnumerator AimAnimation(Vector3 origin, Vector3 target){
        float desiredDuration = .1f; //meterlo en otro lado
        float time = 0;
        while (time < desiredDuration) {
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, target, time/desiredDuration);
            Debug.Log(Time.deltaTime);
            yield return null;
        }
    }


    public void Idle() {
        if (aim) {
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            aim = false;
        }
        animator.SetBool("Sprint",false);
        Debug.Log("Idle");
    }


    public void Sprint() {
        animator.SetBool("Sprint",true);
        Debug.Log("Sprinting");
    }

}