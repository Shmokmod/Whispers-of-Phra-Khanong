using UnityEngine;

[CreateAssetMenu(fileName = "New Detective Note", menuName = "Detective/Note")]
public class DetectiveNote : ScriptableObject
{
    [Header("Note Info")]
    public string noteID; // ต้องไม่ซ้ำกัน เช่น "note_suspect_01"
    public string title = "หัวข้อเบาะแส";

    [TextArea(5, 10)]
    public string content = "รายละเอียดเบาะแส...";

    [Header("Display Order")]
    [Tooltip("ลำดับการแสดง (เรียงจากน้อยไปมาก)")]
    public int orderIndex = 0;

    [Header("Visual")]
    [Tooltip("รูปภาพประกอบ (Optional)")]
    public Sprite noteImage;

    [Header("Unlock Conditions")]
    [Tooltip("ทุกเงื่อนไขต้องเป็นจริง (AND logic) - ถ้าไม่ใส่เงื่อนไข = ปลดล็อกด้วย action trigger")]
    public UnlockCondition[] unlockConditions;
}

[System.Serializable]
public class UnlockCondition
{
    public enum ConditionType
    {
        DialogueReached,
        EvidenceUnlocked,
        StatementUnlocked,
        NoteUnlocked,
        HintInteracted   // ✅ เพิ่ม
    }


    [Header("Condition Settings")]
    public ConditionType type;

    [Tooltip("ใส่ ID ตามประเภท: dialogueID, evidenceID, statementID, noteID")]
    public string targetID;

    [Tooltip("(เฉพาะ DialogueReached) ระบุ index บรรทัด")]
    public int dialogueIndex = -1;
}