using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text loadingText;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private Canvas fadeCanvas;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float minFadeOutDuration = 0.5f;
    [SerializeField] private float spinnerSpeed = 200f;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float holdBlackAfterLoad = 1f;

    // ✅ ถูกต้อง
    public bool IsLoading { get; private set; } = false;
    private Coroutine currentFadeCoroutine = null;

    // -------------------- Unity --------------------
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);

        ValidateReferences();
        SetupCanvas();
        InitializeFadeImage();

        DebugLog("LoadingScreen initialized");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (IsLoading && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }

    // -------------------- Setup --------------------
    private void SetupCanvas()
    {
        if (fadeCanvas == null && fadeImage != null)
        {
            fadeCanvas = fadeImage.GetComponentInParent<Canvas>();
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999;
            DebugLog($"✅ Canvas setup complete - Sort Order: {fadeCanvas.sortingOrder}");
        }
    }

    private void InitializeFadeImage()
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;

            RectTransform rt = fadeImage.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                DebugLog("✅ FadeImage stretched to full screen");
            }
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    // -------------------- Scene Callback --------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsLoading) return;
        if (scene.name == "_PersistentManagers") return;

        DebugLog($"🎬 Scene Loaded: {scene.name}");

        // ✅ ให้ SpawnManager จัดการ Player ทั้งหมด
        // LoadingScreen แค่ทำ Fade In/Out

        StartCoroutine(CompleteLoadingSequence());
    }

    private IEnumerator CompleteLoadingSequence()
    {
        yield return new WaitForEndOfFrame();

        // รอให้ SpawnManager ทำงานเสร็จ
        yield return new WaitForSeconds(0.2f);

        DebugLog($"⏸ Hold black screen {holdBlackAfterLoad}s");
        yield return new WaitForSeconds(holdBlackAfterLoad);

        DebugLog("☀️ Fade IN start");
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        IsLoading = false;
        DebugLog("✅ Load Complete - UI hidden");
    }

    // -------------------- Public API --------------------
    public IEnumerator LoadScene(string sceneName)
    {
        yield return StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        DebugLog($"🚨 LoadSceneAsync called : {sceneName}");
        if (IsLoading)
        {
            DebugLog("⚠️ Already loading!");
            yield break;
        }

        IsLoading = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        DebugLog("🌑 Fade OUT start");
        float fadeStartTime = Time.time;
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        float fadeElapsed = Time.time - fadeStartTime;
        if (fadeElapsed < minFadeOutDuration)
        {
            yield return new WaitForSeconds(minFadeOutDuration - fadeElapsed);
        }

        DebugLog($"🔄 Loading scene: {sceneName}");
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        while (!op.isDone)
        {
            DebugLog($"Loading progress: {op.progress * 100}%");
            yield return null;
        }

        DebugLog("✅ Scene load complete, waiting for callback...");
    }

    // -------------------- Fade --------------------
    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
        {
            DebugLog("❌ FadeImage is null!");
            yield break;
        }

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        DebugLog($"🎨 Fading from {from} to {to} over {duration}s");

        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(from, to, t);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        DebugLog($"✅ Fade complete - Final alpha: {c.a}");
        currentFadeCoroutine = null;
    }

    // -------------------- Utils --------------------
    private void ValidateReferences()
    {
        if (!fadeImage)
            Debug.LogError("❌ FadeImage not assigned!");

        if (!loadingPanel)
            Debug.LogError("❌ LoadingPanel not assigned!");

        if (fadeImage != null)
        {
            Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("❌ FadeImage must be under a Canvas!");
            }
            else if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning("⚠️ Canvas should be Screen Space - Overlay for best results");
            }
        }
    }

    private void DebugLog(string msg)
    {
        if (debugMode)
            Debug.Log($"[LoadingScreen] {msg}");
    }

    // -------------------- Manual Test --------------------
    [ContextMenu("Test Fade Out")]
    private void TestFadeOut()
    {
        Debug.Log("=== TEST FADE OUT ===");
        DebugImageStatus();
        StartCoroutine(Fade(0f, 1f, fadeDuration));
    }

    [ContextMenu("Test Fade In")]
    private void TestFadeIn()
    {
        Debug.Log("=== TEST FADE IN ===");
        DebugImageStatus();
        StartCoroutine(Fade(1f, 0f, fadeDuration));
    }

    [ContextMenu("Debug Image Status")]
    private void DebugImageStatus()
    {
        if (fadeImage == null)
        {
            Debug.LogError("❌ fadeImage is NULL!");
            return;
        }

        Debug.Log($"FadeImage GameObject: {fadeImage.gameObject.name}");
        Debug.Log($"FadeImage Active: {fadeImage.gameObject.activeInHierarchy}");
        Debug.Log($"FadeImage Enabled: {fadeImage.enabled}");
        Debug.Log($"Current Color: {fadeImage.color}");
        Debug.Log($"Current Alpha: {fadeImage.color.a}");

        Canvas canvas = fadeImage.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log($"Canvas: {canvas.gameObject.name}");
            Debug.Log($"Canvas RenderMode: {canvas.renderMode}");
            Debug.Log($"Canvas SortOrder: {canvas.sortingOrder}");
            Debug.Log($"Canvas Active: {canvas.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ No Canvas found!");
        }

        RectTransform rt = fadeImage.GetComponent<RectTransform>();
        if (rt != null)
        {
            Debug.Log($"RectTransform Size: {rt.rect.size}");
            Debug.Log($"Anchors: Min={rt.anchorMin}, Max={rt.anchorMax}");
        }
    }

    [ContextMenu("Force Show Black Screen")]
    private void ForceShowBlack()
    {
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            Debug.Log("✅ Forced black screen ON");
        }
    }
}