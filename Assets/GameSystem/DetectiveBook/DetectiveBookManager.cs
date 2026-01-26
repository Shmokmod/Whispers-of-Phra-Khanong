using GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DetectiveBookManager : MonoBehaviour
{
    public static DetectiveBookManager Instance { get; private set; }

    [Header("All Detective Notes")]
    public DetectiveNote[] allNotes;

    [Header("Runtime Data")]
    private HashSet<string> unlockedNoteIDs = new HashSet<string>();
    public HashSet<string> reachedDialogueKeys = new HashSet<string>(); // "dialogueID_index"
    private HashSet<string> interactedHints = new HashSet<string>();

    [Header("Events")]
    public System.Action<DetectiveNote> OnNoteUnlocked;
    public System.Action<string, int> OnDialogueReached;

    [Header("Notification UI")]
    public Canvas NotificationCanvas;
    public Image NoteIconImage;
    public float notifyDuration = 2f;
    public float blinkSpeed = 0.3f;
    public float fadeTime = 0.3f;
    public float showTime = 1.5f;

    CanvasGroup canvasGroup;

    [Header("TutorialUI")]
    public GameObject DetectiveBooktutorialUI;
    bool tutorialShownThisSession = false;


    // 🆕 Static variable สำหรับรับข้อมูลจาก SaveManager
    public static HashSet<string> pendingDialogueKeys = null;

    void Awake()
    {
        canvasGroup = NotificationCanvas.GetComponent<CanvasGroup>();
        OnNoteUnlocked += ShowNoteNotification;

        if (Instance == null)
        {
            Instance = this;
            
            // 🆕 โหลด dialogue keys จาก SaveManager
            Debug.Log($"[DetectiveBook] pendingDialogueKeys = {(pendingDialogueKeys == null ? "NULL" : pendingDialogueKeys.Count.ToString())}");
            
            if (pendingDialogueKeys != null)
            {
                reachedDialogueKeys = new HashSet<string>(pendingDialogueKeys);
                Debug.Log($"[DetectiveBook] ✅ Loaded {reachedDialogueKeys.Count} dialogue keys from SaveManager:");
                foreach (var key in reachedDialogueKeys)
                {
                    Debug.Log($"[DetectiveBook]   - {key}");
                }
                pendingDialogueKeys = null; // Clear หลังใช้
            }
            else
            {
                reachedDialogueKeys = new HashSet<string>();
                Debug.Log("[DetectiveBook] ⚠️ No pending keys from SaveManager, started fresh");
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // ==================== Unlock Logic ====================
    public void InitAfterSlotSelected()
    {
        unlockedNoteIDs.Clear();
        reachedDialogueKeys.Clear();
        interactedHints.Clear();
        LoadUnlockedNotes();
    }



    void ShowNoteNotification(DetectiveNote note)
    {
        StopAllCoroutines();
        NotificationCanvas.gameObject.SetActive(true);
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // Fade In
        yield return Fade(0f, 1f);

        yield return new WaitForSeconds(showTime);

        // Fade Out
        yield return Fade(1f, 0f);

        NotificationCanvas.gameObject.SetActive(false);
    }

    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void OpenDetectiveBook()
    {
        if (!PlayerPrefs.HasKey("Tutorial_Note"))
        {
            DetectiveBooktutorialUI.SetActive(true);
            PlayerPrefs.SetInt("Tutorial_Note", 1);
        }

        // เปิด UI Book ปกติ
    }

     
    public bool HasReachedDialogue(string dialogueID, int index)
    {
        string key = $"{dialogueID}_{index}";
        return reachedDialogueKeys.Contains(key);
    }

    IEnumerator BlinkIcon()
    {
        float timer = 0f;
        bool visible = true;

        while (timer < notifyDuration)
        {
            visible = !visible;
            NoteIconImage.enabled = visible;
            yield return new WaitForSeconds(blinkSpeed);
            timer += blinkSpeed;
        }

        NoteIconImage.enabled = true;
        NotificationCanvas.gameObject.SetActive(false);
    }

    public void TryUnlockNotes()
    {
        if (allNotes == null || allNotes.Length == 0)
        {
            Debug.LogWarning("⚠️ All Notes is empty! ไม่มี Note ใน Manager");
            return;
        }

        Debug.Log($"🔍 Checking {allNotes.Length} notes...");

        foreach (var note in allNotes)
        {
            if (note == null)
            {
                Debug.LogWarning("⚠️ Note is NULL in array!");
                continue;
            }

            if (IsNoteUnlocked(note.noteID))
            {
                Debug.Log($"✅ Note '{note.noteID}' already unlocked, skip.");
                continue;
            }

            Debug.Log($"🔎 Checking note: {note.noteID}");

            if (CheckAllConditions(note))
            {
                UnlockNote(note.noteID);
            }
            else
            {
                Debug.Log($"❌ Note '{note.noteID}' conditions NOT met.");
            }
        }
    }

    bool CheckAllConditions(DetectiveNote note)
    {
        if (note.unlockConditions == null || note.unlockConditions.Length == 0)
            return true;

        foreach (var condition in note.unlockConditions)
        {
            if (!CheckCondition(condition))
                return false;
        }

        return true;
    }

    bool CheckCondition(UnlockCondition condition)
    {
        switch (condition.type)
        {
            case UnlockCondition.ConditionType.DialogueReached:
                string key = $"{condition.targetID}_{condition.dialogueIndex}";
                bool hasKey = reachedDialogueKeys.Contains(key);
                Debug.Log($"  📋 Check DialogueReached: '{key}' → {(hasKey ? "✅ PASS" : "❌ FAIL")}");
                return hasKey;

            case UnlockCondition.ConditionType.EvidenceUnlocked:
                var evidence = GameDataRuntime.Instance.GetEvidence(condition.targetID);
                bool evidUnlocked = evidence != null && evidence.isUnlocked;
                Debug.Log($"  📋 Check Evidence: '{condition.targetID}' → {(evidUnlocked ? "✅ PASS" : "❌ FAIL")}");
                return evidUnlocked;

            case UnlockCondition.ConditionType.StatementUnlocked:
                var statement = GameDataRuntime.Instance.GetStatement(condition.targetID);
                bool stmtUnlocked = statement != null && statement.isUnlocked;
                Debug.Log($"  📋 Check Statement: '{condition.targetID}' → {(stmtUnlocked ? "✅ PASS" : "❌ FAIL")}");
                return stmtUnlocked;

            case UnlockCondition.ConditionType.NoteUnlocked:
                bool noteUnlocked = IsNoteUnlocked(condition.targetID);
                Debug.Log($"  📋 Check Note: '{condition.targetID}' → {(noteUnlocked ? "✅ PASS" : "❌ FAIL")}");
                return noteUnlocked;

            case UnlockCondition.ConditionType.HintInteracted:
                bool interacted = interactedHints.Contains(condition.targetID);
                Debug.Log($"  📋 Check HintInteracted: '{condition.targetID}' → {(interacted ? "✅ PASS" : "❌ FAIL")}");
                return interacted;

            default:
                Debug.LogWarning($"  ⚠️ Unknown condition type: {condition.type}");
                return false;
        }
    }

    public void UnlockNote(string noteID)
    {
        if (IsNoteUnlocked(noteID)) return;

        unlockedNoteIDs.Add(noteID);
        SaveUnlockedNotes();

        var note = GetNote(noteID);
        if (note != null)
        {
            Debug.Log($"📝 Detective Note Unlocked: {note.title}");
            OnNoteUnlocked?.Invoke(note);
            ShowNoteTutorialOnce();
            Debug.Log($"📘 Total Unlocked Notes: {unlockedNoteIDs.Count}");
        }

        TryUnlockNotes();
    }

    public bool IsNoteUnlocked(string noteID)
    {
        return unlockedNoteIDs.Contains(noteID);
    }

    public DetectiveNote GetNote(string noteID)
    {
        return allNotes.FirstOrDefault(n => n != null && n.noteID == noteID);
    }

    // ==================== Dialogue Tracking ====================

    public void MarkDialogueReached(string dialogueID, int index)
    {
        string key = $"{dialogueID}_{index}";
        if (!reachedDialogueKeys.Contains(key))
        {
            reachedDialogueKeys.Add(key);
            Debug.Log($"📖 Dialogue Reached: {key}");

            OnDialogueReached?.Invoke(dialogueID, index);

            // อัพเดท SceneTrigger ทั้งหมด
            SceneTrigger[] triggers = Object.FindObjectsByType<SceneTrigger>(FindObjectsSortMode.None);
            foreach (var t in triggers)
            {
                t.RefreshUnlockStatus();
            }

            TryUnlockNotes();
        }
    }

    // ==================== Get Unlocked Notes (Sorted) ====================

    public List<DetectiveNote> GetUnlockedNotesSorted()
    {
        return allNotes
            .Where(n => n != null && IsNoteUnlocked(n.noteID))
            .OrderBy(n => n.orderIndex)
            .ToList();
    }

    // ==================== Save / Load ====================

    void SaveUnlockedNotes()
    {
        var data = new SaveData
        {
            unlockedNoteIDs = unlockedNoteIDs.ToList(),
            interactedHints = interactedHints.ToList()
            // ✅ ไม่บันทึก dialogue keys ที่นี่ (SaveManager ดูแลให้)
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(BookSaveKey(), json);
        PlayerPrefs.Save();
        
        Debug.Log($"[DetectiveBook] Saved {unlockedNoteIDs.Count} notes (dialogue keys handled by SaveManager)");
    }

    void LoadUnlockedNotes()
    {
        string key = BookSaveKey();

        if (!PlayerPrefs.HasKey(key))
        {
            Debug.Log("📘 No saved Detective Notes for this slot.");
            return;
        }

        string json = PlayerPrefs.GetString(key);
        var data = JsonUtility.FromJson<SaveData>(json);

        unlockedNoteIDs = new HashSet<string>(data.unlockedNoteIDs);
        interactedHints = new HashSet<string>(data.interactedHints);
        // ✅ ไม่โหลด dialogue keys ที่นี่ (รับจาก SaveManager ใน Awake แล้ว)
        
        Debug.Log($"📘 Loaded {unlockedNoteIDs.Count} unlocked notes");
        Debug.Log($"📖 Current dialogue keys: {reachedDialogueKeys.Count}");
    }

    public void ClearAllProgress()
    {
        unlockedNoteIDs.Clear();
        reachedDialogueKeys.Clear();
        PlayerPrefs.DeleteKey("DetectiveBook_UnlockedNotes");
        Debug.Log("🗑️ Detective Book progress cleared.");
    }

    public void MarkHintInteracted(string hintID)
    {
        if (interactedHints.Add(hintID))
        {
            Debug.Log($"🕵️ Hint Interacted: {hintID}");
            TryUnlockNotes();
        }
    }

    //==================== Tutorial ====================
    void ShowNoteTutorialOnce()
    {
        if (tutorialShownThisSession) return;

        if (!PlayerPrefs.HasKey("Tutorial_Note"))
        {
            DetectiveBooktutorialUI.SetActive(true);
            PlayerPrefs.SetInt("Tutorial_Note", 1);
            tutorialShownThisSession = true;
        }
    }

    public void CloseTutorialUI()
    {
        DetectiveBooktutorialUI.SetActive(false);
        Debug.Log("Detective Book Tutorial UI closed.");
    }

    // ==================== Save Data Structure ====================

    string BookSaveKey()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", -1);
        return $"DetectiveBook_UnlockedNotes_{slot}";
    }

    [System.Serializable]
    class SaveData
    {
        public List<string> unlockedNoteIDs;
        public List<string> interactedHints;
        // ✅ ลบ reachedDialogueKeys ออก (ย้ายไปให้ SaveManager ดูแล)
    }
}