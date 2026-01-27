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
        vp = GetComponent<VideoPlayer>();
        if (vp != null)
        {
            vp.loopPointReached += OnVideoEnd;
        }

        StartCoroutine(EnableSkipAfterDelay());
    }

    // 🆕 รอให้ LoadingScreen fade in เสร็จ + เวลาขั้นต่ำ
    IEnumerator EnableSkipAfterDelay()
    {
        // รอให้ LoadingScreen โหลดเสร็จ (fadeIn 1s + hold 1s + buffer)
        float waitTime = Mathf.Max(minPlayTime, 2.5f);
        
        yield return new WaitForSecondsRealtime(waitTime);
        
        canSkip = true;
        Debug.Log($"✅ Skip enabled after {waitTime}s");
    }

    void Update()
    {
        if (isEnding) return;
        if (!canSkip) return;

        // 🆕 ป้องกัน spam กด
        if (Time.unscaledTime - lastSkipAttempt < skipCooldown)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            lastSkipAttempt = Time.unscaledTime;
            
            float elapsed = Time.unscaledTime - sceneStartTime;
            Debug.Log($"⏩ Skip | elapsed={elapsed:F2}s | videoTime={vp?.time:F2}");
            
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
        if (isEnding) return;

        if (LoadingScreen.Instance != null)
        {
            StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
        }



        isEnding = true;

        Debug.Log("🚪 EndCutscene called");

        // ทำความสะอาด
        StopAllCoroutines();
        
        if (vp != null)
        {
            vp.loopPointReached -= OnVideoEnd;
            vp.Stop();
        }

        Time.timeScale = 1f;
        PauseController.isPaused = false;

        // 🔴 เช็คว่า LoadingScreen พร้อมหรือยัง
        if (LoadingScreen.Instance != null)
        {
            if (!LoadingScreen.Instance.IsLoading)
            {
                StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
            }
            else
            {
                Debug.LogWarning("⏳ LoadingScreen is still loading, waiting...");
                StartCoroutine(WaitAndLoad());
            }
        }
        else
        {
            Debug.LogError("❌ LoadingScreen.Instance is null!");
            // Fallback
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }

    // 🆕 รอให้ LoadingScreen พร้อม (กรณี edge case)
    IEnumerator WaitAndLoad()
    {
        float timeout = 3f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (LoadingScreen.Instance != null && !LoadingScreen.Instance.IsLoading)
            {
                Debug.Log("✅ LoadingScreen ready, loading scene now");
                yield return StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Timeout
        Debug.LogError("❌ LoadingScreen timeout! Using fallback");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

    private void OnDestroy()
    {
        if (vp != null)
        {
            vp.loopPointReached -= OnVideoEnd;
        }
    }
}