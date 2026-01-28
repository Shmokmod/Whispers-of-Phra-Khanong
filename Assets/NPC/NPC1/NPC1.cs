// ==================================================
// NPC.cs
// ระบบ NPC Dialogue + Choice + Cutscene + Detective Book
// ==================================================
using System.Collections;
using System.Linq;
using UnityEngine;
using GameSystem;

public class NPC : MonoBehaviour, IInteractable
{
    // ===============================
    // Inspector
    // ===============================
    [Header("Detective Book Integration")]
    [Tooltip("ID ของ Dialogue ใช้กับ DetectiveBook (เช่น npc_guard)")]
    public string dialogueID;

    [Header("Dialogue Data")]
    public NPCdialogue dialogueData;

    [Header("Visual")]
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private float fastTypingMultiplier = 0.1f;

    // ===============================
    // Controllers
    // ===============================
    private DialogueController dialogueUI;
    private CutsceneController cutsceneController;
    private Animator animator;

    // ===============================
    // Runtime State
    // ===============================
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;

    // ===============================
    // IInteractable
    // ===============================
    public bool CanInteract()
    {
        return !isDialogueActive && !CutsceneController.instance.IsPlayingCutscene();
    }

    public void Interact()
    {
        if (dialogueUI == null)
        {
            dialogueUI = DialogueController.instance;
            if (dialogueUI == null) return;
        }

        if (dialogueData == null) return;

        if (PauseController.isPaused && !isDialogueActive) return;

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    // ===============================
    // Unity Lifecycle
    // ===============================
    private void Start()
    {
        dialogueUI = DialogueController.instance;
        cutsceneController = CutsceneController.instance;
        animator = GetComponent<Animator>();

        if (dialogueUI != null &&
            dialogueUI.npcPortraitImage != null &&
            dialogueUI.npcPortraitImage.sprite == null &&
            defaultSprite != null)
        {
            dialogueUI.npcPortraitImage.sprite = defaultSprite;
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
                isTyping = false;
            }
            else
            {
                NextLine();
            }
        }
    }

    // ===============================
    // Dialogue Flow
    // ===============================
    public void ForceStartDialogue()
    {
        if (isDialogueActive) return;
        StartDialogue();
    }

    private void StartDialogue()
    {
        // เช็คว่ามี cutscene ก่อน dialogue หรือไม่
        var beforeCutscenes = GetCutscenesByTiming(CutsceneTiming.BeforeDialogue);

        if (beforeCutscenes.Length > 0)
        {
            // เล่น cutscene ก่อน แล้วค่อยเริ่ม dialogue
            PlayCutscenesSequentially(beforeCutscenes, InitializeDialogue);
        }
        else
        {
            // ไม่มี cutscene ก่อน dialogue ก็เริ่มเลย
            InitializeDialogue();
        }
    }

