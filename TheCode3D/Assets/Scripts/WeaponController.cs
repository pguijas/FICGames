using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class WeaponController : MonoBehaviour{
    //esta parte de aqui esta basatnte mal -> arreglarla 
    [SerializeField]
    private bool AddBulletSpread = true;
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    private Transform BulletSpawnPoint;
    [SerializeField]
    private ParticleSystem ImpactParticleSystem;
    [SerializeField]
    private TrailRenderer BulletTrail;
    [SerializeField]
    private float ShootDelay = 0.5f;
    [SerializeField]
    private float BulletSpeed = 350f;
    [SerializeField]
    private LayerMask Mask;

    [SerializeField]
    public Vector3 aimcorrection = new Vector3(0f, 0f, 0f);

    // esto es para hacer la máquina de estados
    private Animator animator;
    private float LastShootTime;


    // REVISAR LOS NOMBRES DE LAS VARIABLES (empiezan mayúsculas)
    private Vector3 originPosition;
    private void start(){
        originPosition = transform.localPosition;
    }

    private void Awake(){
        animator = GetComponent<Animator>();
    }



    public void Shoot(){
        if (LastShootTime + ShootDelay < Time.time){
            
            animator.SetBool("Shooting", true);
            ShootingSystem.Play();
            Vector3 direction = GetDirection();
            if (Physics.Raycast(GameObject.FindWithTag("MainCamera").transform.position, direction, out RaycastHit hit, float.MaxValue, Mask)){
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, hit));
                LastShootTime = Time.time;
            
            } else{
                TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
                StartCoroutine(SpawnTrail(trail, direction));
                LastShootTime = Time.time;
            }
        }
    }


    private bool isReoading = false;
    public void Reload(){
        animator.SetTrigger("Reload");
        Debug.Log("Reloading");
    }

    public void Hide(){
        animator.SetTrigger("Hide");
    }


    private bool aim = false;
    public void Aim(){
        if (!aim){
            StartCoroutine(AimAnimation(originPosition, originPosition + aimcorrection));
            aim = true;
        } 
    }

    //revisar xq lo hice pero sin entenderlo al 100
    private IEnumerator AimAnimation(Vector3 origin, Vector3 target){
        float desiredDuration = .1f; //meterlo en otro lado
        float time = 0;
        while (time < desiredDuration){
            time += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(origin, target, time/desiredDuration);
            Debug.Log(Time.deltaTime);
            yield return null;
        }
    }



    public void Idle(){
        if (aim){
            StartCoroutine(AimAnimation(originPosition + aimcorrection, originPosition)); //recordar cargarse la otra corrutina
            aim = false;
        }
        animator.SetBool("Sprint",false);
        Debug.Log("Idle");
    }


    public void Sprint(){
        animator.SetBool("Sprint",true);
        Debug.Log("Sprinting");
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        if (AddBulletSpread)
        {
            direction += new Vector3(
                Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
                Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
                Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
            );
            direction.Normalize();
        }
        return direction;
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, RaycastHit Hit)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;
        float distance = Hit.distance;
        // un impacto q este a BulletSpeed m -> 1 segundo
        Debug.Log(Hit.point);
        while (time < distance/BulletSpeed)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, Hit.point, time);
            time += Time.deltaTime;
            yield return null;
        }
        animator.SetBool("Shooting", false); 

        Trail.transform.position = Hit.point;
        Instantiate(ImpactParticleSystem, Hit.point, Quaternion.LookRotation(Hit.normal));
        Destroy(Trail.gameObject, Trail.time);
    }

    private IEnumerator SpawnTrail(TrailRenderer Trail, Vector3 direction)
    {
        float time = 0;
        Vector3 startPosition = Trail.transform.position;
        Vector3 endPosition = startPosition + direction * 100;
        float distance = Vector3.Distance(startPosition, endPosition);
        while (time < distance/BulletSpeed)
        {
            Trail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += Time.deltaTime;
            yield return null;
        }
        animator.SetBool("Shooting", false); 
        Trail.transform.position = endPosition;
        Destroy(Trail.gameObject, Trail.time);
    }

}