using UnityEngine;
using UnityEngine.AI;

public class PoliceAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float patrolWaitTime = 2f; 

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;
    public float detectionRange = 15f; 
    public float chaseRange = 30f;
    public float catchDistance = 2f;

    [Header("Alert Settings")]
    public float alertCooldown = 3f;

    [Header("References")]
    public Transform player;
    public GameOverCanvas gameOverCanvas;
    public LayerMask playerLayer;

    [Header("Visual Feedback")]
    public Color normalColor = Color.blue;
    public Color alertColor = Color.red;
    public Renderer policeRenderer;
    private NavMeshAgent agent;
    private int currentPatrolIndex = 0;
    private bool isAlerted = false;
    private bool isChasing = false;
    private float alertTimer = 0f;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 patrolCenter;
    private float patrolRadius;

    public enum PoliceState { Patrolling, Investigating, Chasing, Returning }
    public PoliceState currentState = PoliceState.Patrolling;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        if (patrolPoints.Length > 0)
        {
            CalculatePatrolArea();
            GoToNextPatrolPoint();
        }

        if (alertIndicator != null)
            alertIndicator.SetActive(false);
    }

    void Update()
    {
        switch (currentState)
        {
            case PoliceState.Patrolling:
                Patrol();
                CheckForPlayer();
                break;

            case PoliceState.Investigating:
                Investigate();
                break;

            case PoliceState.Chasing:
                ChasePlayer();
                break;

            case PoliceState.Returning:
                ReturnToPatrol();
                break;
        }

        if (isChasing || currentState == PoliceState.Chasing)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= catchDistance)
            {
                CatchPlayer();
            }
        }
    }

    void CalculatePatrolArea()
    {
        Vector3 sum = Vector3.zero;
        foreach (Transform point in patrolPoints)
        {
            sum += point.position;
        }
        patrolCenter = sum / patrolPoints.Length;

        patrolRadius = 0f;
        foreach (Transform point in patrolPoints)
        {
            float distance = Vector3.Distance(patrolCenter, point.position);
            if (distance > patrolRadius)
                patrolRadius = distance;
        }

        patrolRadius += chaseRange;
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.speed = patrolSpeed;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit hit;
            Vector3 directionToPlayer = (player.position - transform.position).normalized;

            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
            {
                if (hit.transform == player)
                {

                }
            }
        }
    }

    public void AlertPolice(Vector3 playerPosition)
    {
        if (currentState == PoliceState.Chasing) return; 

        isAlerted = true;
        lastKnownPlayerPosition = playerPosition;
        currentState = PoliceState.Chasing;
        agent.speed = chaseSpeed;

        if (alertIndicator != null)
            alertIndicator.SetActive(true);

        if (policeRenderer != null)
            policeRenderer.material.color = alertColor;

        Debug.Log("Police alerted! Chasing player!");
    }

    void Investigate()
    {
        agent.speed = patrolSpeed;
        agent.destination = lastKnownPlayerPosition;

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            currentState = PoliceState.Returning;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            AlertPolice(player.position);
        }
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceFromPatrolCenter = Vector3.Distance(transform.position, patrolCenter);

        if (distanceFromPatrolCenter > patrolRadius)
        {
            Debug.Log("Player escaped patrol area!");
            currentState = PoliceState.Returning;
            CalmDown();
            return;
        }

        if (distanceToPlayer > chaseRange)
        {
            Debug.Log("Player too far, investigating last position...");
            currentState = PoliceState.Investigating;
            return;
        }

        agent.speed = chaseSpeed;
        agent.destination = player.position;
        lastKnownPlayerPosition = player.position;
    }

    void ReturnToPatrol()
    {
        agent.speed = patrolSpeed;

        float closestDistance = Mathf.Infinity;
        int closestIndex = 0;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        currentPatrolIndex = closestIndex;
        agent.destination = patrolPoints[currentPatrolIndex].position;

        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            currentState = PoliceState.Patrolling;
            CalmDown();
        }
    }

    void CalmDown()
    {
        isAlerted = false;
        isChasing = false;

        if (alertIndicator != null)
            alertIndicator.SetActive(false);

        if (policeRenderer != null)
            policeRenderer.material.color = normalColor;

        Debug.Log("Police calmed down, returning to patrol.");
    }

    void CatchPlayer()
    {
        Debug.Log("PLAYER CAUGHT! GAME OVER!");

        agent.isStopped = true;
        Time.timeScale = 0f;

        if (gameOverCanvas != null)
        {
            gameOverCanvas.ShowGameOver();
        }
    }

    void OnDrawGizmosSelected()
    {
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (Transform point in patrolPoints)
            {
                if (point != null)
                    Gizmos.DrawWireSphere(point.position, 1f);
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Transform current = patrolPoints[i];
                    Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
                    if (next != null)
                        Gizmos.DrawLine(current.position, next.position);
                }
            }

            if (Application.isPlaying)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
                Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}