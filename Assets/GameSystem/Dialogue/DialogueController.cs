using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    /* =========================================================
     *  Singleton
     * ========================================================= */
    public static DialogueController instance { get; private set; }

    /* =========================================================
     *  UI Elements
     * ========================================================= */
    [Header("UI Elements")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    /* =========================================================
     *  Portrait System - Left
     * ========================================================= */
    [Header("Portrait System - Left")]
    public Image leftPortraitImage;
    public CanvasGroup leftPortraitCanvasGroup;
    public RectTransform leftPortraitRect;

    /* =========================================================
     *  Portrait System - Right
     * ========================================================= */
    [Header("Portrait System - Right")]
    public Image rightPortraitImage;
    public CanvasGroup rightPortraitCanvasGroup;
    public RectTransform rightPortraitRect;

    /* =========================================================
     *  Portrait Settings
     * ========================================================= */
    [Header("Portrait Settings")]
    public float fadeDuration = 0.3f;
    public float activeSpeakerAlpha = 1f;
    public float inactiveSpeakerAlpha = 0.5f;
    public Vector2 activeScale = new Vector2(1.05f, 1.05f);
    public Vector2 inactiveScale = Vector2.one;

    /* =========================================================
     *  Runtime
     * ========================================================= */
    [HideInInspector] public Image npcPortraitImage;
    private SpeakerPosition currentActiveSpeaker = SpeakerPosition.None;



    public static bool IsDialogueActive { get; private set; }

    /* =========================================================
     *  Unity Life Cycle
     * ========================================================= */
    void Awake()
    {
        Debug.Log("[Dialogue] Awake - Scene: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        // ⬇️ เพิ่มการจัดการ Singleton แบบไม่ DontDestroyOnLoad
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[Dialogue] Duplicate instance found - destroying old instance");
            Destroy(instance.gameObject);
        }


        if (SceneManager.GetActiveScene().name.Contains("Mainmenu"))
        {
            gameObject.SetActive(false);
            return;
        }


        instance = this;

        // ⬇️ เพิ่มการ initialize พื้นฐาน
        if (leftPortraitImage != null)
            npcPortraitImage = leftPortraitImage;

        Debug.Log("[Dialogue] Awake completed - instance set");
    }

    void Start()
    {
        Debug.Log("[Dialogue] Start - initializing UI");

        // ซ่อน Portrait ตอนเริ่ม
        InitPortrait(leftPortraitImage, leftPortraitCanvasGroup);
        InitPortrait(rightPortraitImage, rightPortraitCanvasGroup);

        // ⬇️ ซ่อน Dialogue UI ตอนเริ่ม
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
            IsDialogueActive = false;

        }

        Debug.Log("[Dialogue] Start completed");
    }

    void OnDestroy()
    {
        Debug.Log("[Dialogue] OnDestroy called");

        // ⬇️ ทำความสะอาด instance
        if (instance == this)
        {
            instance = null;
        }
    }

    /* =========================================================
     *  Dialogue Core
     * ========================================================= */
    public void ShowDialogue(bool show)
    {
        Debug.Log($"[Dialogue] ShowDialogue({show})");

        if (dialogueUI != null)
        {
            dialogueUI.SetActive(show);
            IsDialogueActive = true;
        }
        else
        {
            Debug.LogError("[Dialogue] dialogueUI is NULL!");
        }
    }

    public void SetDialogueText(string text)
    {
        Debug.Log($"[Dialogue] SetDialogueText called | UI Active = {dialogueUI?.activeSelf}");
        Debug.Log($"[Dialogue] Text = \"{text}\"");

        if (dialogueUI != null && !dialogueUI.activeSelf)
        {
            Debug.LogWarning("[Dialogue] DialogueUI is INACTIVE when setting text");
        }

        if (dialogueText != null)
        {
            dialogueText.text = text;
        }
        else
        {
            Debug.LogError("[Dialogue] dialogueText is NULL!");
        }
    }

    public void SetSpeakerName(string name)
    {
        Debug.Log($"[Dialogue] SetSpeakerName = {name}");
        if (nameText != null)
            nameText.text = name;
    }

    public void ResumeDialogue()
    {
        Debug.Log("[Dialogue] ResumeDialogue");
        ShowDialogue(true);
    }

    /* =========================================================
     *  Portrait Control
     * ========================================================= */
    void InitPortrait(Image img, CanvasGroup cg)
    {
        if (img != null)
        {
            img.gameObject.SetActive(false);
            if (cg != null) cg.alpha = 0f;
        }
    }

    public void SetNPCinfo(string npcName, Sprite portrait)
    {
        Debug.Log($"[Dialogue] SetNPCinfo: {npcName}");
        SetSpeakerName(npcName);

        if (leftPortraitImage != null)
        {
            leftPortraitImage.sprite = portrait;
            npcPortraitImage = leftPortraitImage;
        }
    }

    public void SetupPortraits(Sprite leftSprite, Sprite rightSprite)
    {
        Debug.Log("[Dialogue] SetupPortraits");

        SetupSinglePortrait(leftPortraitImage, leftPortraitCanvasGroup, leftPortraitRect, leftSprite, "Left");
        SetupSinglePortrait(rightPortraitImage, rightPortraitCanvasGroup, rightPortraitRect, rightSprite, "Right");
    }

    void SetupSinglePortrait(Image img, CanvasGroup cg, RectTransform rt, Sprite sprite, string side)
    {
        if (img == null || sprite == null)
        {
            Debug.LogWarning($"[Dialogue] {side} portrait missing");
            return;
        }

        Debug.Log($"[Dialogue] {side} portrait = {sprite.name}");
        img.sprite = sprite;
        img.SetNativeSize();
        img.gameObject.SetActive(true);

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = Vector3.one;
    }

    public void SetActiveSpeaker(SpeakerPosition position)
    {
        Debug.Log($"[Dialogue] SetActiveSpeaker = {position}");

        if (position == currentActiveSpeaker)
            return;

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

            case SpeakerPosition.None:
                FadeSpeaker(leftPortraitCanvasGroup, leftPortraitRect);
                FadeSpeaker(rightPortraitCanvasGroup, rightPortraitRect);
                break;
        }
    }

    void HighlightSpeaker(CanvasGroup cg, RectTransform rt)
    {
        Debug.Log("[Dialogue] HighlightSpeaker");
        if (cg == null) return;

        cg.DOFade(activeSpeakerAlpha, fadeDuration).SetUpdate(true);
        if (rt != null) rt.DOScale(activeScale, fadeDuration).SetUpdate(true);
    }

    void FadeSpeaker(CanvasGroup cg, RectTransform rt)
    {
        Debug.Log("[Dialogue] FadeSpeaker");
        if (cg == null) return;

        cg.DOFade(inactiveSpeakerAlpha, fadeDuration).SetUpdate(true);
        if (rt != null) rt.DOScale(inactiveScale, fadeDuration).SetUpdate(true);
    }

    public void HideAllPortraits()
    {
        Debug.Log("[Dialogue] HideAllPortraits");

        if (leftPortraitImage != null) leftPortraitImage.gameObject.SetActive(false);
        if (rightPortraitImage != null) rightPortraitImage.gameObject.SetActive(false);

        currentActiveSpeaker = SpeakerPosition.None;
    }

    /* =========================================================
     *  Choices
     * ========================================================= */
    public void ClearChoices()
    {
        Debug.Log("[Dialogue] ClearChoices");

        if (choiceContainer == null)
        {
            Debug.LogError("[Dialogue] choiceContainer is NULL!");
            return;
        }

        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
    {
        Debug.Log($"[Dialogue] CreateChoiceButton = {choiceText}");

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("[Dialogue] choiceButtonPrefab is NULL!");
            return null;
        }

        if (choiceContainer == null)
        {
            Debug.LogError("[Dialogue] choiceContainer is NULL!");
            return null;
        }

        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClickAction);

        return choiceButton;
    }
}