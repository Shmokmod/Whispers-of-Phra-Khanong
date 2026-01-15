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
    private HashSet<string> reachedDialogueKeys = new HashSet<string>(); // "dialogueID_index"

    [Header("Events")]
    public System.Action<DetectiveNote> OnNoteUnlocked;

    [Header("Notification UI")]
    public Canvas NotificationCanvas;
    public Image NoteIconImage;
    public float notifyDuration = 2f;
    public float blinkSpeed = 0.3f;
    public float fadeTime = 0.3f;
    public float showTime = 1.5f;

    CanvasGroup canvasGroup;


    void Awake()
    {
        canvasGroup = NotificationCanvas.GetComponent<CanvasGroup>();
        OnNoteUnlocked += ShowNoteNotification;


        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        LoadUnlockedNotes();
    }

    // ==================== Unlock Logic ====================

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
            return true; // ไม่มีเงื่อนไข = ปลดล็อกตั้งแต่แรก

        foreach (var condition in note.unlockConditions)
        {
            if (!CheckCondition(condition))
                return false; // เงื่อนไขใดไม่เป็นจริง = ล้มเหลว
        }

        return true; // ทุกเงื่อนไขเป็นจริง

        //NewNoteUnlockNotificationUI
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
        }

        // ลอง unlock notes อื่นที่รอ note นี้
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
            reachedDialogueKeys = reachedDialogueKeys.ToList()
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("DetectiveBook_UnlockedNotes", json);
        PlayerPrefs.Save();
    }

    void LoadUnlockedNotes()
    {
        if (!PlayerPrefs.HasKey("DetectiveBook_UnlockedNotes"))
        {
            Debug.Log("📘 No saved Detective Notes found. Starting fresh.");
            return;
        }

        string json = PlayerPrefs.GetString("DetectiveBook_UnlockedNotes");
        var data = JsonUtility.FromJson<SaveData>(json);

        unlockedNoteIDs = new HashSet<string>(data.unlockedNoteIDs);
        reachedDialogueKeys = new HashSet<string>(data.reachedDialogueKeys);

        Debug.Log($"📘 Loaded {unlockedNoteIDs.Count} unlocked notes.");
    }

    public void ClearAllProgress()
    {
        unlockedNoteIDs.Clear();
        reachedDialogueKeys.Clear();
        PlayerPrefs.DeleteKey("DetectiveBook_UnlockedNotes");
        Debug.Log("🗑️ Detective Book progress cleared.");
    }

    [System.Serializable]
    class SaveData
    {
        public List<string> unlockedNoteIDs;
        public List<string> reachedDialogueKeys;
    }
}