using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController instance { get; private set; }

    [Header("UI Elements")]
    public GameObject cutsceneCanvas;
    public Image cutsceneImage;
    public CanvasGroup cutsceneCanvasGroup;

    [Header("Video Player")]
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;

    [Header("Skip Prompt")]
    public GameObject skipPrompt;
    public TMP_Text skipPromptText;

    private bool isPlayingCutscene = false;
    private bool canSkip = false;
    private System.Action onCutsceneComplete;
    private GraphicRaycaster cutsceneRaycaster; // เพิ่มตัวนี้

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // เก็บ reference ของ GraphicRaycaster
        if (cutsceneCanvas != null)
        {
            cutsceneRaycaster = cutsceneCanvas.GetComponent<GraphicRaycaster>();
        }
    }

    void Start()
    {
        // ซ่อนทุกอย่างตอนเริ่มต้น
        if (cutsceneCanvas != null)
            cutsceneCanvas.SetActive(false);

        if (videoDisplay != null)
            videoDisplay.gameObject.SetActive(false);

        if (skipPrompt != null)
            skipPrompt.SetActive(false);

        // Setup Video Player
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void Update()
    {
        // ตรวจสอบการกดข้าม
        if (isPlayingCutscene && canSkip)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                SkipCutscene();
            }
        }
    }

    /// <summary>
    /// เล่น Cutscene (รูปภาพ)
    /// </summary>
    public void PlayImageCutscene(CutsceneEvent cutsceneEvent, System.Action onComplete)
    {
        if (cutsceneEvent.cutsceneImage == null)
        {
            Debug.LogWarning("⚠️ Cutscene Image is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(ImageCutsceneRoutine(cutsceneEvent, onComplete));
    }

    /// <summary>
    /// เล่น Video Cutscene
    /// </summary>
    public void PlayVideoCutscene(CutsceneEvent cutsceneEvent, System.Action onComplete)
    {
        if (cutsceneEvent.videoClip == null || videoPlayer == null)
        {
            Debug.LogWarning("⚠️ Video Clip or Video Player is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(VideoCutsceneRoutine(cutsceneEvent, onComplete));
    }

    IEnumerator ImageCutsceneRoutine(CutsceneEvent cutscene, System.Action onComplete)
    {
        isPlayingCutscene = true;
        onCutsceneComplete = onComplete;

        // เปิด Canvas และ DISABLE Raycaster เพื่อไม่ให้บัง UI อื่น
        cutsceneCanvas.SetActive(true);
        if (cutsceneRaycaster != null)
            cutsceneRaycaster.enabled = false;

        cutsceneImage.sprite = cutscene.cutsceneImage;
        cutsceneImage.gameObject.SetActive(true);

        // Fade In
        if (cutscene.useFadeIn)
        {
            cutsceneCanvasGroup.alpha = 0f;
            yield return cutsceneCanvasGroup.DOFade(1f, cutscene.fadeDuration)
                .SetUpdate(true)
                .WaitForCompletion();
        }
        else
        {
            cutsceneCanvasGroup.alpha = 1f;
        }

        // แสดง Skip Prompt
        if (cutscene.imageDuration == 0f)
        {
            canSkip = true;
            ShowSkipPrompt("กด Space หรือคลิกเพื่อดำเนินการต่อ");

            // รอจนกว่าจะข้าม
            while (isPlayingCutscene)
                yield return null;
        }
        else
        {
            // รอตามเวลาที่กำหนด
            canSkip = true;
            ShowSkipPrompt("กด Space หรือคลิกเพื่อข้าม");

            yield return new WaitForSecondsRealtime(cutscene.imageDuration);

            if (isPlayingCutscene) // ถ้ายังไม่ถูกข้าม
                FinishCutscene(cutscene);
        }
    }

    IEnumerator VideoCutsceneRoutine(CutsceneEvent cutscene, System.Action onComplete)
    {
        isPlayingCutscene = true;
        onCutsceneComplete = onComplete;

        // เปิด Canvas และ DISABLE Raycaster
        cutsceneCanvas.SetActive(true);
        if (cutsceneRaycaster != null)
            cutsceneRaycaster.enabled = false;

        videoDisplay.gameObject.SetActive(true);
        cutsceneImage.gameObject.SetActive(false);

        // เตรียม Video Player
        videoPlayer.clip = cutscene.videoClip;
        videoPlayer.Prepare();

        // รอให้วิดีโอเตรียมพร้อม
        while (!videoPlayer.isPrepared)
            yield return null;

        // Fade In
        if (cutscene.useFadeIn)
        {
            cutsceneCanvasGroup.alpha = 0f;
            yield return cutsceneCanvasGroup.DOFade(1f, cutscene.fadeDuration)
                .SetUpdate(true)
                .WaitForCompletion();
        }
        else
        {
            cutsceneCanvasGroup.alpha = 1f;
        }

        // เล่นวิดีโอ
        videoPlayer.Play();

        // แสดง Skip Prompt (ถ้าสามารถข้ามได้)
        if (cutscene.canSkipVideo)
        {
            canSkip = true;
            ShowSkipPrompt("กด Space หรือคลิกเพื่อข้าม");
        }

        // รอจนกว่าวิดีโอจะจบ หรือถูกข้าม
        while (videoPlayer.isPlaying && isPlayingCutscene)
            yield return null;

        if (isPlayingCutscene) // ถ้าจบปกติ (ไม่ถูกข้าม)
            FinishCutscene(cutscene);
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // เรียกเมื่อวิดีโอจบ
        if (isPlayingCutscene)
        {
            CutsceneEvent dummyEvent = new CutsceneEvent
            {
                useFadeOut = true,
                fadeDuration = 0.5f
            };
            FinishCutscene(dummyEvent);
        }
    }

    void SkipCutscene()
    {
        if (!canSkip) return;

        StopAllCoroutines();

        // หยุดวิดีโอถ้ากำลังเล่น
        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        CutsceneEvent dummyEvent = new CutsceneEvent
        {
            useFadeOut = false
        };

        FinishCutscene(dummyEvent);
    }

    void FinishCutscene(CutsceneEvent cutscene)
    {
        StartCoroutine(FinishCutsceneRoutine(cutscene));
    }

    IEnumerator FinishCutsceneRoutine(CutsceneEvent cutscene)
    {
        canSkip = false;
        HideSkipPrompt();

        // Fade Out
        if (cutscene.useFadeOut)
        {
            yield return cutsceneCanvasGroup.DOFade(0f, cutscene.fadeDuration)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        // ปิดทุกอย่าง
        if (videoPlayer.isPlaying)
            videoPlayer.Stop();

        videoDisplay.gameObject.SetActive(false);
        cutsceneImage.gameObject.SetActive(false);

        // IMPORTANT: ปิด Canvas และรอ 1 frame ก่อน callback
        cutsceneCanvas.SetActive(false);
        isPlayingCutscene = false;

        // รอ 1 frame เพื่อให้แน่ใจว่า Canvas ปิดสมบูรณ์
        yield return null;

        // เรียก Callback
        onCutsceneComplete?.Invoke();
        onCutsceneComplete = null;
    }

    void ShowSkipPrompt(string message)
    {
        if (skipPrompt == null) return;

        skipPrompt.SetActive(true);
        if (skipPromptText != null)
            skipPromptText.text = message;
    }

    void HideSkipPrompt()
    {
        if (skipPrompt != null)
            skipPrompt.SetActive(false);
    }

    public bool IsPlayingCutscene()
    {
        return isPlayingCutscene;
    }
}