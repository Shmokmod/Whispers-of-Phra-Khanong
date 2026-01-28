using UnityEngine;
using UnityEngine.Video;

public enum SpeakerPosition
{
    None = 0,
    Left = 1,
    Right = 2
}

public enum CutsceneType
{
    None,
    Image,
    Video
}

public enum CutsceneTiming
{
    BeforeDialogue,
    AtDialogueIndex,
    AfterDialogue
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

    [Header("Speaker Per Line")]
    public SpeakerPosition[] speakerPerLine;

    [Header("Typing Settings")]
    public float typingSpeed = 0.05f;

    [Header("Auto Progress")]
    public bool[] autoProgressLine;
    public float autoProgressDelay = 1f;

    [Header("End Dialogue")]
    public bool[] endDialogueLine;

    [Header("❌ Unskippable Dialogue")]
    public bool[] unskippableLine;

    [Header("Choices")]
    public DialogueChoice[] choices;

    [Header("Statement Unlocks")]
    public StatementUnlockCondition[] statementUnlocks;

    [Header("🎬 Cutscenes")]
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
    public CutsceneType cutsceneType;
    public CutsceneTiming timing;
    public int dialogueIndex;

    public Sprite cutsceneImage;
    public float imageDuration;

    public VideoClip videoClip;
    public bool canSkipVideo;

    public bool useFadeIn;
    public bool useFadeOut;
    public float fadeDuration;
}
