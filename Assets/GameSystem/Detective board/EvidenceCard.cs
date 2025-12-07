using GameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EvidenceCard : MonoBehaviour,
    IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Image cardBackground;
    public Image iconImage;
    public Image typeBorder;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public GameObject dropZoneHighlight;
    public GameObject glowEffect;

    [Header("Colors")]
    public Color normalColor = new Color(0.9f, 0.9f, 0.95f);
    public Color validDropColor = new Color(0.7f, 1f, 0.7f);
    public Color invalidDropColor = new Color(1f, 0.7f, 0.7f);
    public Color usedColor = new Color(0.7f, 0.7f, 0.7f);

    [Header("Data")]
    public EvidenceData data;

    private RectTransform rectTransform;
    private Vector3 originalScale;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
    }

    public void Initialize(EvidenceData evidence)
    {
        data = evidence;

        nameText.text = data.itemName;
        descriptionText.text = data.description;
        iconImage.sprite = data.icon;

        typeBorder.color = GetTypeColor(data.type);

        UpdateVisualState();

        transform.localScale = originalScale;
    }

    Color GetTypeColor(EvidenceType type)
    {
        switch (type)
        {
            case EvidenceType.Photo: return new Color(0.5f, 0.8f, 1f);
            case EvidenceType.Document: return new Color(1f, 0.9f, 0.5f);
            case EvidenceType.Object: return new Color(0.8f, 0.5f, 1f);
            case EvidenceType.Digital: return new Color(0.5f, 1f, 0.8f);
            case EvidenceType.Witness: return new Color(1f, 0.7f, 0.7f);
            default: return Color.white;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        AudioManager.Instance?.PlaySFX("card_drop");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            StatementCard dragging = eventData.pointerDrag.GetComponent<StatementCard>();

            if (dragging != null)
            {
                bool isValid = DetectiveBoardManager.Instance.IsValidConnection(
                    dragging.data.id,
                    data.id
                );
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
    }

    void UpdateVisualState()
    {
        cardBackground.color = data.isUsed ? usedColor : normalColor;
    }

    public void SetUsed()
    {
        data.isUsed = true;
        UpdateVisualState();

        canvasGroup.alpha = 0.6f;
    }

    CanvasGroup canvasGroup
    {
        get
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }
    private CanvasGroup _canvasGroup;
}