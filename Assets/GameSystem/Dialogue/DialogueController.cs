using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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

    [Header("🎬 Cutscene System")]
    public GameObject cutscenePanel;
    public Image cutsceneImage;
    public CanvasGroup cutsceneCanvasGroup;

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
        // ✅ เริ่มต้นด้วยการปิด portraits ทั้งหมด
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

        if (cutscenePanel != null)
        {
            cutscenePanel.SetActive(false);
            if (cutsceneCanvasGroup != null)
                cutsceneCanvasGroup.alpha = 0f;
        }

        DebugCanvasSettings();
        FixChoicesPanelPosition();
    }

    void DebugCanvasSettings()
    {
        Canvas canvas = dialogueUI.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }

    void FixChoicesPanelPosition()
    {
        RectTransform choiceRect = choiceContainer.GetComponent<RectTransform>();
        if (choiceRect != null)
        {
            choiceRect.anchorMin = new Vector2(0.5f, 0f);
            choiceRect.anchorMax = new Vector2(0.5f, 0f);
            choiceRect.pivot = new Vector2(0.5f, 0.5f);
            choiceRect.anchoredPosition = new Vector2(0f, 250f);
            choiceRect.sizeDelta = new Vector2(1000f, 100f);
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
        Debug.Log($"🖼️ SetNPCinfo: {npcName}, Portrait: {(portrait != null ? portrait.name : "NULL")}");

        SetSpeakerName(npcName);

        if (leftPortraitImage != null && portrait != null)
        {
            leftPortraitImage.sprite = portrait;
            leftPortraitImage.SetNativeSize();

            // ✅ CRITICAL FIX: บังคับเปิดและตั้งค่า alpha
            leftPortraitImage.gameObject.SetActive(true);
            leftPortraitImage.enabled = true;

            if (leftPortraitCanvasGroup != null)
            {
                leftPortraitCanvasGroup.alpha = 1f;
                Debug.Log($"   ✅ CanvasGroup alpha set to 1");
            }

            if (leftPortraitRect != null)
            {
                leftPortraitRect.localScale = Vector3.one;
            }

            npcPortraitImage = leftPortraitImage;

            Debug.Log($"   Portrait Status:");
            Debug.Log($"   - GameObject Active: {leftPortraitImage.gameObject.activeSelf}");
            Debug.Log($"   - Image Enabled: {leftPortraitImage.enabled}");
            Debug.Log($"   - Sprite: {leftPortraitImage.sprite?.name}");
            Debug.Log($"   - Color Alpha: {leftPortraitImage.color.a}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Cannot set portrait - Image or Sprite is NULL");
        }
    }

    public void SetSpeakerName(string name)
    {
        if (nameText != null)
            nameText.text = name;
    }

    public void SetupPortraits(Sprite leftSprite, Sprite rightSprite)
    {
        Debug.Log($"🖼️ === SetupPortraits Called ===");
        Debug.Log($"   Left Sprite: {(leftSprite != null ? leftSprite.name : "NULL")}");
        Debug.Log($"   Right Sprite: {(rightSprite != null ? rightSprite.name : "NULL")}");

        // ✅ LEFT PORTRAIT
        if (leftPortraitImage != null && leftSprite != null)
        {
            leftPortraitImage.sprite = leftSprite;
            leftPortraitImage.SetNativeSize();

            // ✅ บังคับเปิด GameObject
            leftPortraitImage.gameObject.SetActive(true);
            leftPortraitImage.enabled = true;

            // ✅ ตั้งค่า CanvasGroup
            if (leftPortraitCanvasGroup != null)
            {
                leftPortraitCanvasGroup.alpha = 1f;
                leftPortraitCanvasGroup.interactable = true;
                leftPortraitCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogWarning("⚠️ leftPortraitCanvasGroup is NULL!");
            }

            // ✅ ตั้งค่า Scale
            if (leftPortraitRect != null)
            {
                leftPortraitRect.localScale = Vector3.one;
            }

            Debug.Log($"   Left Portrait Setup:");
            Debug.Log($"   - Active: {leftPortraitImage.gameObject.activeSelf}");
            Debug.Log($"   - Enabled: {leftPortraitImage.enabled}");
            Debug.Log($"   - Alpha: {(leftPortraitCanvasGroup != null ? leftPortraitCanvasGroup.alpha : -1)}");
        }
        else
        {
            Debug.LogWarning("⚠️ Left portrait setup skipped - Image or Sprite NULL");
        }

        // ✅ RIGHT PORTRAIT
        if (rightPortraitImage != null && rightSprite != null)
        {
            rightPortraitImage.sprite = rightSprite;
            rightPortraitImage.SetNativeSize();

            // ✅ บังคับเปิด GameObject
            rightPortraitImage.gameObject.SetActive(true);
            rightPortraitImage.enabled = true;

            // ✅ ตั้งค่า CanvasGroup
            if (rightPortraitCanvasGroup != null)
            {
                rightPortraitCanvasGroup.alpha = 1f;
                rightPortraitCanvasGroup.interactable = true;
                rightPortraitCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogWarning("⚠️ rightPortraitCanvasGroup is NULL!");
            }

            // ✅ ตั้งค่า Scale
            if (rightPortraitRect != null)
            {
                rightPortraitRect.localScale = Vector3.one;
            }

            Debug.Log($"   Right Portrait Setup:");
            Debug.Log($"   - Active: {rightPortraitImage.gameObject.activeSelf}");
            Debug.Log($"   - Enabled: {rightPortraitImage.enabled}");
            Debug.Log($"   - Alpha: {(rightPortraitCanvasGroup != null ? rightPortraitCanvasGroup.alpha : -1)}");
        }
        else
        {
            Debug.LogWarning("⚠️ Right portrait setup skipped - Image or Sprite NULL");
        }

        Debug.Log($"✅ === SetupPortraits Complete ===\n");
    }

    public void SetActiveSpeaker(SpeakerPosition position)
    {
        Debug.Log($"📢 SetActiveSpeaker: {position}");

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

        // ✅ ตรวจสอบว่า GameObject active อยู่
        if (!canvasGroup.gameObject.activeSelf)
        {
            Debug.LogWarning($"⚠️ CanvasGroup GameObject is not active!");
            canvasGroup.gameObject.SetActive(true);
        }

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

        Debug.Log($"   Highlighted - Alpha: {canvasGroup.alpha}");
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

        Debug.Log($"   Faded - Alpha: {canvasGroup.alpha}");
    }

    public void HideAllPortraits()
    {
        Debug.Log("🔒 HideAllPortraits Called");

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

    public void ShowCutscene(Sprite cutsceneSprite, float duration, float fadeDuration, Action onComplete)
    {
        if (cutscenePanel == null || cutsceneImage == null || cutsceneCanvasGroup == null)
        {
            Debug.LogWarning("⚠️ Cutscene components not assigned!");
            onComplete?.Invoke();
            return;
        }

        if (cutsceneSprite == null)
        {
            Debug.LogWarning("⚠️ Cutscene sprite is null!");
            onComplete?.Invoke();
            return;
        }

        cutsceneImage.sprite = cutsceneSprite;
        cutscenePanel.SetActive(true);
        cutsceneCanvasGroup.alpha = 0f;

        Sequence cutsceneSequence = DOTween.Sequence();
        cutsceneSequence.SetUpdate(true);

        cutsceneSequence.Append(cutsceneCanvasGroup.DOFade(1f, fadeDuration))
                        .AppendInterval(duration)
                        .Append(cutsceneCanvasGroup.DOFade(0f, fadeDuration))
                        .OnComplete(() =>
                        {
                            cutscenePanel.SetActive(false);
                            onComplete?.Invoke();
                        });
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
        if (choiceBottonPrefab == null)
        {
            Debug.LogError("❌ choiceBottonPrefab is NULL!");
            return null;
        }

        GameObject choiceButton = Instantiate(choiceBottonPrefab, choiceContainer);

        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choiceText;
        }

        Button button = choiceButton.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(onClickAction);

            if (!button.interactable)
            {
                button.interactable = true;
            }
        }

        choiceButton.SetActive(true);

        return choiceButton;
    }
}