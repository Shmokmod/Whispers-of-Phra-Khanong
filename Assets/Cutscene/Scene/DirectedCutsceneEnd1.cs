using UnityEngine;
using UnityEngine.Video;

public class DirectedCutsceneEnd1 : MonoBehaviour
{
    public string nextScene = "Level1";
    public float minPlayTime = 2f;

    private bool isEnding;
    private bool canSkip;
    private float startTime;
    private VideoPlayer vp;

    private void Awake()
    {
        Time.timeScale = 1f;
        PauseController.isPaused = false;
        startTime = Time.unscaledTime;
    }

    private void Start()
    {
        vp = GetComponent<VideoPlayer>();
        if (vp != null)
            vp.loopPointReached += OnVideoEnd;

        Invoke(nameof(EnableSkip), minPlayTime);
    }

    void EnableSkip() => canSkip = true;

    private void Update()
    {
        if (isEnding || !canSkip) return;

        if (Input.GetKeyDown(KeyCode.Space))
            EndCutscene();
    }

    void OnVideoEnd(VideoPlayer _)
    {
        EndCutscene();
    }

    void EndCutscene()
    {
        if (isEnding) return;
        isEnding = true;

        if (vp != null)
        {
            vp.loopPointReached -= OnVideoEnd;
            vp.Stop();
        }

        Time.timeScale = 1f;
        PauseController.isPaused = false;

        if (LoadingScreen.Instance != null)
            StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (vp != null)
            vp.loopPointReached -= OnVideoEnd;
    }
}
