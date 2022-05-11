using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour {
    
    [Header ("Gun Settings")]
    [SerializeField]
    public float Damage = 10f;

    [Header ("Weapon Animations")]
    [SerializeField]
    private ParticleSystem ShootingSystem;
    [SerializeField]
    public AudioSource shootSound;
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
    private PlayerController Player;

    // flags
    private bool isReloading = false;
    private bool isSprinting = false;


    private void Start(){
        originPosition = transform.localPosition;
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }


    public void Shoot(float distanceToTarget) {
        // utilizamos la distancia al jugador para ajustar
        // la dispersión de los disparos
        float conversion = distanceToTarget / 50;
        if ((LastShootTime + ShootDelay) < Time.time) {
            // Animacion de fogeo y sonido
            ShootingSystem.Play();
            shootSound.Play();
            // Calculamos la dirección de disparo y la dispersión
            Vector3 direction = GetDirection(conversion);
            // Si el raycast impacta, el trail se renderiza hasta el punto de impacto, utilizamos las
            // capas de obstáculos y jugador para trazar este
            if (Physics.Raycast(BulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, Mask)) {
                StartCoroutine(SpawnTrail(hit));
            } else {
                // Si no impacta, lo renderizamos desde la boquilla en línea recta + dispersión una determinada distancia
                StartCoroutine(SpawnTrail(direction));
            }
            LastShootTime = Time.time;
        }
    }
    
    // Aleatorizar el vector que indica la direccion de disparo (bullet spread)
    private Vector3 GetDirection(float distance) {
        Vector3 direction = Player.transform.position - BulletSpawnPoint.position;
        // si la distancia es superior a 50 metros, no aplicamos dispersión
        if (distance < 1) {
            direction += new Vector3(
                Random.Range(-(BulletSpreadVariance.x/distance), (BulletSpreadVariance.x/distance)),
                Random.Range(-(BulletSpreadVariance.y/distance), (BulletSpreadVariance.y/distance)),
                Random.Range(-(BulletSpreadVariance.z/distance), (BulletSpreadVariance.z/distance))
            );
        }
        direction.Normalize();
        return direction;

    }

    // Animación de disparo hacia objetos dentro de las máscaras de impacto
    private IEnumerator SpawnTrail(RaycastHit hit) {
        float time = 0;
        // Creamos un objeto que animará nuestros disparos
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        float distance = hit.distance;
        // un impacto q este a BulletSpeed m -> 1 segundo
        // distancia/velocidad = tiempo
        // spawneamos la bala un determinado tiempo
        while (time < distance/BulletSpeed) {
            // movimiento de la bala
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += BulletSpeed*Time.deltaTime/distance;
            yield return null;
        }
        trail.transform.position = hit.point;
        // después del tiempo necesario para llegar al punto de impacto
        // comprobamos si hemos impactado en el jugador
        PlayerController player = hit.transform.GetComponent<PlayerController>();
        if (player != null)
            // si impactamos en el jugador, le quitamos salud
            player.TakeDamage(Damage);
        else
            // si no, instanciamos un animación de impacto en un obstáculo
            Instantiate(ImpactParticleSystem, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(trail.gameObject, trail.time);
    }

    // Animación de disparo hacia una dirección, pero sin ningún impacto,
    // por ejemplo disparar al cielo
    private IEnumerator SpawnTrail(Vector3 direction) {
        float time = 0;
        TrailRenderer trail = Instantiate(BulletTrail, BulletSpawnPoint.position, Quaternion.identity);
        Vector3 startPosition = trail.transform.position;
        // precalculamos la distancia a la que llegará la bala
        Vector3 endPosition = startPosition + direction * 100;
        float distance = Vector3.Distance(startPosition, endPosition);
        // renderizamos el efecto un determinado tiempo y después lo destruimos
        while (time < distance/BulletSpeed) {
            trail.transform.position = Vector3.Lerp(startPosition, endPosition, time);
            time += BulletSpeed*Time.deltaTime/distance;
            yield return null;
        }
        trail.transform.position = endPosition;
        Destroy(trail.gameObject, trail.time);
    }
}
