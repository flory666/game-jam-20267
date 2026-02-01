using UnityEngine;

public class NPCWitness : MonoBehaviour
{
    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public float alertDelay = 0.5f;

    [Header("References")]
    public PoliceAI nearestPolice; 

    [Header("Visual Feedback")]
    public GameObject alertIcon;

    private bool hasAlerted = false;
    private float alertTimer = 0f;
    private bool isAlerting = false;

    void Start()
    {
        if (nearestPolice == null)
        {
            nearestPolice = FindObjectOfType<PoliceAI>();
        }

        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    void Update()
    {
        if (isAlerting)
        {
            alertTimer += Time.deltaTime;
            if (alertTimer >= alertDelay)
            {
                CallPolice();
            }
        }
    }

    public void WitnessPrank(Vector3 prankPosition)
    {
        if (hasAlerted) return;

        float distance = Vector3.Distance(transform.position, prankPosition);

        if (distance <= detectionRange)
        {
            Debug.Log($"{gameObject.name} witnessed a prank!");
            isAlerting = true;

            if (alertIcon != null)
                alertIcon.SetActive(true);

            transform.LookAt(new Vector3(prankPosition.x, transform.position.y, prankPosition.z));
        }
    }

    void CallPolice()
    {
        if (nearestPolice != null && !hasAlerted)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                nearestPolice.AlertPolice(player.transform.position);
                hasAlerted = true;
                isAlerting = false;

                Debug.Log($"{gameObject.name} called the police!");
            }
        }

        if (alertIcon != null)
            alertIcon.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}