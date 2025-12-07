using DG.Tweening;
using GameSystem;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatementCard : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IDropHandler // ✅ เพิ่ม IDropHandler
{
    [Header("UI References")]
    public Image cardBackground;
    public Image portraitImage;
    public Image statusIcon;
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI contentText;
    public GameObject glowEffect;

    [Header("Colors")]
    public Color normalColor = new Color(0.95f, 0.95f, 0.95f);
    public Color hoverColor = Color.white;
    public Color verifiedColor = new Color(0.7f, 1f, 0.7f);
    public Color contradictedColor = new Color(1f, 0.7f, 0.7f);

    [Header("Settings")]
    public float hoverScale = 1.05f;
    public float dragAlpha = 0.7f;

    [Header("Data")]
    public StatementData data;

    public bool isLocked = false;
    private bool isDragging = false;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSiblingIndex;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        originalScale = transform.localScale;
    }

    public void Initialize(StatementData statement)
    {
        data = statement;

        speakerText.text = data.speakerName;
        contentText.text = data.text;
        portraitImage.sprite = data.portrait;

        UpdateVisualState();

        transform.localScale = originalScale;
    }

    // ✅ OnDrop - รับการ์ดที่ลากมาวาง
    public void OnDrop(PointerEventData eventData)
    {
        StatementCard draggedCard = eventData.pointerDrag?.GetComponent<StatementCard>();

        if (draggedCard != null && draggedCard != this)
        {
            // ลาก Statement มาวางที่ Statement นี้
            DetectiveBoardManager.Instance.CheckContradiction(draggedCard.data.id, data.id);
            AudioManager.Instance?.PlaySFX("card_drop");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        isDragging = true;

        // ✅ เก็บข้อมูลเดิม
        originalParent = transform.parent;
        originalPosition = rectTransform.position;
        originalSiblingIndex = transform.GetSiblingIndex();

        canvasGroup.alpha = dragAlpha;
        canvasGroup.blocksRaycasts = false;

        // ✅ ย้ายไปอยู่ใน Detective Board (ระดับเดียวกับ Container)
        // ทำให้ลากได้ข้ามทั้ง 2 หน้า
        Transform boardPanel = DetectiveBoardManager.Instance.boardUI.transform;
        transform.SetParent(boardPanel, true); // worldPositionStays = true
        transform.SetAsLastSibling();

        DetectiveBoardManager.Instance.OnCardStartDrag(this);
        AudioManager.Instance?.PlaySFX("card_pickup");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        // ✅ ลากแบบ World Position (ไม่มีปัญหา Container)
        rectTransform.position = eventData.position;

        DetectiveBoardManager.Instance.CheckHoverOverEvidence(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        isDragging = false;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // ✅ ตรวจสอบว่าวางบน Evidence หรือ Statement
        EvidenceCard targetEvidence = DetectiveBoardManager.Instance.GetHoveredEvidence();
        StatementCard targetStatement = GetHoveredStatement(eventData.position);

        if (targetEvidence != null)
        {
            // วางบน Evidence → ใช้ระบบเดิม (Connection)
            DetectiveBoardManager.Instance.CheckConnection(data.id, targetEvidence.data.id);
        }
        else if (targetStatement != null && targetStatement != this)
        {
            // วางบน Statement → ใช้ระบบใหม่ (Contradiction)
            DetectiveBoardManager.Instance.CheckContradiction(data.id, targetStatement.data.id);
        }
        else
        {
            ReturnToPosition();
        }

        // ✅ ย้ายกลับ Parent เดิม (Statement Container)
        transform.SetParent(originalParent, false);
        transform.SetSiblingIndex(originalSiblingIndex);

        DetectiveBoardManager.Instance.OnCardEndDrag();
    }

    void ReturnToPosition()
    {
        // ✅ กลับไปตำแหน่งเดิมด้วย Animation
        rectTransform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked || isDragging) return;

        cardBackground.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked || isDragging) return;

        UpdateVisualState();
    }

    void UpdateVisualState()
    {
        if (data.isVerified)
        {
            cardBackground.color = verifiedColor;
            statusIcon.sprite = Resources.Load<Sprite>("Icons/Check");
            statusIcon.gameObject.SetActive(true);
            statusIcon.color = Color.green;
            isLocked = true;
        }
        else if (data.isContradicted)
        {
            cardBackground.color = contradictedColor;
            statusIcon.sprite = Resources.Load<Sprite>("Icons/X");
            statusIcon.gameObject.SetActive(true);
            statusIcon.color = Color.red;
            isLocked = true;
        }
        else
        {
            cardBackground.color = normalColor;
            statusIcon.gameObject.SetActive(false);
        }
    }

    public void SetVerified()
    {
        data.isVerified = true;
        UpdateVisualState();
        AudioManager.Instance?.PlaySFX("card_verified");
    }

    public void SetContradicted()
    {
        data.isContradicted = true;
        UpdateVisualState();
        AudioManager.Instance?.PlaySFX("card_contradicted");
    }

    // ✅ ฟังก์ชันหา Statement ที่ hover อยู่ (เหลือแค่อันเดียว)
    StatementCard GetHoveredStatement(Vector2 screenPosition)
    {
        var allStatements = DetectiveBoardManager.Instance.GetActiveStatementCards();

        foreach (var card in allStatements)
        {
            if (card != this && RectTransformUtility.RectangleContainsScreenPoint(
                card.GetComponent<RectTransform>(), screenPosition, null))
            {
                return card;
            }
        }

        return null;
    }
}