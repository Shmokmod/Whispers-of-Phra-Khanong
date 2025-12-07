using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance;

    [Header("UI References")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Text loadingText; // ถ้ามี (Optional)
    [SerializeField] private GameObject loadingSpinner; // ไอคอนหมุน (Optional)

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float spinnerSpeed = 200f; // ความเร็วหมุน

    private bool isLoading = false;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ซ่อนหน้าโหลดตอนเริ่ม
            if (loadingPanel != null)
                loadingPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // หมุน Spinner ถ้ากำลัง Loading
        if (isLoading && loadingSpinner != null)
        {
            loadingSpinner.transform.Rotate(0f, 0f, -spinnerSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Fade Out (ดำ) → รอให้ callback เสร็จ → Delay → Fade In (กลับ)
    /// </summary>
    public IEnumerator FadeTransition(System.Action onFadeComplete, float delayAfterLoad = 1f)
    {
        if (isLoading) yield break;
        isLoading = true;

        // แสดง Loading Panel
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        // === Fade Out (ดำทึบ) ===
        yield return StartCoroutine(Fade(0f, 1f, fadeDuration));

        // เรียก callback (เช่น การย้าย Player)
        onFadeComplete?.Invoke();

        // รอให้ตำแหน่งถูกต้อง 100%
        yield return new WaitForSeconds(delayAfterLoad);

        // === Fade In (กลับมาใส) ===
        yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

        // ซ่อน Loading Panel
        if (loadingPanel != null)
            loadingPanel.SetActive(false);

        isLoading = false;
    }

    /// <summary>
    /// Fade Image จาก alpha หนึ่งไปอีก alpha หนึ่ง
    /// </summary>
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color color = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            color.a = alpha;
            fadeImage.color = color;
            yield return null;
        }

        // ตั้งค่า alpha สุดท้ายให้แน่นอน
        color.a = endAlpha;
        fadeImage.color = color;
    }

    /// <summary>
    /// อัปเดตข้อความ Loading (Optional)
    /// </summary>
    public void SetLoadingText(string text)
    {
        if (loadingText != null)
            loadingText.text = text;
    }
}