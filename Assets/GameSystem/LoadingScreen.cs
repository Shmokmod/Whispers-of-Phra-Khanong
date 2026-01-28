using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    // ==================================================
    // Singleton
    // ==================================================
    public static LoadingScreen Instance;

    // ==================================================
    // UI References
    // ==================================================
    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text loadingText;
    [SerializeField] private GameObject loadingSpinner;
    [SerializeField] private Canvas fadeCanvas;

    // ==================================================
    // Settings
    // ==================================================
    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float minFadeOutDuration = 0.5f;
    [SerializeField] private float spinnerSpeed = 200f;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private float holdBlackAfterLoad = 1f;
    [SerializeField] private float defaultFadeDuration = 1.5f;
    [SerializeField] private float loadingTimeout = 2f;
    [SerializeField]
    private AnimationCurve fadeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    // ==================================================
    // State
    // ==================================================
    public bool IsLoading { get; private set; } = false;
    private bool isFading = false;
    private bool isCompleting = false; // 🆕 กำลังทำ CompleteLoadingSequence
    private Coroutine currentLoadCoroutine = null;

    // ==================================================
    // Unity Lifecycle
    // ==================================================
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DebugLog("⚠️ Duplicate LoadingScreen detected - destroying");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ValidateReferences();
        SetupCanvas();
        InitializeFadeImage();

        DebugLog("LoadingScreen initialized");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoadedReset;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedReset;
    }

    private void Update()
    {
        if (IsLoading && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(
                0f, 0f, -spinnerSpeed * Time.deltaTime
            );
        }

        // Debug keys
        if (debugMode && Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log($"===== LoadingScreen State =====");
            Debug.Log($"IsLoading: {IsLoading}");
            Debug.Log($"isFading: {isFading}");
            Debug.Log($"isCompleting: {isCompleting}");
            Debug.Log($"currentLoadCoroutine: {(currentLoadCoroutine != null ? "Active" : "NULL")}");
            Debug.Log($"fadeImage.alpha: {fadeImage?.color.a}");
            Debug.Log($"Canvas order: {fadeCanvas?.sortingOrder}");
            Debug.Log($"==============================");
        }

        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            float alpha = fadeImage != null ? fadeImage.color.a : -1f;
            int order = fadeCanvas != null ? fadeCanvas.sortingOrder : -999;
            Debug.Log($"🎨 Alpha: {alpha}, IsLoading: {IsLoading}, Canvas Order: {order}");
        }

        // 🆕 กด R เพื่อ Force Reset
        if (debugMode && Input.GetKeyDown(KeyCode.R))
        {
            Debug.LogWarning("🔧 Manual Force Reset triggered!");
            ForceReset();
        }
    }

    // ==================================================
    // Scene Events
    // ==================================================
    private void OnSceneLoadedReset(Scene scene, LoadSceneMode mode)
    {
        DebugLog($"🎬 Scene Loaded: {scene.name}, IsLoading: {IsLoading}");

        // 🆕 ถ้ากำลังทำ CompleteLoadingSequence ห้ามรบกวน!
        if (isCompleting)
        {
            DebugLog("⏸️ Completing load sequence, don't interfere");
            return;
        }

        // Don't interfere if we're actively loading
        if (IsLoading && currentLoadCoroutine != null)
        {
            DebugLog("✅ Active load in progress, letting it finish");
            return;
        }

        // If somehow we got stuck, force reset
        if (IsLoading && currentLoadCoroutine == null)
        {
            Debug.LogWarning("⚠️ IsLoading=true but no active coroutine, force resetting");
            ForceReset();
        }
    }

    // ==================================================
    // Public API
    // ==================================================

    public void ForceResetIfStuck()
    {
        if (IsLoading)
        {
            Debug.LogWarning("🔧 Force reset called from external script");
            ForceReset();
        }
        else
        {
            DebugLog("✅ No need to reset - not loading");
        }
    }

    public IEnumerator LoadScene(string sceneName)
    {
        // Timeout protection
        if (IsLoading)
        {
            Debug.LogWarning($"⚠️ Already loading! Waiting for current load to finish or timeout...");

            float waitTime = 0f;
            while (IsLoading && waitTime < loadingTimeout)
            {
                waitTime += Time.unscaledDeltaTime;
                yield return null;
            }

            if (IsLoading)
            {
                Debug.LogWarning($"⏱️ Loading stuck for {loadingTimeout}s! Force resetting...");
                ForceReset();
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        DebugLog($"🚀 Start loading: {sceneName}");

        currentLoadCoroutine = StartCoroutine(LoadSceneInternal(sceneName));
        yield return currentLoadCoroutine;
    }

    private IEnumerator LoadSceneInternal(string sceneName)
    {
        BeginLoading();

        // Fade OUT
        yield return Fade(0f, 1f, defaultFadeDuration);
        DebugLog("🌑 Screen is BLACK");

        // Load Scene
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        if (op == null)
        {
            Debug.LogError($"❌ Cannot load scene: {sceneName}");
            ForceReset();
            currentLoadCoroutine = null;
            yield break;
        }

        while (!op.isDone)
        {
            yield return null;
        }

        DebugLog($"📦 Scene '{sceneName}' loaded");

        yield return StartCoroutine(CompleteLoadingSequence());

        currentLoadCoroutine = null;
    }


    public void BeginLoading()
    {
        // 🆕 ป้องกันเรียกซ้ำ
        if (IsLoading)
        {
            Debug.LogWarning("⚠️ BeginLoading called but already loading!");
            return;
        }

        IsLoading = true;

        if (fadeCanvas != null)
        {
            fadeCanvas.sortingOrder = 9999;
            DebugLog($"Canvas Order raised to: {fadeCanvas.sortingOrder}");
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        Time.timeScale = 1f;
        DebugLog("BeginLoading called");
    }

    // ==================================================
    // Loading Flow
    // ==================================================
    private IEnumerator CompleteLoadingSequence()
    {
        isCompleting = true; // 🆕 ป้องกันถูกรบกวน
        DebugLog("🎬 CompleteLoadingSequence START");

        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSecondsRealtime(0.3f);

        DebugLog("☀️ Fade IN start");
        yield return Fade(1f, 0f, defaultFadeDuration);

        // 🆕 รออีก frame เพื่อให้แน่ใจว่า Fade เสร็จ
        yield return new WaitForEndOfFrame();

        if (fadeCanvas != null)
        {
            fadeCanvas.sortingOrder = -1;
            DebugLog("Canvas Order reset to -1");
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            DebugLog($"Fade alpha set to: {fadeImage.color.a}");
        }

        IsLoading = false;
        isFading = false;
        isCompleting = false; // 🆕 เสร็จแล้ว

        DebugLog("✅ Load Complete");
    }

    // ==================================================
    // Fade
    // ==================================================
    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogError("⚠️ fadeImage is NULL!");
            yield break;
        }

        if (isFading)
        {
            Debug.LogWarning($"⚠️ Already fading! Ignoring {from}→{to}");
            yield break;
        }

        isFading = true;

        Debug.Log($"🎨 Fade {from} → {to} ({duration}s)");

        float elapsed = 0f;
        Color c = fadeImage.color;
        c.a = from;
        fadeImage.color = c;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curve = fadeCurve.Evaluate(t);
            c.a = Mathf.Lerp(from, to, curve);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        isFading = false;
        Debug.Log($"✅ Fade COMPLETE: {to}");
    }

    // ==================================================
    // Safety / Recovery
    // ==================================================
    private void ForceReset()
    {
        if (fadeCanvas != null)
            fadeCanvas.sortingOrder = -1;

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        IsLoading = false;
        isFading = false;
        isCompleting = false; // 🆕
        currentLoadCoroutine = null;

        DebugLog("🔄 ForceReset complete");
    }

    // ==================================================
    // Setup & Utils
    // ==================================================
    private void SetupCanvas()
    {
        if (fadeCanvas == null && fadeImage != null)
            fadeCanvas = fadeImage.GetComponentInParent<Canvas>();

        if (fadeCanvas != null)
        {
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = -1;
        }
    }

    private void InitializeFadeImage()
    {
        if (fadeImage == null) return;

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
        }

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void ValidateReferences()
    {
        if (!fadeImage) Debug.LogError("❌ FadeImage not assigned!");
        if (!loadingPanel) Debug.LogError("❌ LoadingPanel not assigned!");
    }

    private void DebugLog(string msg)
    {
        if (debugMode)
            Debug.Log($"[LoadingScreen] {msg}");
    }
}