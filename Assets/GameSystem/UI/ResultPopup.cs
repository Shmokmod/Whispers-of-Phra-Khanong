using NUnit.Framework.Internal;
using TMPro;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Collections.AllocatorManager;

public class ResultPopup : MonoBehaviour
{
    [Header("UI References")]
    public GameObject popupPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Image iconImage;
    public Button closeButton;

    [Header("Icons")]
    public Sprite correctIcon;
    public Sprite wrongIcon;
    public Sprite hintIcon;
    public Sprite completeIcon;

    [Header("Colors")]
    public Color correctColor = new Color(0.3f, 0.8f, 0.3f);
    public Color wrongColor = new Color(0.8f, 0.3f, 0.3f);
    public Color hintColor = new Color(0.3f, 0.6f, 0.9f);
    public Color completeColor = new Color(1f, 0.8f, 0.2f);

    private bool isShowing = false;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    public void Show(string title, string message, ResultType type)
    {
        if (popupPanel == null)
        {
            Debug.LogError("PopupPanel is not assigned!");
            return;
        }

        isShowing = true;

        // Set text
        if (titleText != null) titleText.text = title;
        if (messageText != null) messageText.text = message;

        // Set color and icon based on type
        switch (type)
        {
            case ResultType.Correct:
                if (iconImage != null) iconImage.sprite = correctIcon;
                if (titleText != null) titleText.color = correctColor;
                break;

            case ResultType.Wrong:
                if (iconImage != null) iconImage.sprite = wrongIcon;
                if (titleText != null) titleText.color = wrongColor;
                break;

            case ResultType.Hint:
                if (iconImage != null) iconImage.sprite = hintIcon;
                if (titleText != null) titleText.color = hintColor;
                break;

            case ResultType.PhaseComplete:
                if (iconImage != null) iconImage.sprite = completeIcon;
                if (titleText != null) titleText.color = completeColor;
                break;

            case ResultType.Duplicate:
                if (iconImage != null) iconImage.sprite = wrongIcon;
                if (titleText != null) titleText.color = Color.gray;
                break;
        }

        // Show popup
        popupPanel.SetActive(true);

        // Auto hide after 3 seconds (except phase complete)
        if (type != ResultType.PhaseComplete)
        {
            Invoke(nameof(Hide), 3f);
        }
    }

    public void Hide()
    {
        if (!isShowing) return;

        CancelInvoke(nameof(Hide));

        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }

        isShowing = false;
    }
}

// ต้องมี enum นี้ด้วย (ถ้ายังไม่มี)
public enum ResultType
{
    Correct,
    Wrong,
    Duplicate,
    Hint,
    PhaseComplete
}