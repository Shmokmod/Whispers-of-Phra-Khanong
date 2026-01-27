using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class CutsceneEnd : MonoBehaviour
{
    public string nextScene = "Level1";

    [Header("Skip Settings")]
    [Tooltip("ป้องกัน skip ใน X วินาทีแรก")]
    public float minPlayTime = 2f;
    [Tooltip("ป้องกัน spam กดติดๆ")]
    public float skipCooldown = 0.3f;

    private bool isEnding = false;
    private bool canSkip = false;
    private VideoPlayer vp;
    private float sceneStartTime;
    private float lastSkipAttempt = -999f;

    private void Awake()
    {
        Time.timeScale = 1f;
        PauseController.isPaused = false;
        sceneStartTime = Time.unscaledTime;
    }

    void Start()
    {
        // 🔑 แก้จอดำค้างจาก fade ก่อนหน้า
        if (LoadingScreen.Instance != null)
        {
            var canvas = LoadingScreen.Instance.GetComponentInChildren<CanvasGroup>();
            if (canvas != null)
            {
                canvas.alpha = 0f;
                canvas.blocksRaycasts = false;
            }
        }

        vp = GetComponent<VideoPlayer>();
        if (vp != null)
            vp.loopPointReached += OnVideoEnd;

        StartCoroutine(EnableSkipAfterDelay());
    }



    IEnumerator EnableSkipAfterDelay()
    {
        float waitTime = Mathf.Max(minPlayTime, 2.5f);
        yield return new WaitForSecondsRealtime(waitTime);
        canSkip = true;
        Debug.Log($"✅ Skip enabled after {waitTime}s");
    }

    void Update()
    {
        if (isEnding) return;
        if (!canSkip) return;

        if (LoadingScreen.Instance != null && LoadingScreen.Instance.IsLoading)
            return;

        if (Time.unscaledTime - lastSkipAttempt < skipCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastSkipAttempt = Time.unscaledTime;
            EndCutscene();
        }
    }


    void OnVideoEnd(VideoPlayer source)
    {
        Debug.Log("🎬 Video finished naturally");
        EndCutscene();
    }

    void EndCutscene()
    {
        // ✅ ป้องกันเรียกซ้ำ (พอแล้ว)
        if (isEnding)
            return;

        isEnding = true;
        Debug.Log("🚪 EndCutscene called");

        StopAllCoroutines();

        if (vp != null)
        {
            vp.loopPointReached -= OnVideoEnd;
            vp.Stop();
        }

        Time.timeScale = 1f;
        PauseController.isPaused = false;

        if (LoadingScreen.Instance != null)
        {
            StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }



    private void OnDestroy()
    {
        if (vp != null)
        {
            vp.loopPointReached -= OnVideoEnd;
        }
    }
}