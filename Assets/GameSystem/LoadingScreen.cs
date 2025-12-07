using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    private bool isLoading = false;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isLoading && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// ทำ Transition: Fade Out → Callback → Delay → Fade In
    /// ใช้สำหรับเปลี่ยนซีนหรือวาร์ป
    /// </summary>
    public IEnumerator FadeTransition(System.Action onFadeComplete, float delayAfterLoad = 1f)
    {
        if (isLoading)
            yield break;

        isLoading = true;

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // Fade to black
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // Execute callback (เช่น ย้าย Player)
        onFadeComplete?.Invoke();

        // รอให้ตำแหน่ง Player/Scene เสถียร
        yield return new WaitForSeconds(delayAfterLoad);

        // Fade to clear
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        isLoading = false;
    }

    /// <summary>
    /// เปลี่ยนข้อความ Loading
    /// </summary>
    public void SetLoadingText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Fade ค่าความโปร่งใสของภาพจาก start → end
    /// </summary>
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null)
            yield break;

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;

            yield return null;
        }

        // Apply final alpha
        color.a = endAlpha;
        fadeImage.color = color;
    }

    #endregion
}
