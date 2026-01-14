// ===============================
// DetectiveNote.cs
// ScriptableObject สำหรับเก็บข้อมูล Note ในสมุดนักสืบ
// ===============================

using UnityEngine;

[CreateAssetMenu(
    fileName = "New Detective Note",
    menuName = "Detective/Note"
)]
public class DetectiveNote : ScriptableObject
{
    [Header("Note Info")]
    [Tooltip("ID ต้องไม่ซ้ำ เช่น note_suspect_01")]
    public string noteID;

    [Tooltip("ชื่อหัวข้อที่แสดงในสมุดนักสืบ")]
    public string title = "หัวข้อเบาะแส";

    [TextArea(5, 10)]
    [Tooltip("เนื้อหาเบาะแส")]
    public string content = "รายละเอียดเบาะแส...";

    [Header("Display Order")]
    [Tooltip("ลำดับการแสดง (เรียงจากน้อย → มาก)")]
    public int orderIndex = 0;

    [Header("Visual")]
    [Tooltip("รูปภาพประกอบ (ไม่ใส่ก็ได้)")]
    public Sprite noteImage;

    [Header("Unlock Conditions")]
    [Tooltip("ทุกเงื่อนไขต้องเป็นจริง (AND logic)")]
    public UnlockCondition[] unlockConditions;
}

// ===============================
// UnlockCondition.cs
// เงื่อนไขสำหรับปลดล็อก Detective Note
// ===============================

[System.Serializable]
public class UnlockCondition
{
    public enum ConditionType
    {
        DialogueReached,   // ถึง dialogue index ที่กำหนด
        EvidenceUnlocked,  // Evidence ถูกปลดล็อก
        StatementUnlocked, // Statement ถูกปลดล็อก
        NoteUnlocked       // Note อื่นถูกปลดล็อกแล้ว
    }

    [Header("Condition Settings")]
    public ConditionType type;

    [Tooltip("ID ตามประเภท (dialogueID / evidenceID / statementID / noteID)")]
    public string targetID;

    [Tooltip("ใช้เฉพาะ DialogueReached (index ของ dialogue)")]
    public int dialogueIndex = -1;
}
