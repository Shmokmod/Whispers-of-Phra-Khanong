using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueController : MonoBehaviour
{
    public static DialogueController instance { get; private set; }

    [Header("UI Elements")]
    public GameObject dialogueUI;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Transform choiceContainer;
    public GameObject choiceBottonPrefab;

    [Header("Portrait System - Left")]
    public Image leftPortraitImage;
    public CanvasGroup leftPortraitCanvasGroup;
    public RectTransform leftPortraitRect;

    [Header("Portrait System - Right")]
    public Image rightPortraitImage;
    public CanvasGroup rightPortraitCanvasGroup;
    public RectTransform rightPortraitRect;

    [Header("Portrait Settings")]
    public float fadeDuration = 0.3f;
    public float activeSpeakerAlpha = 1f;
    public float inactiveSpeakerAlpha = 0.5f;
    public Vector2 activeScale = new Vector2(1.05f, 1.05f);
    public Vector2 inactiveScale = Vector2.one;

    [HideInInspector] public Image npcPortraitImage;

    private SpeakerPosition currentActiveSpeaker = SpeakerPosition.None;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        npcPortraitImage = leftPortraitImage;
    }

    void Start()
    {
        // ✅ ซ่อน GameObject ตอนเริ่มต้น
        if (leftPortraitImage != null)
        {
            leftPortraitImage.gameObject.SetActive(false);
            if (leftPortraitCanvasGroup != null)
                leftPortraitCanvasGroup.alpha = 0f;
        }

        if (rightPortraitImage != null)
        {
            rightPortraitImage.gameObject.SetActive(false);
            if (rightPortraitCanvasGroup != null)
                rightPortraitCanvasGroup.alpha = 0f;
        }
    }

    public void ShowDialogue(bool Show)
    {
        dialogueUI.SetActive(Show);
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
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

    public void SetSpeakerName(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    public void SetupPortraits(Sprite leftSprite, Sprite rightSprite)
    {
        Debug.Log("=== SetupPortraits Called ===");

        if (leftPortraitImage != null && leftSprite != null)
        {
            leftPortraitImage.sprite = leftSprite;
            leftPortraitImage.SetNativeSize();
            leftPortraitImage.gameObject.SetActive(true); // ✅ Force Active

            Debug.Log($"Left Portrait Set: {leftSprite.name}");

            if (leftPortraitCanvasGroup != null)
            {
                leftPortraitCanvasGroup.alpha = 1f; // ✅ ทดสอบให้เด่นเลย
                Debug.Log($"Left Alpha: {leftPortraitCanvasGroup.alpha}");

                if (leftPortraitRect != null)
                {
                    leftPortraitRect.localScale = Vector3.one; // ✅ Scale ปกติ
                    Debug.Log($"Left Scale: {leftPortraitRect.localScale}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Left Portrait Image or Sprite is NULL!");
        }

        if (rightPortraitImage != null && rightSprite != null)
        {
            rightPortraitImage.sprite = rightSprite;
            rightPortraitImage.SetNativeSize();
            rightPortraitImage.gameObject.SetActive(true); // ✅ Force Active

            Debug.Log($"Right Portrait Set: {rightSprite.name}");

            if (rightPortraitCanvasGroup != null)
            {
                rightPortraitCanvasGroup.alpha = 1f; // ✅ ทดสอบให้เด่นเลย
                Debug.Log($"Right Alpha: {rightPortraitCanvasGroup.alpha}");

                if (rightPortraitRect != null)
                {
                    rightPortraitRect.localScale = Vector3.one; // ✅ Scale ปกติ
                    Debug.Log($"Right Scale: {rightPortraitRect.localScale}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Right Portrait Image or Sprite is NULL!");
        }
    }

    public void SetActiveSpeaker(SpeakerPosition position)
    {
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

    void HighlightSpeaker(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        if (canvasGroup == null) return;

        if (Application.isPlaying && canvasGroup.gameObject.activeInHierarchy)
        {
            canvasGroup.DOFade(activeSpeakerAlpha, fadeDuration).SetUpdate(true);
            if (rectTransform != null)
                rectTransform.DOScale(activeScale, fadeDuration).SetUpdate(true);
        }
        else
        {
            canvasGroup.alpha = activeSpeakerAlpha;
            if (rectTransform != null)
                rectTransform.localScale = activeScale;
        }
    }

    void FadeSpeaker(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        if (canvasGroup == null) return;

        if (Application.isPlaying && canvasGroup.gameObject.activeInHierarchy)
        {
            canvasGroup.DOFade(inactiveSpeakerAlpha, fadeDuration).SetUpdate(true);
            if (rectTransform != null)
                rectTransform.DOScale(inactiveScale, fadeDuration).SetUpdate(true);
        }
        else
        {
            canvasGroup.alpha = inactiveSpeakerAlpha;
            if (rectTransform != null)
                rectTransform.localScale = inactiveScale;
        }
    }

    public void HideAllPortraits()
    {
        // ✅ ซ่อน GameObject แทนการใช้ Alpha
        if (leftPortraitImage != null)
        {
            if (Application.isPlaying && leftPortraitImage.gameObject.activeInHierarchy)
            {
                leftPortraitCanvasGroup?.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    leftPortraitImage.gameObject.SetActive(false);
                });
            }
            else
            {
                leftPortraitImage.gameObject.SetActive(false);
            }
        }

        if (rightPortraitImage != null)
        {
            if (Application.isPlaying && rightPortraitImage.gameObject.activeInHierarchy)
            {
                rightPortraitCanvasGroup?.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    rightPortraitImage.gameObject.SetActive(false);
                });
            }
            else
            {
                rightPortraitImage.gameObject.SetActive(false);
            }
        }

        currentActiveSpeaker = SpeakerPosition.None;
    }

    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public GameObject CreateChoiceButton(string choiceText, UnityEngine.Events.UnityAction onClickAction)
    {
        GameObject choiceButton = Instantiate(choiceBottonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClickAction);
        return choiceButton;
    }
}