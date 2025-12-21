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
            Debug.Log($"📊 Canvas Settings:");
            Debug.Log($"   Render Mode: {canvas.renderMode}");
            Debug.Log($"   Sort Order: {canvas.sortingOrder}");
            Debug.Log($"   Layer: {LayerMask.LayerToName(canvas.gameObject.layer)}");

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                Debug.Log($"   Canvas Scaler: {scaler.uiScaleMode}");
                Debug.Log($"   Reference Resolution: {scaler.referenceResolution}");

                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    Debug.LogWarning("⚠️ Canvas Scaler ไม่ถูกต้อง! กำลังแก้ไข...");
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                    Debug.Log("✅ Canvas Scaler แก้ไขเรียบร้อย!");
                }
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

            Debug.Log($"✅ ChoicesPanel repositioned:");
            Debug.Log($"   Anchored Position: {choiceRect.anchoredPosition}");
            Debug.Log($"   Size: {choiceRect.sizeDelta}");
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
            leftPortraitImage.gameObject.SetActive(true);

            if (leftPortraitCanvasGroup != null)
            {
                leftPortraitCanvasGroup.alpha = 1f;
                if (leftPortraitRect != null)
                    leftPortraitRect.localScale = Vector3.one;
            }
        }

        if (rightPortraitImage != null && rightSprite != null)
        {
            rightPortraitImage.sprite = rightSprite;
            rightPortraitImage.SetNativeSize();
            rightPortraitImage.gameObject.SetActive(true);

            if (rightPortraitCanvasGroup != null)
            {
                rightPortraitCanvasGroup.alpha = 1f;
                if (rightPortraitRect != null)
                    rightPortraitRect.localScale = Vector3.one;
            }
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
        Debug.Log($"🔍 === Creating Choice Button: '{choiceText}' ===");

        if (choiceBottonPrefab == null)
        {
            Debug.LogError("❌ choiceBottonPrefab is NULL!");
            return null;
        }

        Debug.Log($"   Prefab Name: {choiceBottonPrefab.name}");

        // เช็ค Prefab components
        Image prefabImage = choiceBottonPrefab.GetComponent<Image>();
        Button prefabButton = choiceBottonPrefab.GetComponent<Button>();
        RectTransform prefabRect = choiceBottonPrefab.GetComponent<RectTransform>();

        Debug.Log($"   📦 Prefab Components:");
        Debug.Log($"      ├─ Has Image: {prefabImage != null}");
        if (prefabImage != null)
        {
            Debug.Log($"      │  ├─ Sprite: {(prefabImage.sprite != null ? prefabImage.sprite.name : "NULL")}");
            Debug.Log($"      │  ├─ Color: {prefabImage.color}");
            Debug.Log($"      │  └─ Enabled: {prefabImage.enabled}");
        }
        Debug.Log($"      ├─ Has Button: {prefabButton != null}");
        Debug.Log($"      └─ Size: {(prefabRect != null ? prefabRect.sizeDelta.ToString() : "NULL")}");

        // สร้างปุ่ม
        GameObject choiceButton = Instantiate(choiceBottonPrefab, choiceContainer);

        Debug.Log($"   🎯 After Instantiate:");
        Debug.Log($"      ├─ Name: {choiceButton.name}");
        Debug.Log($"      ├─ Active: {choiceButton.activeSelf}");
        Debug.Log($"      ├─ Layer: {LayerMask.LayerToName(choiceButton.layer)}");

        // เช็ค Image หลัง Instantiate
        Image buttonImage = choiceButton.GetComponent<Image>();
        Debug.Log($"   🖼️ Button Image After Instantiate:");
        Debug.Log($"      ├─ Has Image: {buttonImage != null}");
        if (buttonImage != null)
        {
            Debug.Log($"      ├─ Sprite: {(buttonImage.sprite != null ? buttonImage.sprite.name : "NULL")}");
            Debug.Log($"      ├─ Color: {buttonImage.color}");
            Debug.Log($"      ├─ Enabled: {buttonImage.enabled}");
            Debug.Log($"      ├─ Alpha: {buttonImage.color.a}");
            Debug.Log($"      └─ Raycast Target: {buttonImage.raycastTarget}");
        }

        // เช็ค RectTransform
        RectTransform buttonRect = choiceButton.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            Debug.Log($"   📐 RectTransform:");
            Debug.Log($"      ├─ Size: {buttonRect.sizeDelta}");
            Debug.Log($"      ├─ Scale: {buttonRect.localScale}");
            Debug.Log($"      ├─ Anchored Pos: {buttonRect.anchoredPosition}");
            Debug.Log($"      └─ World Pos: {buttonRect.position}");
        }

        // ตั้งค่าข้อความ
        TMP_Text buttonText = choiceButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = choiceText;
            Debug.Log($"   📝 Text: '{buttonText.text}', Size: {buttonText.fontSize}, Color: {buttonText.color}");
        }
        else
        {
            Debug.LogError("❌ TMP_Text not found!");
        }

        // ตั้งค่า Button
        Button button = choiceButton.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(onClickAction);
            Debug.Log($"   🔘 Button Interactable: {button.interactable}, Transition: {button.transition}");

            if (!button.interactable)
            {
                button.interactable = true;
                Debug.LogWarning("⚠️ Button was not interactable - fixed!");
            }
        }

        choiceButton.SetActive(true);

        Debug.Log($"✅ === Button Creation Complete ===\n");

        return choiceButton;
    }
}