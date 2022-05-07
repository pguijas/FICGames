using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SchutzWalkBehavior : StateMachineBehaviour {
    
    private List<Transform> waypoints = new List<Transform>();
    private NavMeshAgent agent;

    private int currentWayPoint = 0;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        Transform wayPointsObject = GameObject.FindGameObjectWithTag("Rute1").transform;
        foreach (Transform t in wayPointsObject)
            waypoints.Add(t);
        agent = animator.GetComponent<NavMeshAgent>();
        agent.SetDestination(waypoints[currentWayPoint].position);
        Debug.Log("walking");
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        if (agent.remainingDistance <= agent.stoppingDistance)
            if (currentWayPoint == waypoints.Count -1) {
                currentWayPoint = 0;
                animator.SetInteger("Status_walk", 0);
            } else 
                currentWayPoint++;
            agent.SetDestination(waypoints[currentWayPoint].position);
    }   

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        agent.SetDestination(agent.transform.position);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
