// ===============================
// NPC.cs
// ระบบ NPC Dialogue + Choice + Cutscene + Detective Book
// ===============================

using System.Collections;
using System.Linq;
using UnityEngine;
using GameSystem;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("Detective Book Integration")]
    [Tooltip("ID ของ Dialogue ใช้กับ DetectiveBook (เช่น npc_guard)")]
    public string dialogueID;

    [Header("Dialogue Data")]
    public NPCdialogue dialogueData;

    [Header("Visual")]
    [SerializeField] private Sprite defaultSprite;


    [SerializeField] private float fastTypingMultiplier = 0.1f;



    // Controllers
    private DialogueController dialogueUI;
    private CutsceneController cutsceneController;
    private Animator animator;

    // Runtime State
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

    // ===============================
    // Unity Lifecycle
    // ===============================
    private void Start()
    {
        // ⬇️ รอให้แน่ใจว่า DialogueController พร้อมใช้งาน
        if (DialogueController.instance == null)
        {
            Debug.LogError($"[NPC] {gameObject.name} - DialogueController.instance is NULL!");
            return;
        }

        dialogueUI = DialogueController.instance;

        if (CutsceneController.instance != null)
        {
            cutsceneController = CutsceneController.instance;
        }
        else
        {
            Debug.LogWarning($"[NPC] {gameObject.name} - CutsceneController not found");
        }

        animator = GetComponent<Animator>();

        // ตั้ง sprite เริ่มต้น
        if (dialogueUI != null &&
            dialogueUI.npcPortraitImage != null &&
            dialogueUI.npcPortraitImage.sprite == null &&
            defaultSprite != null)
        {
            dialogueUI.npcPortraitImage.sprite = defaultSprite;
        }

        Debug.Log($"[NPC] {gameObject.name} initialized successfully");
    }

    // ===============================
    // Interaction
    // ===============================
    public void Interact()
    {
        // ⬇️ ตรวจสอบ dialogueUI ก่อนใช้งาน
        if (dialogueUI == null)
        {
            Debug.LogError($"[NPC] {gameObject.name} - dialogueUI is NULL! Re-finding...");
            dialogueUI = DialogueController.instance;

            if (dialogueUI == null)
            {
                Debug.LogError("[NPC] Cannot find DialogueController!");
                return;
            }
        }

        if (dialogueData == null)
        {
            Debug.LogWarning($"[NPC] {gameObject.name} - No dialogue data!");
            return;
        }

        if (PauseController.isPaused && !isDialogueActive)
        {
            return;
        }

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
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

    private void StartDialogue()
    {
        InitializeDialogue();
    }

    private void NextLine()
    {
        // กดข้าม typing
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        // จบบท
        if (IsEndDialogueLine())
        {
            EndDialogue();
            return;
        }

        // ตัวเลือก
        if (TryDisplayChoices())
            return;

        dialogueUI.ClearChoices();
        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            HandleDialogueCutsceneOrContinue();
        }
        else
        {
            EndDialogue();
        }
    }

    // ===============================
    // Dialogue Display
    // ===============================
    private void DisplayCurrentLine()
    {
        StopAllCoroutines();

        // แจ้ง DetectiveBook ว่าถึง dialogue index นี้แล้ว
        if (!string.IsNullOrEmpty(dialogueID))
        {
            DetectiveBookManager.Instance?.MarkDialogueReached(dialogueID, dialogueIndex);
        }

        SetupSpeaker();
        StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        string line = dialogueData.dialogueLines[dialogueIndex];

        foreach (char letter in line)
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);

            float speed = dialogueData.typingSpeed;
            if (Input.GetKey(KeyCode.Space))
                speed *= fastTypingMultiplier;

            yield return new WaitForSecondsRealtime(speed);
        }

        isTyping = false;

        // ✅ autoProgress เดิม (ไม่แตะ)
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
        var afterCutscenes = GetCutscenesByTiming(CutsceneTiming.AfterDialogue);
        if (afterCutscenes.Length > 0)
        {
            CloseDialogueUI();
            PlayCutscenesSequentially(afterCutscenes, FinishDialogue);
        }
        else
        {
            CloseDialogueUI();
            FinishDialogue();
        }
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

        dialogueUI.ShowDialogue(false);
    }

    private void FinishDialogue()
    {
        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    // ===============================
    // Helpers
    // ===============================
    private void SetupPortraits()
    {
        bool twoPortraits =
            dialogueData.leftPortrait != null &&
            dialogueData.rightPortrait != null;

        if (twoPortraits)
        {
            dialogueUI.SetupPortraits(
                dialogueData.leftPortrait,
                dialogueData.rightPortrait
            );
        }
        else
        {
            Sprite portrait =
                dialogueData.npcPortrait != null
                ? dialogueData.npcPortrait
                : defaultSprite;

            dialogueUI.SetNPCinfo(dialogueData.npcName, portrait);
            dialogueUI.npcPortraitImage.SetNativeSize();
            dialogueUI.npcPortraitImage.gameObject.SetActive(true);
        }
    }

    private void SetupSpeaker()
    {
        bool twoPortraits =
            dialogueData.leftPortrait != null &&
            dialogueData.rightPortrait != null;

        if (twoPortraits &&
            dialogueData.speakerPerLine != null &&
            dialogueIndex < dialogueData.speakerPerLine.Length)
        {
            var speaker = dialogueData.speakerPerLine[dialogueIndex];
            dialogueUI.SetActiveSpeaker(speaker);
            dialogueUI.SetSpeakerName(GetSpeakerName(speaker));
        }
        else
        {
            dialogueUI.SetSpeakerName(dialogueData.npcName);
        }
    }

    private bool IsEndDialogueLine()
    {
        return dialogueData.endDialogueLine != null &&
               dialogueData.endDialogueLine.Length > dialogueIndex &&
               dialogueData.endDialogueLine[dialogueIndex];
    }

    private bool TryDisplayChoices()
    {
        if (dialogueData.choices == null) return false;

        foreach (var choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                dialogueUI.ClearChoices();
                DisplayChoices(choice);
                return true;
            }
        }
        return false;
    }

    private void HandleDialogueCutsceneOrContinue()
    {
        var cutscenes = GetCutscenesByDialogueIndex(dialogueIndex);
        if (cutscenes.Length > 0)
        {
            dialogueUI.ShowDialogue(false);
            PlayCutscenesSequentially(cutscenes, () =>
            {
                dialogueUI.ShowDialogue(true);
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

    private string GetSpeakerName(SpeakerPosition position)
    {
        return position switch
        {
            SpeakerPosition.Left => dialogueData.npcName,
            SpeakerPosition.Right => dialogueData.playerName,
            _ => dialogueData.npcName
        };
    }



    // ===============================
    // Choices
    // ===============================
    private void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choice.Length; i++)
        {
            int index = i;
            int nextIndex = choice.nextDialogueIndex[i];

            dialogueUI.CreateChoiceButton(
                choice.choice[i],
                () => ChooseOption(choice, index, nextIndex)
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

    private void UnlockFromChoice(DialogueChoice choice, int choiceIndex)
    {
        if (choice.statementToUnlock == null ||
            choiceIndex >= choice.statementToUnlock.Length)
            return;

        string id = choice.statementToUnlock[choiceIndex];
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
            .Where(c =>
                c.timing == CutsceneTiming.AtDialogueIndex &&
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
        foreach (var cutscene in cutscenes)
        {
            bool finished = false;

            if (cutscene.cutsceneType == CutsceneType.Image)
                cutsceneController.PlayImageCutscene(cutscene, () => finished = true);
            else if (cutscene.cutsceneType == CutsceneType.Video)
                cutsceneController.PlayVideoCutscene(cutscene, () => finished = true);

            while (!finished)
                yield return null;
        }

        onComplete?.Invoke();
    }
}
