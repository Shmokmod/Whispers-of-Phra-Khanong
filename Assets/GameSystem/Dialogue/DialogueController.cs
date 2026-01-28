// =========================================================
// DialogueController.cs
// ระบบควบคุม UI บทสนทนา, Portrait, Speaker, Choices
// =========================================================

using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================
    public static DialogueController instance { get; private set; }
    public static bool IsDialogueActive { get; private set; }

    // =========================================================
    // UI Elements
    // =========================================================
    [Header("UI Elements")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    // =========================================================
    // Portrait - Left
    // =========================================================
    [Header("Portrait System - Left")]
    public Image leftPortraitImage;
    public CanvasGroup leftPortraitCanvasGroup;
    public RectTransform leftPortraitRect;

    // =========================================================
    // Portrait - Right
    // =========================================================
    [Header("Portrait System - Right")]
    public Image rightPortraitImage;
    public CanvasGroup rightPortraitCanvasGroup;
    public RectTransform rightPortraitRect;

    // =========================================================
    // Portrait Settings
    // =========================================================
    [Header("Portrait Settings")]
    public float fadeDuration = 0.3f;
    public float activeSpeakerAlpha = 1f;
    public float inactiveSpeakerAlpha = 0.5f;
    public Vector2 activeScale = new Vector2(1.05f, 1.05f);
    public Vector2 inactiveScale = Vector2.one;

    // =========================================================
    // Runtime State
    // =========================================================
    [HideInInspector] public Image npcPortraitImage;
    private SpeakerPosition currentActiveSpeaker = SpeakerPosition.None;

    // =========================================================
    // Unity Lifecycle
    // =========================================================
    void Awake()
    {
        Debug.Log("[Dialogue] Awake - Scene: " + SceneManager.GetActiveScene().name);

        // Singleton (ไม่ใช้ DontDestroyOnLoad)
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[Dialogue] Duplicate instance found");
            Destroy(instance.gameObject);
        }

        if (SceneManager.GetActiveScene().name.Contains("Mainmenu"))
        {
            gameObject.SetActive(false);
            return;
        }

        instance = this;

        // default portrait
        if (leftPortraitImage != null)
            npcPortraitImage = leftPortraitImage;
    }

    void Start()
    {
        // ซ่อน portrait ตอนเริ่ม
        InitPortrait(leftPortraitImage, leftPortraitCanvasGroup);
        InitPortrait(rightPortraitImage, rightPortraitCanvasGroup);

        // ซ่อน UI
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
            IsDialogueActive = false;
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    // =========================================================
    // Dialogue Core
    // =========================================================
    public void ShowDialogue(bool show)
    {
        if (dialogueUI == null) return;
        dialogueUI.SetActive(show);
        IsDialogueActive = show;
    }

    public void HideDialogue()
    {
        if (dialogueUI != null)
            dialogueUI.SetActive(false);

        IsDialogueActive = false;
        currentActiveSpeaker = SpeakerPosition.None;
    }

    public void ResumeDialogue()
    {
        ShowDialogue(true);
    }

    public void SetDialogueText(string text)
    {
        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void SetSpeakerName(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    // =========================================================
    // Portrait Setup
    // =========================================================
    void InitPortrait(Image img, CanvasGroup cg)
    {
        if (img == null) return;
        img.gameObject.SetActive(false);
        if (cg != null) cg.alpha = 0f;
    }

    public void SetNPCinfo(string npcName, Sprite portrait)
    {
        SetSpeakerName(npcName);

        if (leftPortraitImage != null)
        {
            leftPortraitImage.sprite = portrait;
            npcPortraitImage = leftPortraitImage;
        }
    }

    public void SetupPortraits(Sprite leftSprite, Sprite rightSprite)
    {
        SetupSinglePortrait(leftPortraitImage, leftPortraitCanvasGroup, leftPortraitRect, leftSprite);
        SetupSinglePortrait(rightPortraitImage, rightPortraitCanvasGroup, rightPortraitRect, rightSprite);
    }

    void SetupSinglePortrait(Image img, CanvasGroup cg, RectTransform rt, Sprite sprite)
    {
        if (img == null || sprite == null) return;

        img.sprite = sprite;
        img.SetNativeSize();
        img.gameObject.SetActive(true);

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = Vector3.one;
    }

    public void HideAllPortraits()
    {
        if (leftPortraitImage != null) leftPortraitImage.gameObject.SetActive(false);
        if (rightPortraitImage != null) rightPortraitImage.gameObject.SetActive(false);
        currentActiveSpeaker = SpeakerPosition.None;
    }

    // =========================================================
    // Speaker Highlight
    // =========================================================
    public void SetActiveSpeaker(SpeakerPosition position)
    {
        if (position == currentActiveSpeaker) return;
        currentActiveSpeaker = position;

        switch (position)
        {
            case SpeakerPosition.Left:
                HighlightSpeaker(leftPortraitCanvasGroup, leftPortraitRect);
                FadeSpeaker(rightPortraitCanvasGroup, rightPortraitRect);
                break;

            case SpeakerPosition.Right:
                HighlightSpeaker(rightPortraitCanvasGroup, rightPortraitRect);
                FadeSpeaker(leftPortraitCanvasGroup, leftPortraitRect);
                break;

            default:
                FadeSpeaker(leftPortraitCanvasGroup, leftPortraitRect);
                FadeSpeaker(rightPortraitCanvasGroup, rightPortraitRect);
                break;
        }
    }

    void HighlightSpeaker(CanvasGroup cg, RectTransform rt)
    {
        if (cg == null) return;
        cg.DOFade(activeSpeakerAlpha, fadeDuration).SetUpdate(true);
        if (rt != null) rt.DOScale(activeScale, fadeDuration).SetUpdate(true);
    }

    void FadeSpeaker(CanvasGroup cg, RectTransform rt)
    {
        if (cg == null) return;
        cg.DOFade(inactiveSpeakerAlpha, fadeDuration).SetUpdate(true);
        if (rt != null) rt.DOScale(inactiveScale, fadeDuration).SetUpdate(true);
    }

    // =========================================================
    // Choices
    // =========================================================
    public void ClearChoices()
    {
        if (choiceContainer == null) return;
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
    {
        if (choiceButtonPrefab == null || choiceContainer == null) return null;

        GameObject btn = Instantiate(choiceButtonPrefab, choiceContainer);
        btn.GetComponentInChildren<TMP_Text>().text = choiceText;
        btn.GetComponent<Button>().onClick.AddListener(onClickAction);
        return btn;
    }
}
