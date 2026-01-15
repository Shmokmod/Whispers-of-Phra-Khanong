using GameSystem;
using UnityEngine;

public class InteractWithHint : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }

    [Header("Hint Settings")]
    [Tooltip("ใส่ ID เอง หรือเว้นว่างไว้ให้ระบบสร้างอัตโนมัติ")]
    public string hintID = "";

    [Header("Items")]
    public GameObject itemPrefab;
    public Sprite openSpriteHint;
    public GameObject GotItemUI;

    [Header("🔓 Direct Note Unlock (ไม่ต้องผ่าน Evidence)")]
    [Tooltip("Note ID ที่จะปลดล็อกเมื่อเปิด Hint นี้")]
    public string[] noteIDsToUnlock;

    [Header("Evidence to Unlock (Optional - ถ้ามีระบบ Evidence)")]
    public string[] evidenceIDsToUnlock;

    void Start()
    {
        if (string.IsNullOrEmpty(hintID))
        {
            hintID = GoableHelper.GenerateUniqueID(gameObject);
            Debug.Log($"🔑 Auto-generated Hint ID: {hintID}");
        }
    }

    public bool CanInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenHint();
    }

    private void OpenHint()
    {
        SetOpened(true);

        // ✅ ปลดล็อก Note โดยตรง (ไม่ต้องผ่าน Evidence)
        UnlockNotes();

        // ✅ ปลดล็อก Evidence (ถ้ามีระบบ Evidence)
        UnlockEvidence();

        if (itemPrefab)
        {
            print("Dropped hint item");
            GotItemUI.SetActive(true);
            PauseController.isPaused = true;
        }
    }

    // ✅ ฟังก์ชันใหม่: ปลดล็อก Note โดยตรง
    void UnlockNotes()
    {
        if (noteIDsToUnlock == null || noteIDsToUnlock.Length == 0)
        {
            // ไม่มี Note ให้ปลดล็อก = ไม่ต้องทำอะไร
            return;
        }

        if (DetectiveBookManager.Instance == null)
        {
            Debug.LogWarning("⚠️ DetectiveBookManager not found!");
            return;
        }

        foreach (string noteID in noteIDsToUnlock)
        {
            if (string.IsNullOrEmpty(noteID))
                continue;

            var note = DetectiveBookManager.Instance.GetNote(noteID);
            if (note != null)
            {
                DetectiveBookManager.Instance.UnlockNote(noteID);
                Debug.Log($"✅ Note Unlocked from Hint: {noteID}");
            }
            else
            {
                Debug.LogWarning($"❌ Note ไม่พบ: {noteID}");
            }
        }
    }

    // ✅ ฟังก์ชันเดิม: ปลดล็อก Evidence (ถ้ามี)
    void UnlockEvidence()
    {
        if (evidenceIDsToUnlock == null || evidenceIDsToUnlock.Length == 0)
        {
            // ไม่มี Evidence ให้ปลดล็อก = ข้าม
            return;
        }

        foreach (string evidID in evidenceIDsToUnlock)
        {
            if (string.IsNullOrEmpty(evidID))
                continue;

            EvidenceData evid = GameDataRuntime.Instance.GetEvidence(evidID);
            if (evid != null)
            {
                evid.isUnlocked = true;
                GameDataRuntime.Instance.SaveUnlockedEvidence(evidID);
                Debug.Log($"✅ Evidence Unlocked: {evidID}");
            }
            else
            {
                Debug.LogWarning($"❌ Evidence ไม่พบ: {evidID}");
            }
        }

        // ลองปลดล็อก Notes ที่รอ Evidence นี้
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.TryUnlockNotes();
        }
    }

    public void SetOpened(bool opened)
    {
        isOpened = opened;
        if (isOpened == opened)
        {
            //GetComponent<SpriteRenderer>().sprite = openSpriteHint;
        }
    }

    public void CloseGotitemUI()
    {
        GotItemUI.SetActive(false);
        PauseController.isPaused = false;
    }
}