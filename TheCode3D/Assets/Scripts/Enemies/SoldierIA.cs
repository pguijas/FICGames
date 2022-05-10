using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using QuantumTek.QuantumTravel;


[RequireComponent(typeof(QT_MapObject))]
public class SoldierIA : MonoBehaviour {
    
    [SerializeField]
    public Animator anim;
    
    [Header("Enemy Settings")]
    [SerializeField]
    public float health = 100f;
    [SerializeField]
    public EnemyWeaponController weapon;
    [SerializeField]
    public float radius;
    [SerializeField]
    [Range(0,360)]
    public float angle;
    [SerializeField]
    public NavMeshAgent agent;
    [SerializeField]
    public Transform wayPointsObject;

    [Header("Masks")]
    public LayerMask playerMask;
    public LayerMask obstacleMask;

    private PlayerController player;
    private GameObject MP40;
    private GameObject STG44;
    private int currentWayPoint;
    private List<Transform> waypoints = new List<Transform>();
    private IEnumerator coroutine;
    private float distanceToTarget = Mathf.Infinity;
    private Vector3 directionToTarget = Vector3.zero;

    // flags
    private bool notifyed = false;
    private bool canSeePlayer = false;
    private bool shooting = false;
    private bool isDead = false;


    // Start is called before the first frame update
    void Start() {
        if (gameObject.tag == "wehrmacht_b") {
            GameObject WeaponPath = GameObject.Find(gameObject.name + "/Wehrmacht_soilder_B");
            MP40 = WeaponPath.transform.Find("MP40").gameObject;
        }
        if (gameObject.tag == "schutzstaffel_b") {
            GameObject WeaponPath = GameObject.Find(gameObject.name + "/SchutzStaffel_B");
            STG44 = WeaponPath.transform.Find("STG44").gameObject;
        }
        anim.SetInteger("Status_walk", 0);
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        // Field of View
        StartCoroutine(FieldOfViewRoutine());
        AddRoute();
    }

    // Update is called once per frame
    void Update() {
        if (isDead)
            return;
        if (!notifyed) {
            EventManager.instance.NewSoldierEvent.Invoke(gameObject.GetComponent<QT_MapObject>());
            notifyed = true;
        }
        if (canSeePlayer) {
            if (!shooting) {
                StartCoroutine(ShootDelay());
            } else {
                weapon.Shoot(distanceToTarget);
            }
        } else {
            if (shooting) {
                anim.SetInteger("Status_walk", 2);
                shooting = false;
            } else if (anim.GetInteger("Status_walk") == 1) {
                if (!isDead) {
                    agent.speed = 3.5f;
                    StartCoroutine(FollowRouteRoutine());
                }
            }
        }
    }

    private IEnumerator ShootDelay() {
        yield return new WaitForSeconds(0.5f);
        anim.SetInteger("Status_walk", 3);
        shooting = true;
        weapon.Shoot(distanceToTarget);
    }

    private IEnumerator FollowRouteRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        while (!isDead) {
            yield return wait;
            if (currentWayPoint == waypoints.Count -1) {
                currentWayPoint = 0;
                anim.SetInteger("Status_walk", 0);
                break;
            } else {
                FollowRoute();
            }
        }
    }

    private void FollowRoute() {
        if (isDead)
            return;
        if (agent.remainingDistance <= agent.stoppingDistance) {
            currentWayPoint++;
            agent.SetDestination(waypoints[currentWayPoint].position);
        }
    }

    private void AddRoute() {
        foreach (Transform t in wayPointsObject)
            waypoints.Add(t);
        agent = anim.GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[currentWayPoint].position);
    }

    public void TakeDamage(float damage) {
        if (health - damage <= 0) {
            health = 0;
            isDead = true;
            StartCoroutine(Die());
        } else {
            health -= damage;
            if (distanceToTarget < radius)
                anim.SetInteger("Status_walk", 3);
            else 
                anim.SetInteger("Status_walk", 2);
        }
    }


    private IEnumerator Die() {
        yield return null;
        EventManager.instance.DeadSoldierEvent.Invoke(gameObject.GetComponent<QT_MapObject>());
        Destroy(gameObject);
    }


    private IEnumerator FieldOfViewRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (true) {
            yield return wait;
            FieldOfViewCheck();
        }
    }

    private void FieldOfViewCheck() {
        // Sphere cast
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, playerMask);
        if (rangeChecks.Length != 0) {
            Transform target = rangeChecks[0].transform;
            directionToTarget = (target.position - transform.position).normalized;
            distanceToTarget = Vector3.Distance(transform.position, target.position);
            if ((Vector3.Angle(transform.forward, directionToTarget) < angle / 2) || distanceToTarget < radius/2) {
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                    canSeePlayer = true;
                else {
                    canSeePlayer = false;
                    if (distanceToTarget < radius/3)
                        anim.SetInteger("Status_walk", 2);
                }
            } else
                canSeePlayer = false;
        } else if (canSeePlayer)
            canSeePlayer = false;
    }
}
