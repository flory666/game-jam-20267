using UnityEngine;
using UnityEngine.AI; 

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class SimplePatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    public Transform pointA;
    public Transform pointB;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform currentTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Disable automatic rotation if you want smoother turns via animation, 
        // but for simple NPCs, leaving this true is safer.
        agent.updateRotation = true;

        // Start going to Point A
        currentTarget = pointA;
        if (currentTarget != null)
        {
            agent.SetDestination(currentTarget.position);
        }
    }

void Update()
    {
        if (currentTarget == null || !agent.isOnNavMesh) return;

        // EMERGENCY KICKSTART: If he has a target but no path, force him to go!
        if (!agent.hasPath && agent.remainingDistance < 0.1f)
        {
            agent.SetDestination(currentTarget.position);
        }

        // 1. Check if we reached the destination
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            SwitchTarget();
        }

        // 2. Animate
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude); 
        }
    }

    void SwitchTarget()
    {
        if (currentTarget == pointA) currentTarget = pointB;
        else currentTarget = pointA;

        agent.SetDestination(currentTarget.position);
    }
}