    private void InitializeDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;
        SetupPortraits();
        dialogueUI.ShowDialogue(true);
        PauseController.isPaused = true;
        Time.timeScale = 0f;
        DisplayCurrentLine();
    }

    private void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        if (IsEndDialogueLine())
        {
            EndDialogue();
            return;
        }

        if (TryDisplayChoices()) return;

        dialogueUI.ClearChoices();
        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
            HandleDialogueCutsceneOrContinue();
        else
            EndDialogue();
    }

    private void HandleDialogueCutsceneOrContinue()
    {
        var cutscenes = GetCutscenesByDialogueIndex(dialogueIndex);

        if (cutscenes.Length > 0)
        {
            // ซ่อน UI ชั่วคราว (ห้าม HideDialogue)
            dialogueUI.dialogueUI.SetActive(false);

            PlayCutscenesSequentially(cutscenes, () =>
            {
                dialogueUI.dialogueUI.SetActive(true);
                DisplayCurrentLine();
                CheckUnlockByDialogueIndex();
            });
        }
        else
        {
            DisplayCurrentLine();
            CheckUnlockByDialogueIndex();
        }
    }

    // ===============================
    // Dialogue Display
    // ===============================
    private void DisplayCurrentLine()
    {
        StopAllCoroutines();

        if (!string.IsNullOrEmpty(dialogueID))
            DetectiveBookManager.Instance?.MarkDialogueReached(dialogueID, dialogueIndex);

        SetupSpeaker();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");
        string line = dialogueData.dialogueLines[dialogueIndex];

        foreach (char c in line)
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + c);

            float speed = dialogueData.typingSpeed;
            if (Input.GetKey(KeyCode.Space))
                speed *= fastTypingMultiplier;

            yield return new WaitForSecondsRealtime(speed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLine != null &&
            dialogueIndex < dialogueData.autoProgressLine.Length &&
            dialogueData.autoProgressLine[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // ===============================
    // End Dialogue
    // ===============================
    private void EndDialogue()
    {
        var after = GetCutscenesByTiming(CutsceneTiming.AfterDialogue);
        CloseDialogueUI();

        if (after.Length > 0)
            PlayCutscenesSequentially(after, FinishDialogue);
        else
            FinishDialogue();
    }

    private void CloseDialogueUI()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();

        if (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null)
            dialogueUI.HideAllPortraits();
        else
            dialogueUI.npcPortraitImage.gameObject.SetActive(false);

        dialogueUI.HideDialogue();
    }

    private void FinishDialogue()
    {
        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    // ===============================
    // Portrait & Speaker
    // ===============================
    private void SetupPortraits()
    {
        bool two = dialogueData.leftPortrait && dialogueData.rightPortrait;

        if (two)
        {
            dialogueUI.SetupPortraits(
                dialogueData.leftPortrait,
                dialogueData.rightPortrait
            );
        }
        else
        {
            Sprite portrait = dialogueData.npcPortrait ?? defaultSprite;
            dialogueUI.SetNPCinfo(dialogueData.npcName, portrait);
            dialogueUI.npcPortraitImage.SetNativeSize();
            dialogueUI.npcPortraitImage.gameObject.SetActive(true);
        }
    }

    private void SetupSpeaker()
    {
        bool two = dialogueData.leftPortrait && dialogueData.rightPortrait;

        if (two && dialogueData.speakerPerLine != null &&
            dialogueIndex < dialogueData.speakerPerLine.Length)
        {
            var sp = dialogueData.speakerPerLine[dialogueIndex];
            dialogueUI.SetActiveSpeaker(sp);
            dialogueUI.SetSpeakerName(GetSpeakerName(sp));
        }
        else
        {
            dialogueUI.SetSpeakerName(dialogueData.npcName);
        }
    }

    private string GetSpeakerName(SpeakerPosition pos)
    {
        return pos switch
        {
            SpeakerPosition.Left => dialogueData.npcName,
            SpeakerPosition.Right => dialogueData.playerName,
            _ => dialogueData.npcName
        };
    }

    // ===============================
    // Choices
    // ===============================
    private bool TryDisplayChoices()
    {
        if (dialogueData.choices == null) return false;

        foreach (var choice in dialogueData.choices)
        {
            if (choice.dialogueIndex != dialogueIndex) continue;

            dialogueUI.ClearChoices();
            DisplayChoices(choice);
            return true;
        }

        return false;
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choice.Length; i++)
        {
            int idx = i;
            int next = choice.nextDialogueIndex[i];

            dialogueUI.CreateChoiceButton(
                choice.choice[i],
                () => ChooseOption(choice, idx, next)
            );
        }
    }

    private void ChooseOption(DialogueChoice choice, int choiceIndex, int nextIndex)
    {
        dialogueUI.ClearChoices();
        dialogueIndex = nextIndex;
        DisplayCurrentLine();
        UnlockFromChoice(choice, choiceIndex);
        DetectiveBookManager.Instance?.TryUnlockNotes();
    }

    private void UnlockFromChoice(DialogueChoice choice, int index)
    {
        if (choice.statementToUnlock == null || index >= choice.statementToUnlock.Length)
            return;

        string id = choice.statementToUnlock[index];
        if (string.IsNullOrEmpty(id)) return;

        if (GameDataRuntime.Instance.GetStatement(id) != null)
            GameDataRuntime.Instance.UnlockAndSaveStatement(id);
        else if (GameDataRuntime.Instance.GetEvidence(id) != null)
            GameDataRuntime.Instance.UnlockAndSaveEvidence(id);
        else
            DetectiveBookManager.Instance?.UnlockNote(id);
    }

    // ===============================
    // Dialogue Index Unlock
    // ===============================
    private void CheckUnlockByDialogueIndex()
    {
        if (dialogueData.statementUnlocks == null) return;

        foreach (var cond in dialogueData.statementUnlocks)
        {
            if (cond.dialogueIndex != dialogueIndex) continue;

            foreach (string id in cond.statementIDs)
            {
                if (GameDataRuntime.Instance.GetStatement(id) != null)
                    GameDataRuntime.Instance.UnlockAndSaveStatement(id);
                else if (GameDataRuntime.Instance.GetEvidence(id) != null)
                    GameDataRuntime.Instance.UnlockAndSaveEvidence(id);
            }

            DetectiveBookManager.Instance?.TryUnlockNotes();
        }
    }

    // ===============================
    // Cutscene Helpers
    // ===============================
    private CutsceneEvent[] GetCutscenesByTiming(CutsceneTiming timing)
    {
        if (dialogueData.cutsceneEvents == null)
            return new CutsceneEvent[0];

        return dialogueData.cutsceneEvents
            .Where(c => c.timing == timing)
            .ToArray();
    }

    private CutsceneEvent[] GetCutscenesByDialogueIndex(int index)
    {
        if (dialogueData.cutsceneEvents == null)
            return new CutsceneEvent[0];

        return dialogueData.cutsceneEvents
            .Where(c => c.timing == CutsceneTiming.AtDialogueIndex &&
                       c.dialogueIndex == index)
            .ToArray();
    }

    private void PlayCutscenesSequentially(
        CutsceneEvent[] cutscenes,
        System.Action onComplete)
    {
        StartCoroutine(PlayCutscenesRoutine(cutscenes, onComplete));
    }

    private IEnumerator PlayCutscenesRoutine(
        CutsceneEvent[] cutscenes,
        System.Action onComplete)
    {
        foreach (var cs in cutscenes)
        {
            bool finished = false;

            if (cs.cutsceneType == CutsceneType.Image)
                cutsceneController.PlayImageCutscene(cs, () => finished = true);
            else if (cs.cutsceneType == CutsceneType.Video)
                cutsceneController.PlayVideoCutscene(cs, () => finished = true);

            while (!finished)
                yield return null;
        }

        onComplete?.Invoke();
    }

    // ===============================
    // Utils
    // ===============================
    private bool IsEndDialogueLine()
    {
        return dialogueData.endDialogueLine != null &&
               dialogueIndex < dialogueData.endDialogueLine.Length &&
               dialogueData.endDialogueLine[dialogueIndex];
    }
}