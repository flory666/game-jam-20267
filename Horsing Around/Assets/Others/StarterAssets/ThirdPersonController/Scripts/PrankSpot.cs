using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections; 

public enum PrankType { Graffiti, FireAlarm, KickObject }

[RequireComponent(typeof(AudioSource))]
public class PrankSpot : MonoBehaviour
{
    [Header("Settings")]
    public PrankType myPrankType;   
    
    [Header("Prank Sound")]
    public AudioClip soundEffect; 

    [Header("Timers")]
    public float graffitiFadeTime = 5f; 
    public float alarmDuration = 5f;    // Alarm rings for this long
    public float kickRespawnTime = 10f; 

    [Header("Kick Settings")]
    public float kickForce = 15f; 

    [Header("Drag Objects Here")]
    public GameObject targetObject; 

    // Private variables
    private GameObject globalPrompt;
    private AudioSource myAudioSource; 
    private bool isPlayerClose = false;
    private bool hasBeenPranked = false;
    
    private Transform playerTransform; 

    // Respawn Memory
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
        myAudioSource.playOnAwake = false; 

        if (targetObject != null)
        {
            originalPosition = targetObject.transform.position;
            originalRotation = targetObject.transform.rotation;
        }

        globalPrompt = GameObject.FindGameObjectWithTag("PrankUI");
        if (globalPrompt == null) 
        {
             Transform found = Canvas.FindObjectOfType<Canvas>().transform.Find("InteractionPrompt");
             if (found != null) globalPrompt = found.gameObject;
        }
        if (globalPrompt != null) globalPrompt.SetActive(false);

        if (targetObject != null && myPrankType == PrankType.Graffiti)
        {
            SetGraffitiAlpha(0f); 
        }
    }

    void Update()
    {
        if (isPlayerClose && !hasBeenPranked && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DoThePrank();
        }
    }

    void DoThePrank()
    {
        hasBeenPranked = true; 
        if (globalPrompt != null) globalPrompt.SetActive(false); 

        switch (myPrankType)
        {
            case PrankType.Graffiti:
                if (soundEffect != null) myAudioSource.PlayOneShot(soundEffect);
                SetGraffitiAlpha(1f); 
                StartCoroutine(GraffitiRoutine()); 
                break;

            case PrankType.FireAlarm:
                StartCoroutine(FireAlarmRoutine());
                break;

            case PrankType.KickObject:
                if (soundEffect != null) myAudioSource.PlayOneShot(soundEffect);
                
                Rigidbody rb = targetObject.GetComponent<Rigidbody>();
                if (rb != null && playerTransform != null)
                {
                    Vector3 playerDir = playerTransform.forward;
                    Vector3 kickDir = (playerDir + Vector3.up * 0.5f).normalized;

                    rb.AddForce(kickDir * kickForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * kickForce, ForceMode.Impulse);
                }
                
                StartCoroutine(KickRespawnRoutine());
                break;
        }
    }

    // --- RESET LOGIC ---
    void ResetPrank()
    {
        hasBeenPranked = false; 
        if (isPlayerClose && globalPrompt != null)
        {
            globalPrompt.SetActive(true);
        }
    }

    // --- 1. KICK RESPAWN ROUTINE ---
    IEnumerator KickRespawnRoutine()
    {
        yield return new WaitForSeconds(kickRespawnTime);

        if (targetObject != null)
        {
            Rigidbody rb = targetObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero; 
                rb.angularVelocity = Vector3.zero; 
            }
            targetObject.transform.position = originalPosition;
            targetObject.transform.rotation = originalRotation;
        }
        ResetPrank(); 
    }

    // --- 2. GRAFFITI ROUTINE ---
    IEnumerator GraffitiRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < graffitiFadeTime)
        {
            elapsedTime += Time.deltaTime; 
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / graffitiFadeTime);
            SetGraffitiAlpha(newAlpha);
            yield return null; 
        }
        SetGraffitiAlpha(0f);   
        ResetPrank(); 
    }

    // --- 3. FIRE ALARM ROUTINE (UPDATED) ---
    IEnumerator FireAlarmRoutine()
    {
        // A. PLAY SOUND
        if (soundEffect != null)
        {
            myAudioSource.clip = soundEffect;
            myAudioSource.loop = true;  
            myAudioSource.Play();
        }

        // B. WAIT FOR DURATION (e.g. 5 seconds)
        yield return new WaitForSeconds(alarmDuration);

        // C. STOP SOUND
        myAudioSource.Stop();
        myAudioSource.loop = false; 

        // D. SILENT COOLDOWN (Duration x 10)
        // If duration is 5s, we wait 50s here.
        yield return new WaitForSeconds(alarmDuration * 10f);

        // E. RESET
        ResetPrank(); 
    }

    void SetGraffitiAlpha(float alpha)
    {
        if (targetObject != null)
        {
            SpriteRenderer sr = targetObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    // --- TRIGGER ZONES ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerTransform = other.transform; 
            isPlayerClose = true;
            if (globalPrompt != null && !hasBeenPranked) globalPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerClose = false;
            if (globalPrompt != null) globalPrompt.SetActive(false);
        }
    }
}