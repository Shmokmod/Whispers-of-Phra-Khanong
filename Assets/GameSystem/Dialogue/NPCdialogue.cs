using UnityEngine;
using UnityEngine.Video;

// ✅ ย้าย enum มาไว้ด้านนอก class
public enum SpeakerPosition
{
    None = 0,
    Left = 1,
    Right = 2
}

public enum CutsceneType
{
    None,
    Image,    // แสดงรูปภาพ
    Video     // เล่นวิดีโอ
}

public enum CutsceneTiming
{
    BeforeDialogue,  // ก่อนเริ่ม dialogue
    AtDialogueIndex, // ตอนบรรทัดที่กำหนด
    AfterDialogue    // หลังจบ dialogue
}

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "Dialogue/NPC Dialogue")]
public class NPCdialogue : ScriptableObject
{
    [Header("Character Info")]
    public string npcName;

    [Header("Player Info")]
    public string playerName;

    [Header("Portraits")]
    public Sprite npcPortrait;
    public Sprite leftPortrait;
    public Sprite rightPortrait;

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

    [Header("🎬 Cutscenes & Videos")]
    public CutsceneEvent[] cutsceneEvents;
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

[System.Serializable]
public class CutsceneEvent
{
    [Header("Cutscene Type")]
    public CutsceneType cutsceneType = CutsceneType.Image;

    [Header("Timing")]
    public CutsceneTiming timing = CutsceneTiming.BeforeDialogue;

    [Tooltip("ถ้าเลือก AtDialogueIndex ให้ระบุ index ของบรรทัด dialogue")]
    public int dialogueIndex = 0;

    [Header("Image Cutscene (ถ้าเลือก Image)")]
    public Sprite cutsceneImage;
    public float imageDuration = 3f; // วินาที (0 = กดเพื่อข้าม)

    [Header("Video Cutscene (ถ้าเลือก Video)")]
    public VideoClip videoClip;
    public bool canSkipVideo = true;

    [Header("Fade Settings")]
    public bool useFadeIn = true;
    public bool useFadeOut = true;
    public float fadeDuration = 0.5f;




}