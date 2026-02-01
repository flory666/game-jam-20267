using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI scoreText;
    public Button restartButton;
    public Button quitButton;

    private HorsingAroundMeter scoreManager;

    void Start()
    {
        gameOverPanel.SetActive(false);
        scoreManager = FindObjectOfType<HorsingAroundMeter>();

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);

        if (scoreText != null && scoreManager != null)
        {
            int finalScore = Mathf.RoundToInt(scoreManager.currentChaos);
            scoreText.text = $"Final Score: {finalScore}";
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void RestartGame()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");

        Application.Quit();
    }
}