using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectiveBookUI : MonoBehaviour
{
    public static DetectiveBookUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject bookPanel;
    public TMP_Text titleText;
    public TMP_Text contentText;
    public TMP_Text pageNumberText;
    public Image noteImage; // ✅ เพิ่ม: รูปภาพประกอบ

    [Header("Navigation Buttons")]
    public Button prevButton;
    public Button nextButton;
    public Button closeButton;

    [Header("Audio")]
    public AudioClip pageFlipSound;
    private AudioSource audioSource;

    [Header("Animation (Optional)")]
    public float fadeSpeed = 5f;
    private CanvasGroup canvasGroup;

    [Header("Runtime Data")]
    private List<DetectiveNote> unlockedNotes = new List<DetectiveNote>();
    private int currentPageIndex = 0;
    private int lastViewedPageIndex = 0; // ✅ เก็บหน้าที่เปิดล่าสุด
    private bool hasNewNote = false; // ✅ มี Note ใหม่หรือไม่

    void Awake()
    {
        // ✅ ไม่ใช้ Singleton แบบเดิม ให้แต่ละ Scene มี UI ของตัวเอง
        Instance = this;

        if (bookPanel != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            canvasGroup = bookPanel.GetComponent<CanvasGroup>();

            if (canvasGroup == null)
                canvasGroup = bookPanel.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        // Setup buttons
        if (prevButton != null)
            prevButton.onClick.AddListener(PrevPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        if (closeButton != null)
            closeButton.onClick.AddListener(CloseBook);

        // Subscribe to unlock event
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnNoteUnlocked += OnNoteUnlockedHandler;
        }

        // ซ่อนตอนเริ่มต้น
        if (bookPanel != null)
            bookPanel.SetActive(false);
    }

    void Update()
    {
        // ✅ เช็คว่า bookPanel ยังมีอยู่ก่อน
        if (bookPanel == null)
            return;

        // กดปุ่ม B เพื่อเปิด/ปิด (เปลี่ยนได้ตามต้องการ)
        if (Input.GetKeyDown(KeyCode.B))
        {
            if (bookPanel.activeSelf)
                CloseBook();
            else
                OpenBook();
        }
    }

    // ==================== Open / Close ====================

    public void OpenBook()
    {
        // ✅ เช็ค null ทุกอย่างก่อนใช้
        if (bookPanel == null)
        {
            Debug.LogWarning("📘 Cannot open book: bookPanel is null");
            return;
        }

        RefreshNotesList();

        if (unlockedNotes.Count == 0)
        {
            Debug.Log("📘 ยังไม่มีเบาะแสที่ปลดล็อก!");
            return;
        }

        bookPanel.SetActive(true);

        // ✅ ถ้ามี Note ใหม่ → ไปหน้า Note ใหม่ล่าสุด
        if (hasNewNote)
        {
            currentPageIndex = unlockedNotes.Count - 1; // หน้าสุดท้าย
            hasNewNote = false; // Reset flag
            Debug.Log($"📖 Opening to NEW note (page {currentPageIndex + 1})");
        }
        // ✅ ถ้าไม่มี Note ใหม่ → กลับไปหน้าที่เปิดล่าสุด
        else
        {
            currentPageIndex = Mathf.Clamp(lastViewedPageIndex, 0, unlockedNotes.Count - 1);
            Debug.Log($"📖 Opening to LAST viewed page {currentPageIndex + 1}");
        }

        ShowCurrentPage();

        // Pause game
        PauseController.isPaused = true;
        Time.timeScale = 0f;
    }

    public void CloseBook()
    {
        // ✅ เช็ค null ก่อนปิด
        if (bookPanel == null)
            return;

        // ✅ เก็บหน้าที่เปิดอยู่ตอนปิด
        lastViewedPageIndex = currentPageIndex;
        Debug.Log($"📖 Closing book, saving page {lastViewedPageIndex + 1}");

        bookPanel.SetActive(false);

        // Unpause game
        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    // ==================== Page Navigation ====================

    public void NextPage()
    {
        if (currentPageIndex < unlockedNotes.Count - 1)
        {
            currentPageIndex++;
            ShowCurrentPage();
            PlayPageFlipSound();
        }
    }

    public void PrevPage()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            ShowCurrentPage();
            PlayPageFlipSound();
        }
    }

    void ShowCurrentPage()
    {
        if (unlockedNotes.Count == 0) return;

        var note = unlockedNotes[currentPageIndex];

        if (titleText != null)
            titleText.text = note.title;

        if (contentText != null)
            contentText.text = note.content;

        if (pageNumberText != null)
            pageNumberText.text = $"{currentPageIndex + 1} / {unlockedNotes.Count}";

        // ✅ แสดงรูปภาพ (ถ้ามี)
        if (noteImage != null)
        {
            if (note.noteImage != null)
            {
                noteImage.sprite = note.noteImage;
                noteImage.gameObject.SetActive(true);
            }
            else
            {
                noteImage.gameObject.SetActive(false); // ซ่อนถ้าไม่มีรูป
            }
        }

        // Update button interactability
        if (prevButton != null)
            prevButton.interactable = (currentPageIndex > 0);

        if (nextButton != null)
            nextButton.interactable = (currentPageIndex < unlockedNotes.Count - 1);
    }

    // ==================== Helpers ====================

    void RefreshNotesList()
    {
        if (DetectiveBookManager.Instance != null)
        {
            unlockedNotes = DetectiveBookManager.Instance.GetUnlockedNotesSorted();
        }
    }

    void PlayPageFlipSound()
    {
        if (pageFlipSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pageFlipSound);
        }
    }

    void OnNoteUnlockedHandler(DetectiveNote note)
    {
        Debug.Log($"✨ UI: New note unlocked - {note.title}");

        // ✅ ตั้งค่าว่ามี Note ใหม่
        hasNewNote = true;
    }

    void OnDestroy()
    {
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnNoteUnlocked -= OnNoteUnlockedHandler;
        }
    }

    // ==================== Public Helper ====================

    public void ShowNotification(string message)
    {
        Debug.Log($"📬 Notification: {message}");
        // TODO: แสดง UI notification ถ้าต้องการ
    }
}