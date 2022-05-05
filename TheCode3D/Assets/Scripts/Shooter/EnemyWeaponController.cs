using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour {

    //public AudioSource shootSound;
    
    [Header ("Gun Settings")]
    [SerializeField]
    public float Damage = 10f;
    [SerializeField]
    public int MagSize = 30;
    [SerializeField]
    public int currentMag = 30;
    [SerializeField]
    public int bullets = 3000;

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
    private float BulletSpeed = 350f;
    
    [Header ("Bullet Spreading")]
    [SerializeField]
    private Vector3 BulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    
    [SerializeField]
    private LayerMask Mask;
    
    private float LastShootTime;
    private Vector3 originPosition;

    // flags
    private bool isReloading = false;
    private bool isAiming = false;
    private bool isSprinting = false;


    private void Start(){
        originPosition = transform.localPosition;
    }


    public void Shoot(){
        if ((LastShootTime + ShootDelay < Time.time) && (currentMag > 0) && !isReloading) {
            //animator.SetBool("Shooting", true);
            ShootingSystem.Play();
            //shootSound.Play();
            Vector3 direction = transform.TransformDirection(GetDirection()) * 10;
            Debug.DrawRay(new Vector3(BulletSpawnPoint.position.x, BulletSpawnPoint.position.y, BulletSpawnPoint.position.z), direction, Color.green);
            // Si el raycast impacta, el trail se renderiza hasta el punto de impacto
            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask)){
                StartCoroutine(SpawnTrail(hit));
                LastShootTime = Time.time;
            // Si no impacta, lo renderizamos desde la boquilla en línea recta + dispersión una determinada distancia
            } else{
                StartCoroutine(SpawnTrail(direction));
                LastShootTime = Time.time;
            }
            currentMag -= 1;
        } else if (currentMag == 0)
            Reload();
    }
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection() {
        Vector3 direction = transform.forward;
        direction += new Vector3(
            Random.Range(-BulletSpreadVariance.x, BulletSpreadVariance.x),
            Random.Range(-BulletSpreadVariance.y, BulletSpreadVariance.y),
            Random.Range(-BulletSpreadVariance.z, BulletSpreadVariance.z)
        );
        direction.Normalize();
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
        while (time < 1000*distance/BulletSpeed) {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += BulletSpeed*Time.deltaTime/distance;
            yield return null;
        }
        trail.transform.position = hit.point;
        PlayerController player = hit.transform.GetComponent<PlayerController>();
        if (player != null)
            player.TakeDamage(Damage);
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


    public void Reload() {
        if (isReloading == true || currentMag == MagSize || bullets == 0)
            return;
        //reloadSound.Play();
        currentMag = 30;
    }

}
