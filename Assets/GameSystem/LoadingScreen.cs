using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using Debug = UnityEngine.Debug;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text loadingText;
    [SerializeField] private GameObject loadingSpinner;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float spinnerSpeed = 200f;
    [SerializeField] private bool debugMode = true;

    private bool isLoading = false;
    private Coroutine currentLoadingCoroutine = null;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ValidateReferences();

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            DebugLog("LoadingScreen initialized");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneFullyLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneFullyLoaded;
    }

    private void Update()
    {
        if (isLoading && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.L) && debugMode)
        {
            Debug.Log($"[LoadingScreen] isLoading: {isLoading}");
        }
    }
    #endregion

    #region Scene Load Callback
    private void OnSceneFullyLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugLog($"🎬 OnSceneFullyLoaded: {scene.name}, isLoading: {isLoading}");

        // ถ้ายัง loading อยู่ ให้เริ่ม fade in
        if (isLoading)
        {
            DebugLog("🔄 Starting post-load fade in sequence...");
            StartCoroutine(CompleteLoadingSequence());
        }
    }

    private IEnumerator CompleteLoadingSequence()
    {
        // รอให้ SpawnManager ทำงานเสร็จ
        DebugLog("⏳ Waiting for SpawnManager (7 frames)...");
        for (int i = 0; i < 7; i++)
        {
            yield return null;
        }

        SetLoadingText("Preparing...");
        yield return new WaitForSeconds(0.3f);

        // Fade in
        DebugLog("☀️ Fading in...");
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            DebugLog("📱 Loading panel deactivated");
        }

        yield return new WaitForEndOfFrame();

        isLoading = false;
        currentLoadingCoroutine = null;
        DebugLog("=== Scene Load Complete ===");
    }
    #endregion

    #region Validation
    private void ValidateReferences()
    {
        if (loadingPanel == null)
            Debug.LogError("❌ LoadingPanel is not assigned!");

        if (fadeImage == null)
            Debug.LogError("❌ FadeImage is not assigned!");
        else
        {
            // ❌ ลบบรรทัดนี้ออก - อย่า set alpha = 0 ตอน awake!
            // Color c = fadeImage.color;
            // c.a = 0f;
            // fadeImage.color = c;

            Canvas parentCanvas = fadeImage.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("❌ FadeImage must be child of Canvas!");
                Debug.LogError("💡 Current parent: " + (fadeImage.transform.parent ? fadeImage.transform.parent.name : "None"));
            }
            else
            {
                DebugLog($"✅ FadeImage is under Canvas: {parentCanvas.name}");

                if (parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    Debug.LogWarning($"⚠️ Canvas Render Mode is {parentCanvas.renderMode}, should be ScreenSpaceOverlay");
                }

                if (parentCanvas.sortingOrder < 100)
                {
                    Debug.LogWarning($"⚠️ Canvas Sorting Order is {parentCanvas.sortingOrder}, recommend 999");
                }
            }

            // ✅ เพิ่ม - ดู alpha ปัจจุบัน
            DebugLog($"✅ FadeImage validated - Current alpha: {fadeImage.color.a:F2}");
        }

        if (loadingText == null)
            Debug.LogWarning("⚠️ LoadingText is not assigned!");

        if (loadingSpinner == null)
            Debug.LogWarning("⚠️ LoadingSpinner is not assigned!");
    }
//```

//## ตั้งค่า FadeImage ใน Inspector:
//```
//FadeImage Component:
//Image → Color: Black(0, 0, 0, 255) ← Alpha = 255 (ทึบ)!
    #endregion

    #region Public Methods - Scene Loading
    public IEnumerator LoadSceneAsync(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("⚠️ Already loading!");
            yield break;
        }

        if (!SceneExists(sceneName))
        {
            Debug.LogError($"❌ Scene '{sceneName}' not found in Build Settings!");
            yield break;
        }

        DebugLog($"🎮 Async loading scene: {sceneName}");
        isLoading = true;
        SetLoadingText($"Loading {sceneName}...");

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Fade to black
        DebugLog("🌑 Fading to black...");
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        DebugLog("✅ Fade to black complete");

        // Start async load
        DebugLog("📦 Starting async scene load...");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        // แสดงความคืบหน้า
        while (asyncLoad.progress < 0.9f)
        {
            float progress = asyncLoad.progress / 0.9f;
            SetLoadingText($"Loading... {(progress * 100):F0}%");
            yield return null;
        }

        SetLoadingText("Almost there...");
        DebugLog("📦 Scene load at 90%, activating...");

        // Activate scene
        asyncLoad.allowSceneActivation = true;
        DebugLog("✅ Scene activation allowed");

        // รอให้ Scene โหลดเสร็จ
        while (!asyncLoad.isDone)
        {
            DebugLog($"⏳ Waiting for scene... progress: {asyncLoad.progress}");
            yield return null;
        }

        DebugLog("✅ AsyncOperation.isDone = true");

        // NOTE: จากจุดนี้ OnSceneFullyLoaded จะถูกเรียกและจัดการ Fade In ต่อ
        DebugLog("🎬 Waiting for OnSceneFullyLoaded callback...");
    }
    #endregion

    #region Public Methods - Basic Fade
    public IEnumerator FadeTransition(System.Action onFadeComplete, float delayAfterLoad = 0.5f)
    {
        if (isLoading)
        {
            Debug.LogWarning("⚠️ Loading already in progress!");
            yield break;
        }

        DebugLog("=== FadeTransition Started ===");
        isLoading = true;

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
            DebugLog("📱 Loading panel activated");
        }

        DebugLog("🌑 Starting fade OUT (to black)...");
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        DebugLog("✅ Fade OUT complete");

        DebugLog("⚙️ Executing callback...");
        try
        {
            onFadeComplete?.Invoke();
            DebugLog("✅ Callback executed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Error in callback: {e.Message}\n{e.StackTrace}");
        }

        DebugLog($"⏳ Waiting {delayAfterLoad} seconds...");
        yield return new WaitForSeconds(delayAfterLoad);

        DebugLog("☀️ Starting fade IN (to clear)...");
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));
        DebugLog("✅ Fade IN complete");

        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
            DebugLog("📱 Loading panel deactivated");
        }

        yield return new WaitForEndOfFrame();

        isLoading = false;
        DebugLog("=== FadeTransition Complete ===");
    }

    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
            DebugLog($"📝 Loading text set to: {text}");
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogError("❌ FadeImage is null! Cannot fade.");
            yield break;
        }

        DebugLog($"🎨 Fading from {startAlpha:F2} to {endAlpha:F2} over {duration}s");

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;

            yield return null;
        }

        color.a = endAlpha;
        fadeImage.color = color;
        DebugLog($"✅ Fade complete. Final alpha: {color.a:F2}");
    }

    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (name == sceneName)
                return true;
        }
        return false;
    }

    private void DebugLog(string message)
    {
        if (debugMode)
            Debug.Log($"[LoadingScreen] {message}");
    }
    #endregion

    #region Utility Methods
    public bool IsLoading()
    {
        return isLoading;
    }

    public void ShowLoadingPanel()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);
    }

    public void HideLoadingPanel()
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    public void ForceStopLoading()
    {
        StopAllCoroutines();
        isLoading = false;
        currentLoadingCoroutine = null;

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
        }

        DebugLog("⚠️ Force stopped loading");
    }
    #endregion
}