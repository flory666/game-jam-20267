using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoToNextScene : MonoBehaviour
{
    public string nextSceneName;

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("MainMenu");
    }
}