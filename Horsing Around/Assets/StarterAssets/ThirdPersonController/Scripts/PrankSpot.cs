using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections; 

public enum PrankType { Graffiti, FireAlarm, LockDoor }

[RequireComponent(typeof(AudioSource))] // This forces Unity to add an AudioSource if you forget
public class PrankSpot : MonoBehaviour
{
    [Header("Settings")]
    public PrankType myPrankType;   
    
    [Header("Prank Sound")]
    public AudioClip soundEffect; // DRAG YOUR MP3 HERE (Spray, Bell, Click)

    [Header("Timers")]
    public float graffitiFadeTime = 10f; 
    public float alarmDuration = 10f;    
    public float lockDuration = 10f;     

    [Header("Drag Objects Here")]
    public GameObject targetObject; 

    // Private variables
    private GameObject globalPrompt;
    private AudioSource myAudioSource; // The speaker
    private bool isPlayerClose = false;
    private bool hasBeenPranked = false;

    void Start()
    {
        // 1. Get the Audio Source attached to this object
        myAudioSource = GetComponent<AudioSource>();
        myAudioSource.playOnAwake = false; // Just to be safe

        // 2. Find UI
        globalPrompt = GameObject.FindGameObjectWithTag("PrankUI");
        if (globalPrompt != null) globalPrompt.SetActive(false);

        // 3. Setup Initial State
        if (targetObject != null)
        {
            switch (myPrankType)
            {
                case PrankType.Graffiti:
                    SetGraffitiAlpha(0f); 
                    break;
                case PrankType.LockDoor:
                    targetObject.SetActive(false); 
                    break;
                case PrankType.FireAlarm:
                    // Alarm target usually doesn't need hiding, just sound
                    break;
            }
        }
    }

    void Update()
    {
        // INPUT CHECK
        if (isPlayerClose && !hasBeenPranked && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DoThePrank();
        }

        // SMART UI REFRESH
        if (isPlayerClose && !hasBeenPranked && globalPrompt != null && !globalPrompt.activeSelf)
        {
            globalPrompt.SetActive(true);
        }
    }

    void DoThePrank()
    {
        hasBeenPranked = true; 
        if (globalPrompt != null) globalPrompt.SetActive(false); 

        switch (myPrankType)
        {
            case PrankType.Graffiti:
                // Play Spray Sound (One Shot)
                if (soundEffect != null) myAudioSource.PlayOneShot(soundEffect);
                
                SetGraffitiAlpha(1f); 
                StartCoroutine(GraffitiRoutine()); 
                break;

            case PrankType.FireAlarm:
                // Alarm is special: it needs to LOOP for the duration
                StartCoroutine(FireAlarmRoutine());
                break;

            case PrankType.LockDoor:
                // Play Lock Click (One Shot)
                if (soundEffect != null) myAudioSource.PlayOneShot(soundEffect);

                StartCoroutine(LockDoorRoutine());
                break;
        }
    }

    // --- 1. GRAFFITI ROUTINE ---
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
        hasBeenPranked = false; 
    }

    // --- 2. FIRE ALARM ROUTINE (Loops Audio) ---
    IEnumerator FireAlarmRoutine()
    {
        if (soundEffect != null)
        {
            myAudioSource.clip = soundEffect;
            myAudioSource.loop = true;  // Turn ON looping
            myAudioSource.Play();
        }

        // Wait for duration
        yield return new WaitForSeconds(alarmDuration);

        // Stop Audio
        myAudioSource.Stop();
        myAudioSource.loop = false; // Turn OFF looping
        
        hasBeenPranked = false; 
    }

    // --- 3. LOCK DOOR ROUTINE ---
    IEnumerator LockDoorRoutine()
    {
        if (targetObject != null) targetObject.SetActive(true);

        yield return new WaitForSeconds(lockDuration);

        if (targetObject != null) targetObject.SetActive(false);

        hasBeenPranked = false; 
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
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