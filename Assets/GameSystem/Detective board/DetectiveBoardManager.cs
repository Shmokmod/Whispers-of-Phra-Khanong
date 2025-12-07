using DG.Tweening;
using GameSystem;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DetectiveBoardManager : MonoBehaviour
{
    public bool isnowPaused = true;
    public static DetectiveBoardManager Instance;

    [Header("UI Panels")]
    public GameObject boardUI;
    public Transform statementContainer;
    public Transform evidenceContainer;
    public ResultPopup resultPopup;
    public Button closeButton;

    [Header("Header")]
    public TextMeshProUGUI phaseTitleText;
    public TextMeshProUGUI objectiveText;

    [Header("Progress")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    [Header("Prefabs")]
    public GameObject statementCardPrefab;
    public GameObject evidenceCardPrefab;

    [Header("Card Layout Settings")]
    [Tooltip("ตำแหน่งเริ่มต้นของ Statement Card จากด้านบน")]
    public float statementStartY = -80f;
    [Tooltip("ระยะห่างระหว่าง Statement Cards")]
    public float statementYOffset = -100f;
    [Tooltip("ตำแหน่งเริ่มต้นของ Evidence Card")]
    public Vector2 evidenceStartPos = new Vector2(-90f, -100f);
    [Tooltip("ระยะห่างระหว่าง Evidence Cards")]
    public Vector2 evidenceOffset = new Vector2(200f, -240f);
    [Tooltip("จำนวนคอลัมน์ของ Evidence")]
    public int evidenceColumns = 2;

    [Header("Current State")]
    public int currentPhase = 1;
    public int correctConnectionsThisPhase = 0;
    public int requiredConnectionsThisPhase = 2;

    private StatementCard currentDraggingCard;
    private EvidenceCard currentHoveredEvidence;
    public List<string> completedConnections = new List<string>();
    private List<StatementCard> activeStatementCards = new List<StatementCard>();
    private List<EvidenceCard> activeEvidenceCards = new List<EvidenceCard>();

    public int wrongAttempts = 0;
    public int maxAttemptsBeforeHint = 3;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseUI);
        else
            Debug.LogError("CloseButton ไม่ได้ assign ใน Inspector!");

        boardUI.SetActive(false);
        isnowPaused = false;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && Time.timeSinceLevelLoad > 0.1f)
        {
            if (isnowPaused)
            {
                CloseUI();
                ResumeGame();
            }
            else
            {
                PauseGame();
                OpenUI();
            }
        }
    }

    public void PauseGame()
    {
        boardUI.SetActive(true);
        Time.timeScale = 0f;
        isnowPaused = true;
    }

    public void ResumeGame()
    {
        boardUI.SetActive(false);
        Time.timeScale = 1f;
        isnowPaused = false;
    }

    public void OpenUI()
    {
        boardUI.SetActive(true);
        CanvasGroup cg = boardUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = boardUI.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.DOFade(1f, 0.3f).SetUpdate(true);

        AudioManager.Instance?.PlayBGM("detective_theme");
        isnowPaused = true;

        LoadPhase(currentPhase);
    }

    public void CloseUI()
    {
        CanvasGroup cg = boardUI.GetComponent<CanvasGroup>();
        cg.DOFade(0f, 0.3f).SetUpdate(true).OnComplete(() =>
        {
            boardUI.SetActive(false);
            isnowPaused = false;
            AudioManager.Instance?.ResumePreviousBGM();
        });
    }

    void LoadPhase(int phase)
    {
        ClearBoard();
        correctConnectionsThisPhase = 0;
        wrongAttempts = 0;
        completedConnections.Clear();

        switch (phase)
        {
            case 1:
                LoadPhase1_FindTheLiar();
                break;
            case 2:
                break;
            case 3:
                break;
        }

        UpdateProgressUI();
    }

    void ClearBoard()
    {
        foreach (var card in activeStatementCards) if (card != null) Destroy(card.gameObject);
        foreach (var card in activeEvidenceCards) if (card != null) Destroy(card.gameObject);

        activeStatementCards.Clear();
        activeEvidenceCards.Clear();
    }

    void LoadPhase1_FindTheLiar()
    {
        phaseTitleText.text = "Phase 1: Find the Liar";
        objectiveText.text = "Find Evidence";
        requiredConnectionsThisPhase = 2;

        CreateStatementIfUnlocked("0001");
        CreateStatementIfUnlocked("0002");
        CreateStatementIfUnlocked("0003");

        CreateEvidenceIfUnlocked("0001");
        CreateEvidenceIfUnlocked("0002");
        CreateEvidenceIfUnlocked("0003");
        CreateEvidenceIfUnlocked("0004");
    }

    void CreateStatementIfUnlocked(string id)
    {
        StatementData data = GameDataRuntime.Instance?.GetStatement(id);
        if (data == null)
        {
            Debug.LogWarning($"Statement not found: {id}");
            return;
        }

        if (!data.isUnlocked)
        {
            Debug.Log($"⏸️ Statement ยังไม่ unlock: {id}");
            return;
        }

        Debug.Log($"🔓 Statement unlock แล้ว: {id} → สร้าง card");
        CreateStatement(id);
    }

    void CreateStatement(string id)
    {
        if (GameDataRuntime.Instance == null || GameDataRuntime.Instance.data == null)
        {
            Debug.LogError("GameDataRuntime or GameData is null!");
            return;
        }

        StatementData data = GameDataRuntime.Instance.GetStatement(id);
        if (data == null)
        {
            Debug.LogWarning($"Statement not found: {id}");
            return;
        }

        if (statementCardPrefab == null)
        {
            Debug.LogError("Statement Card Prefab is not assigned!");
            return;
        }

        if (statementContainer == null)
        {
            Debug.LogError("Statement Container is not assigned!");
            return;
        }

        // ✅ Debug Container Info
        RectTransform containerRect = statementContainer.GetComponent<RectTransform>();
        Debug.Log("=================================================");
        Debug.Log($"📦 STATEMENT CONTAINER INFO:");
        Debug.Log($"   Name: {statementContainer.name}");
        Debug.Log($"   Anchors: Min=({containerRect.anchorMin.x:F2}, {containerRect.anchorMin.y:F2}), Max=({containerRect.anchorMax.x:F2}, {containerRect.anchorMax.y:F2})");
        Debug.Log($"   Size: {containerRect.rect.width:F0} x {containerRect.rect.height:F0}");
        Debug.Log($"   Position: ({containerRect.anchoredPosition.x:F0}, {containerRect.anchoredPosition.y:F0})");

        GameObject obj = Instantiate(statementCardPrefab, statementContainer);
        StatementCard card = obj.GetComponent<StatementCard>();
        if (card == null)
        {
            Debug.LogError("Statement Card script not found on prefab!");
            Destroy(obj);
            return;
        }

        // ✅ ใช้ Center-Center Anchor แทน Top-Center
        RectTransform cardRect = obj.GetComponent<RectTransform>();

        // ตั้งเป็น Center-Center
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        // คำนวณตำแหน่งจากกลาง Container
        int cardIndex = activeStatementCards.Count;

        // วางการ์ดเรียงจากบนลงล่าง (ใช้ค่าลบ)
        float spacing = 140f; // ระยะห่างระหว่างการ์ด
        float startY = -100f; // เริ่มใต้กลางนิดหน่อย

        float yPos = startY - (spacing * cardIndex);

        cardRect.anchoredPosition = new Vector2(0, yPos);
        cardRect.localScale = Vector3.one;
        cardRect.localRotation = Quaternion.identity;

        // ✅ Debug Card Position
        Debug.Log($"🃏 CARD CREATED ({id}):");
        Debug.Log($"   Index: {cardIndex}");
        Debug.Log($"   Start Y: {startY:F0}");
        Debug.Log($"   Final Y: {yPos:F0}");
        Debug.Log($"   Anchored Position: ({cardRect.anchoredPosition.x:F0}, {cardRect.anchoredPosition.y:F0})");
        Debug.Log("=================================================\n");

        card.Initialize(data);
        activeStatementCards.Add(card);
    }

    void CreateEvidenceIfUnlocked(string id)
    {
        EvidenceData data = GameDataRuntime.Instance?.GetEvidence(id);
        if (data == null)
        {
            Debug.LogWarning($"Evidence not found: {id}");
            return;
        }

        if (!data.isUnlocked)
        {
            Debug.Log($"⏸️ Evidence ยังไม่ unlock: {id}");
            return;
        }

        Debug.Log($"🔓 Evidence unlock แล้ว: {id} → สร้าง card");
        CreateEvidence(id);
    }

    void CreateEvidence(string id)
    {
        if (GameDataRuntime.Instance == null || GameDataRuntime.Instance.data == null)
        {
            Debug.LogError("GameDataRuntime or GameData is null!");
            return;
        }

        EvidenceData data = GameDataRuntime.Instance.GetEvidence(id);
        if (data == null)
        {
            Debug.LogWarning($"Evidence not found: {id}");
            return;
        }

        if (evidenceCardPrefab == null)
        {
            Debug.LogError("Evidence Card Prefab is not assigned!");
            return;
        }

        if (evidenceContainer == null)
        {
            Debug.LogError("Evidence Container is not assigned!");
            return;
        }

        GameObject obj = Instantiate(evidenceCardPrefab, evidenceContainer);
        EvidenceCard card = obj.GetComponent<EvidenceCard>();
        if (card == null)
        {
            Debug.LogError("Evidence Card script not found on prefab!");
            Destroy(obj);
            return;
        }

        // ✅ ใช้ Center-Center Anchor เหมือนกัน
        RectTransform cardRect = obj.GetComponent<RectTransform>();
        RectTransform containerRect = evidenceContainer.GetComponent<RectTransform>();

        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);

        // คำนวณตำแหน่งแบบ Grid
        int cardIndex = activeEvidenceCards.Count;
        int col = cardIndex % evidenceColumns;
        int row = cardIndex / evidenceColumns;

        float containerHeight = containerRect.rect.height;

        // ระยะห่างระหว่างการ์ด
        float spacingX = 220f;
        float spacingY = 270f;

        // คำนวณตำแหน่งเริ่มต้นให้ Grid อยู่กลาง
        float totalWidth = (evidenceColumns * spacingX) - 20;
        float startX = -(totalWidth / 2) + (spacingX / 2);
        float startY = containerHeight * 0.2f; // เริ่มจากบนลงมา

        float xPos = startX + (spacingX * col);
        float yPos = startY - (spacingY * row);

        cardRect.anchoredPosition = new Vector2(xPos, yPos);
        cardRect.localScale = Vector3.one;

        Debug.Log($"✅ Evidence card created: {id}");
        Debug.Log($"   Position: ({xPos:F0}, {yPos:F0})");
        Debug.Log($"   Grid: Col {col}, Row {row}");

        card.Initialize(data);
        activeEvidenceCards.Add(card);
    }

    public void CheckConnection(string statementID, string evidenceID)
    {
        string key = $"{statementID}_{evidenceID}";
        if (completedConnections.Contains(key))
        {
            ShowResult("คุณเชื่อมโยงคู่นี้ไปแล้ว", ResultType.Duplicate);
            return;
        }

        if (currentPhase == 1)
        {
            if (statementID == "0001" && evidenceID == "0001")
            {
                OnCorrectConnection(statementID, evidenceID, "✅ ถูกต้อง", "คำให้การตรงกับหลักฐาน", true);
                return;
            }
            if (statementID == "0002" && evidenceID == "0002")
            {
                OnCorrectConnection(statementID, evidenceID, "✅ ถูกต้อง", "คำให้การตรงกับหลักฐาน", false);
                return;
            }
            if (statementID == "0003" && evidenceID == "0003")
            {
                OnCorrectConnection(statementID, evidenceID, "✅ ถูกต้อง", "คำให้การตรงกับหลักฐาน", true);
                return;
            }
        }

        OnWrongConnection();
    }

    void OnCorrectConnection(string stmtID, string evidID, string title, string message, bool isContradiction)
    {
        string key = $"{stmtID}_{evidID}";
        completedConnections.Add(key);
        correctConnectionsThisPhase++;
        wrongAttempts = 0;

        StatementCard stmt = FindStatementCard(stmtID);
        if (stmt != null)
        {
            if (isContradiction) stmt.SetContradicted();
            else stmt.SetVerified();
        }

        EvidenceCard evid = FindEvidenceCard(evidID);
        if (evid != null) evid.SetUsed();

        ShowResult(title, message, ResultType.Correct);
        UpdateProgressUI();

        if (correctConnectionsThisPhase >= requiredConnectionsThisPhase)
            Invoke(nameof(OnPhaseComplete), 2f);

        StoryManager.Instance?.OnCorrectConnection(key);
    }

    void OnWrongConnection()
    {
        wrongAttempts++;
        ShowResult("❌ ไม่ถูกต้อง", "หลักฐานชิ้นนี้ไม่เกี่ยวข้องกับคำให้การ", ResultType.Wrong);

        if (wrongAttempts >= maxAttemptsBeforeHint)
        {
            Invoke(nameof(ShowHint), 1.5f);
            wrongAttempts = 0;
        }
    }

    StatementCard FindStatementCard(string id) => activeStatementCards.Find(c => c.data.id == id);
    EvidenceCard FindEvidenceCard(string id) => activeEvidenceCards.Find(c => c.data.id == id);

    // ✅ เพิ่มฟังก์ชันที่ Script อื่นเรียกใช้
    public List<StatementCard> GetActiveStatementCards() => activeStatementCards;

    public bool CheckContradiction(string stmtID, string evidID)
    {
        // ตรวจสอบว่าคู่นี้ขัดแย้งกันหรือไม่
        if (currentPhase == 1)
        {
            if (stmtID == "0001" && evidID == "0001") return true;
            if (stmtID == "0003" && evidID == "0003") return true;
        }
        return false;
    }

    public bool IsValidConnection(string stmtID, string evidID)
    {
        if (currentPhase == 1)
        {
            if (stmtID == "0001" && evidID == "0001") return true;
            if (stmtID == "0002" && evidID == "0002") return true;
            if (stmtID == "0003" && evidID == "0003") return true;
        }
        return false;
    }

    public void OnCardStartDrag(StatementCard card)
    {
        currentDraggingCard = card;

        foreach (var evidCard in activeEvidenceCards)
        {
            if (IsValidConnection(card.data.id, evidCard.data.id))
                evidCard.glowEffect.SetActive(true);
        }
    }

    public void OnCardEndDrag()
    {
        currentDraggingCard = null;
        currentHoveredEvidence = null;

        foreach (var evidCard in activeEvidenceCards)
            evidCard.glowEffect.SetActive(false);
    }

    public void CheckHoverOverEvidence(Vector2 screenPosition)
    {
        currentHoveredEvidence = null;

        foreach (var evidCard in activeEvidenceCards)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                evidCard.GetComponent<RectTransform>(), screenPosition, null))
            {
                currentHoveredEvidence = evidCard;
                break;
            }
        }
    }

    public EvidenceCard GetHoveredEvidence() => currentHoveredEvidence;

    void UpdateProgressUI()
    {
        progressBar.value = (float)correctConnectionsThisPhase / requiredConnectionsThisPhase;
        progressText.text = $"{correctConnectionsThisPhase}/{requiredConnectionsThisPhase} เบาะแสพบแล้ว";
    }

    void ShowResult(string title, string message, ResultType type) => resultPopup.Show(title, message, type);
    void ShowResult(string message, ResultType type) => resultPopup.Show("", message, type);

    void ShowHint()
    {
        string hint = "ลองอ่านคำให้การและหลักฐานอย่างละเอียด มองหาความขัดแย้ง";

        if (currentPhase == 1)
        {
            if (!completedConnections.Contains("0001_0001"))
                hint = "💡 ลองเชื่อมคำให้การแรกกับหลักฐานแรก";
            else if (!completedConnections.Contains("0002_0002"))
                hint = "💡 ลองเชื่อมคำให้การที่สองกับหลักฐานที่สอง";
        }

        resultPopup.Show("💡 คำใบ้", hint, ResultType.Hint);
    }

    void OnPhaseComplete()
    {
        resultPopup.Show("✅ Phase เสร็จสมบูรณ์!", $"คุณค้นพบเบาะแสครบทั้งหมดแล้ว!\n\nกำลังเตรียม Phase ถัดไป...", ResultType.PhaseComplete);
        StoryManager.Instance?.OnDetectiveBoardPhaseComplete(currentPhase);
        Invoke(nameof(CloseUI), 3f);
    }

    public void GoToNextPhase()
    {
        currentPhase++;
        OpenUI();
    }

    public void ResetPhase()
    {
        LoadPhase(currentPhase);
    }

    // ✅ ฟังก์ชันช่วยดีบัก - เรียกจาก Inspector
    [ContextMenu("Refresh Card Positions")]
    public void RefreshCardPositions()
    {
        // อัปเดตตำแหน่ง Statement Cards
        for (int i = 0; i < activeStatementCards.Count; i++)
        {
            if (activeStatementCards[i] != null)
            {
                RectTransform cardRect = activeStatementCards[i].GetComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0.5f, 1f);
                cardRect.anchorMax = new Vector2(0.5f, 1f);
                cardRect.pivot = new Vector2(0.5f, 1f);

                float yPos = statementStartY + (statementYOffset * i);
                cardRect.anchoredPosition = new Vector2(0, yPos);

                Debug.Log($"Updated Statement Card {i} to Y={yPos}");
            }
        }

        // อัปเดตตำแหน่ง Evidence Cards
        for (int i = 0; i < activeEvidenceCards.Count; i++)
        {
            if (activeEvidenceCards[i] != null)
            {
                RectTransform cardRect = activeEvidenceCards[i].GetComponent<RectTransform>();
                cardRect.anchorMin = new Vector2(0.5f, 1f);
                cardRect.anchorMax = new Vector2(0.5f, 1f);
                cardRect.pivot = new Vector2(0.5f, 1f);

                int col = i % evidenceColumns;
                int row = i / evidenceColumns;

                float xPos = evidenceStartPos.x + (evidenceOffset.x * col);
                float yPos = evidenceStartPos.y + (evidenceOffset.y * row);

                cardRect.anchoredPosition = new Vector2(xPos, yPos);

                Debug.Log($"Updated Evidence Card {i} to ({xPos}, {yPos})");
            }
        }

        Debug.Log("✅ Card positions refreshed!");
    }

    // ✅ ฟังก์ชันช่วยดีบัก - แสดงตำแหน่งการ์ดปัจจุบัน
    [ContextMenu("Debug - Show Current Positions")]
    public void DebugShowPositions()
    {
        Debug.Log("=== CURRENT POSITIONS ===");
        Debug.Log($"Layout Settings:");
        Debug.Log($"  Statement Start Y: {statementStartY}");
        Debug.Log($"  Statement Y Offset: {statementYOffset}");
        Debug.Log($"  Evidence Start: {evidenceStartPos}");
        Debug.Log($"  Evidence Offset: {evidenceOffset}");
        Debug.Log($"  Evidence Columns: {evidenceColumns}");

        Debug.Log($"\nStatement Cards ({activeStatementCards.Count}):");
        for (int i = 0; i < activeStatementCards.Count; i++)
        {
            if (activeStatementCards[i] != null)
            {
                RectTransform rt = activeStatementCards[i].GetComponent<RectTransform>();
                Debug.Log($"  [{i}] {activeStatementCards[i].data.id}: {rt.anchoredPosition}");
            }
        }

        Debug.Log($"\nEvidence Cards ({activeEvidenceCards.Count}):");
        for (int i = 0; i < activeEvidenceCards.Count; i++)
        {
            if (activeEvidenceCards[i] != null)
            {
                RectTransform rt = activeEvidenceCards[i].GetComponent<RectTransform>();
                Debug.Log($"  [{i}] {activeEvidenceCards[i].data.id}: {rt.anchoredPosition}");
            }
        }
    }
}   