using UnityEngine;
using UnityEngine.UI; // Needed for UI
using UnityEngine.SceneManagement; // Needed to change scenes
using UnityEngine.InputSystem; // Needed for "E" key

public class HorsingAroundMeter : MonoBehaviour
{
    [Header("UI Settings")]
    public Slider meterSlider; // Drag your UI Slider here
    public float maxChaos = 1000f;
    public float currentChaos;

    [Header("Game Settings")]
    public float decayRate = 25f; // How fast it drops per second
    public float prankReward = 15f; // How much E adds
    public string mainMenuSceneName = "MainMenu"; // Name of your menu scene

    void Start()
    {
        currentChaos = maxChaos;
        
        if (meterSlider != null)
        {
            meterSlider.maxValue = maxChaos;
            meterSlider.value = currentChaos;
        }
    }

    void Update()
    {
        // 1. DRAIN THE METER
        currentChaos -= decayRate * Time.deltaTime;

        // 2. CHECK FOR PRANK (Press E to Refill)
        // Note: In the final game, you might want to call this from PrankSpot.cs instead
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            AddChaos(prankReward);
        }

        // 3. UPDATE UI
        if (meterSlider != null)
        {
            meterSlider.value = currentChaos;
        }

        // 4. GAME OVER CHECK
        if (currentChaos <= 0)
        {
            GameOver();
        }
    }

    public void AddChaos(float amount)
    {
        currentChaos += amount;
        if (currentChaos > maxChaos) currentChaos = maxChaos;
    }

    void GameOver()
    {
        Debug.Log("Game Over! Returning to Main Menu...");
        // Ensure your Main Menu is added in Build Settings!
        SceneManager.LoadScene(mainMenuSceneName);
    }
}