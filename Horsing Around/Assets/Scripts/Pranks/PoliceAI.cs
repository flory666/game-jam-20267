using UnityEngine;

public class PoliceAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float rotationSpeed = 5f;

    [Header("Chase Settings")]
    public float chaseSpeed = 6f;
    public float chaseRange = 30f;
    public float catchDistance = 2f;

    [Header("References")]
    public Transform player;
    public HorsingAroundMeter horseMeter;

    [Header("Visual Feedback")]
    public GameObject alertIndicator;
    public Color normalColor = Color.blue;
    public Color alertColor = Color.red;
    public Renderer policeRenderer;

    private int currentPatrolIndex = 0;
    private bool isChasing = false;
    private Vector3 patrolCenter;
    private float patrolRadius;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.freezeRotation = true; 
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (horseMeter == null)
            horseMeter = FindFirstObjectByType<HorsingAroundMeter>();

        if (patrolPoints.Length > 0)
        {
            CalculatePatrolArea();
        }

        if (alertIndicator != null)
            alertIndicator.SetActive(false);
    }

    void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        if (isChasing && player != null)
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
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 targetPosition = targetPoint.position;

        MoveTowards(targetPosition, patrolSpeed);

        float distance = Vector3.Distance(transform.position, targetPosition);
        if (distance < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void ChasePlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceFromPatrolCenter = Vector3.Distance(transform.position, patrolCenter);

        if (distanceFromPatrolCenter > patrolRadius)
        {
            Debug.Log("Player escaped patrol area!");
            StopChasing();
            return;
        }

        if (distanceToPlayer > chaseRange)
        {
            Debug.Log("Player too far!");
            StopChasing();
            return;
        }

        MoveTowards(player.position, chaseSpeed);
    }

    void MoveTowards(Vector3 targetPosition, float speed)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; 

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;
        newPosition.y = transform.position.y; 
        rb.MovePosition(newPosition);
    }

    public void AlertPolice(Vector3 playerPosition)
    {
        if (isChasing) return;

        isChasing = true;

        if (alertIndicator != null)
            alertIndicator.SetActive(true);

        if (policeRenderer != null)
            policeRenderer.material.color = alertColor;

        Debug.Log("Police alerted! Chasing player!");
    }

    void StopChasing()
    {
        isChasing = false;

        if (alertIndicator != null)
            alertIndicator.SetActive(false);

        if (policeRenderer != null)
            policeRenderer.material.color = normalColor;

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

        Debug.Log("Police stopped chasing.");
    }

    void CatchPlayer()
    {
        Debug.Log("PLAYER CAUGHT! GAME OVER!");

        isChasing = false;
        rb.linearVelocity = Vector3.zero;
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
