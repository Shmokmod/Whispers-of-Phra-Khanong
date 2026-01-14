// ===============================
// InteractWithHint.cs
// ระบบโต้ตอบ Hint สำหรับเปิดเบาะแส + ปลดล็อก Evidence
// ===============================

using UnityEngine;
using GameSystem;

public class InteractWithHint : MonoBehaviour, IInteractable
{
    // สถานะว่า Hint นี้ถูกเปิดแล้วหรือยัง
    public bool isOpened { get; private set; }

    [Header("Hint Settings")]
    [Tooltip("ใส่ ID เอง หรือเว้นว่างไว้ให้ระบบสร้างอัตโนมัติ")]
    public string hintID = "";

    [Header("Items / UI")]
    public GameObject itemPrefab;      // ของที่ได้จาก Hint (optional)
    public Sprite openSpriteHint;       // sprite ตอนเปิดแล้ว
    public GameObject gotItemUI;        // UI แจ้งว่าได้ของ

    [Header("Evidence to Unlock")]
    public string[] evidenceIDsToUnlock;

    // ===============================
    // Unity Lifecycle
    // ===============================
    private void Start()
    {
        // ถ้าไม่ได้กำหนด ID ให้ → สร้างอัตโนมัติ
        if (string.IsNullOrEmpty(hintID))
        {
            hintID = GoableHelper.GenerateUniqueID(gameObject);
            Debug.Log($"🔑 Auto-generated Hint ID: {hintID}");
        }
    }

    // ===============================
    // IInteractable
    // ===============================
    public bool CanInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenHint();
    }

    // ===============================
    // Core Logic
    // ===============================
    private void OpenHint()
    {
        SetOpened(true);
        UnlockEvidence();
        ShowGotItemUI();
    }

    /// <summary>
    /// ปลดล็อก Evidence ตาม ID ที่กำหนด
    /// </summary>
    private void UnlockEvidence()
    {
        if (evidenceIDsToUnlock == null || evidenceIDsToUnlock.Length == 0)
            return;

        foreach (string evidID in evidenceIDsToUnlock)
        {
            EvidenceData evid = GameDataRuntime.Instance.GetEvidence(evidID);
            if (evid == null)
            {
                Debug.LogWarning($"❌ Evidence ไม่พบ: {evidID}");
                continue;
            }

            evid.isUnlocked = true;
            GameDataRuntime.Instance.SaveUnlockedEvidence(evidID);
            Debug.Log($"✅ Evidence Unlocked: {evidID}");
        }

        // หลังได้ Evidence → ตรวจปลดล็อก Detective Note ต่อ
        DetectiveBookManager.Instance?.TryUnlockNotes();
    }

    /// <summary>
    /// แสดง UI ได้ของ + Pause เกม
    /// </summary>
    private void ShowGotItemUI()
    {
        if (!itemPrefab || !gotItemUI) return;

        gotItemUI.SetActive(true);
        PauseController.isPaused = true;
    }

    // ===============================
    // State Control
    // ===============================
    public void SetOpened(bool opened)
    {
        isOpened = opened;

        // ถ้าต้องการเปลี่ยน sprite ตอนเปิด
        // GetComponent<SpriteRenderer>().sprite = openSpriteHint;
    }

    public void CloseGotItemUI()
    {
        gotItemUI.SetActive(false);
        PauseController.isPaused = false;
    }
}
