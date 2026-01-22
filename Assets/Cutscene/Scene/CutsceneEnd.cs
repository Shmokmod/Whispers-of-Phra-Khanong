using UnityEngine;
using UnityEngine.Video;

public class CutsceneEnd : MonoBehaviour
{
    public string nextScene = "GameScene";
    private bool isEnding = false;

    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        vp.loopPointReached += OnVideoEnd;
    }

    private void Awake()
    {
        Time.timeScale = 1f;
        PauseController.isPaused = false;

    }
    void Update()
    {
        if (isEnding) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Skip | timeScale={Time.timeScale} paused={PauseController.isPaused}");
            EndCutscene();
        }

    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Cutscene ended, loading next scene");
        EndCutscene();
    }

    void EndCutscene()
    {
        if (isEnding) return;
        isEnding = true;

        Time.timeScale = 1f;           // 🔴 สำคัญ
        PauseController.isPaused = false;

        StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
    }

}
