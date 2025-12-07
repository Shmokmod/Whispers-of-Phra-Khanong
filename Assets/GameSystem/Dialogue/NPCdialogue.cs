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
    [Header("Character Info")]
    public string npcName;
    [Header("Portraits")]
    public Sprite npcPortrait; // เก็บไว้เพื่อ backward compatibility
    public Sprite leftPortrait;  // รูปตัวละครฝั่งซ้าย
    public Sprite rightPortrait; // รูปตัวละครฝั่งขวา
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