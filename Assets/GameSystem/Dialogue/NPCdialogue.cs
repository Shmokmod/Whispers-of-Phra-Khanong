using UnityEngine;

// ✅ ย้าย enum มาไว้ด้านนอก class
public enum SpeakerPosition
{
    None = 0,
    Left = 1,
    Right = 2
}

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "Dialogue/NPC Dialogue")]
public class NPCdialogue : ScriptableObject
{

    // ✅ เพิ่มบรรทัดนี้
    [Header("Quest Integration")]
    public string dialogueID; // ID สำหรับเชื่อมกับ Quest System


    [Header("Character Info")]
    public string npcName;

    [Header("Portraits")]
    public Sprite npcPortrait; // เก็บไว้เพื่อ backward compatibility
    public Sprite leftPortrait;  // รูปตัวละครฝั่งซ้าย
    public Sprite rightPortrait; // รูปตัวละครฝั่งขวา

    [Header("🎬 Cutscene Settings")]
    [Tooltip("เปิดใช้งาน cutscene ก่อนเริ่ม dialogue")]
    public bool useCutscene = false;

    [Tooltip("รูป cutscene ที่จะแสดง")]
    public Sprite cutsceneImage;

    [Tooltip("ระยะเวลาแสดง cutscene (วินาที)")]
    public float cutsceneDuration = 2f;

    [Tooltip("ระยะเวลา fade in/out (วินาที)")]
    public float cutsceneFadeDuration = 0.5f;

    [Header("Dialogue Lines")]
    public string[] dialogueLines;

    [Header("Speaker Per Line (ใช้ตัวเลข: 0=None, 1=Left, 2=Right)")]
    public SpeakerPosition[] speakerPerLine;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Auto Progress")]
    public bool[] autoProgressLine;
    public float autoProgressDelay = 1f;

    [Header("End Dialogue")]
    public bool[] endDialogueLine;

    [Header("Choices")]
    public DialogueChoice[] choices;

    [Header("Statement Unlocks")]
    public StatementUnlockCondition[] statementUnlocks;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex;
    public string[] choice;
    public int[] nextDialogueIndex;
    public string[] statementToUnlock;
}

[System.Serializable]
public class StatementUnlockCondition
{
    public int dialogueIndex;
    public string[] statementIDs;
}