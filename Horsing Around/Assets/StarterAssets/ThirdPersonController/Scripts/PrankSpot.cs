using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections; 

public enum PrankType { Graffiti, KickObject, PlaySound }

public class PrankSpot : MonoBehaviour
{
    [Header("Settings")]
    public PrankType myPrankType;   
    public bool isOneTimeOnly = true; 
    
    [Header("Graffiti Settings")]
    public float fadeDuration = 10f; // Time to fade (Set this to 120 for 2 mins)

    [Header("Drag Objects Here")]
    public GameObject targetObject; 
    public float kickForce = 500f;  

    // Private variables
    private GameObject globalPrompt;
    private bool isPlayerClose = false;
    private bool hasBeenPranked = false;

    void Start()
    {
        globalPrompt = GameObject.FindGameObjectWithTag("PrankUI");
        
        if (globalPrompt != null) globalPrompt.SetActive(false);

        // Hide Graffiti immediately on start
        if (myPrankType == PrankType.Graffiti && targetObject != null)
        {
            SetGraffitiAlpha(0f);
        }
    }

    void Update()
    {
        // 1. INPUT CHECK: If close, clean wall, and pressed E
        if (isPlayerClose && !hasBeenPranked && Keyboard.current.eKey.wasPressedThisFrame)
        {
            DoThePrank();
        }

        // 2. SMART UI REFRESH: 
        // If the wall just became clean while we are standing here, show the text again!
        if (isPlayerClose && !hasBeenPranked && globalPrompt != null && !globalPrompt.activeSelf)
        {
            globalPrompt.SetActive(true);
        }
    }

    void DoThePrank()
    {
        hasBeenPranked = true; // Lock it so we can't spam E
        if (globalPrompt != null) globalPrompt.SetActive(false); // Hide text

        switch (myPrankType)
        {
            case PrankType.Graffiti:
                SetGraffitiAlpha(1f); // Show art
                StartCoroutine(FadeOutAndReset()); // Start the timer
                break;

            case PrankType.KickObject:
                Rigidbody rb = targetObject.GetComponent<Rigidbody>();
                if (rb != null) rb.AddExplosionForce(kickForce, transform.position, 3f);
                break;

            case PrankType.PlaySound:
                AudioSource audio = targetObject.GetComponent<AudioSource>();
                if (audio != null) audio.Play();
                break;
        }
    }

    // --- THE RESET TIMER ---
    IEnumerator FadeOutAndReset()
    {
        float elapsedTime = 0f;
        float startAlpha = 1f;

        // Loop until the timer runs out
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime; 
            float newAlpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeDuration);
            SetGraffitiAlpha(newAlpha);
            yield return null; 
        }

        // END OF TIMER:
        SetGraffitiAlpha(0f);   // Ensure fully invisible
        hasBeenPranked = false; // <--- CRITICAL: RESET THE SYSTEM
    }

    // Helper function to keep code clean
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
            isPlayerClose = true;
            // Only show prompt if the wall is clean (not pranked yet)
            if (globalPrompt != null && !hasBeenPranked) 
            {
                globalPrompt.SetActive(true);
            }
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