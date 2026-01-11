using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameSystem;
using System.Linq;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCdialogue dialogueData;
    private DialogueController dialogueUI;
    private CutsceneController cutsceneController;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private Animator animator;

    [SerializeField] private Sprite defaultSprite;

    public bool CanInteract()
    {
        return !isDialogueActive && !CutsceneController.instance.IsPlayingCutscene();
    }

    void Start()
    {
        dialogueUI = DialogueController.instance;
        cutsceneController = CutsceneController.instance;
        animator = GetComponent<Animator>();

        if (dialogueUI.npcPortraitImage != null && dialogueUI.npcPortraitImage.sprite == null)
            dialogueUI.npcPortraitImage.sprite = defaultSprite;
    }

    public void Interact()
    {
        if (dialogueData == null || (PauseController.isPaused && !isDialogueActive))
            return;

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    void StartDialogue()
    {
        // ✅ เช็ค Cutscene ก่อนเริ่ม Dialogue
        var beforeCutscenes = GetCutscenesByTiming(CutsceneTiming.BeforeDialogue);

        if (beforeCutscenes.Length > 0)
        {
            PlayCutscenesSequentially(beforeCutscenes, () =>
            {
                InitializeDialogue();
            });
        }
        else
        {
            InitializeDialogue();
        }
    }

    void InitializeDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits)
        {
            dialogueUI.SetupPortraits(dialogueData.leftPortrait, dialogueData.rightPortrait);
        }
        else
        {
            Sprite portrait = dialogueData.npcPortrait != null ? dialogueData.npcPortrait : defaultSprite;
            dialogueUI.SetNPCinfo(dialogueData.npcName, portrait);
            dialogueUI.npcPortraitImage.SetNativeSize();
            dialogueUI.npcPortraitImage.gameObject.SetActive(true);
        }

        dialogueUI.ShowDialogue(true);

        PauseController.isPaused = true;
        Time.timeScale = 0f;

        DisplayCurrentLine();
    }

    void NextLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            return;
        }

        // จบบท
        if (dialogueData.endDialogueLine != null &&
            dialogueData.endDialogueLine.Length > dialogueIndex &&
            dialogueData.endDialogueLine[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // ตัวเลือก
        if (dialogueData.choices != null && dialogueData.choices.Length > 0)
        {
            foreach (DialogueChoice dialogueChoice in dialogueData.choices)
            {
                if (dialogueChoice.dialogueIndex == dialogueIndex)
                {
                    dialogueUI.ClearChoices();
                    DisplayChoices(dialogueChoice);
                    return;
                }
            }
        }

        dialogueUI.ClearChoices();
        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            // ✅ เช็ค Cutscene ที่ตำแหน่งนี้
            var cutscenes = GetCutscenesByDialogueIndex(dialogueIndex);

            if (cutscenes.Length > 0)
            {
                // ปิด Dialogue ชั่วคราว
                dialogueUI.ShowDialogue(false);

                PlayCutscenesSequentially(cutscenes, () =>
                {
                    // เปิด Dialogue กลับมา
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
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (dialogueData.autoProgressLine != null &&
            dialogueData.autoProgressLine.Length > dialogueIndex &&
            dialogueData.autoProgressLine[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndDialogue()
    {
        // ✅ เช็ค Cutscene หลังจบ Dialogue
        var afterCutscenes = GetCutscenesByTiming(CutsceneTiming.AfterDialogue);

        if (afterCutscenes.Length > 0)
        {
            // ปิด Dialogue UI ก่อน
            CloseDialogueUI();

            PlayCutscenesSequentially(afterCutscenes, () =>
            {
                FinishDialogue();
            });
        }
        else
        {
            CloseDialogueUI();
            FinishDialogue();
        }
    }

    void CloseDialogueUI()
    {
        StopAllCoroutines();
        isDialogueActive = false;

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();

        if (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null)
        {
            dialogueUI.HideAllPortraits();
        }
        else
        {
            dialogueUI.npcPortraitImage.gameObject.SetActive(false);
        }

        dialogueUI.ShowDialogue(false);
    }

    void FinishDialogue()
    {
        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();

        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits && dialogueData.speakerPerLine != null &&
            dialogueIndex < dialogueData.speakerPerLine.Length)
        {
            SpeakerPosition speaker = dialogueData.speakerPerLine[dialogueIndex];
            dialogueUI.SetActiveSpeaker(speaker);

            string speakerName = GetSpeakerName(speaker);
            dialogueUI.SetSpeakerName(speakerName);
        }
        else
        {
            dialogueUI.SetSpeakerName(dialogueData.npcName);
        }

        StartCoroutine(TypeLine());
    }

    string GetSpeakerName(SpeakerPosition position)
    {
        switch (position)
        {
            case SpeakerPosition.Left:
                return "ตัวละคร A";
            case SpeakerPosition.Right:
                return "ตัวละคร B";
            default:
                return dialogueData.npcName;
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choice.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndex[i];
            int choiceIndex = i;

            dialogueUI.CreateChoiceButton(choice.choice[i],
                () => ChooseOption(choice, choiceIndex, nextIndex));
        }
    }

    void ChooseOption(DialogueChoice choice, int choiceIndex, int nextIndex)
    {
        dialogueUI.ClearChoices();
        dialogueIndex = nextIndex;

        DisplayCurrentLine();

        if (choice.statementToUnlock != null && choice.statementToUnlock.Length > 0)
        {
            foreach (string id in choice.statementToUnlock)
            {
                if (GameDataRuntime.Instance.GetStatement(id) != null)
                {
                    GameDataRuntime.Instance.UnlockAndSaveStatement(id);
                }
                else if (GameDataRuntime.Instance.GetEvidence(id) != null)
                {
                    GameDataRuntime.Instance.UnlockAndSaveEvidence(id);
                }
                else
                {
                    Debug.LogWarning($"⚠️ ID '{id}' ไม่พบใน Statement หรือ Evidence");
                }
            }
        }
    }

    void CheckUnlockByDialogueIndex()
    {
        if (dialogueData.statementUnlocks == null) return;

        foreach (var cond in dialogueData.statementUnlocks)
        {
            if (cond.dialogueIndex == dialogueIndex)
            {
                foreach (string id in cond.statementIDs)
                {
                    if (GameDataRuntime.Instance.GetStatement(id) != null)
                    {
                        GameDataRuntime.Instance.UnlockAndSaveStatement(id);
                    }
                    else if (GameDataRuntime.Instance.GetEvidence(id) != null)
                    {
                        GameDataRuntime.Instance.UnlockAndSaveEvidence(id);
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ ID '{id}' ไม่พบใน Statement หรือ Evidence");
                    }
                }
            }
        }
    }

    // ==================== Cutscene Helpers ====================

    CutsceneEvent[] GetCutscenesByTiming(CutsceneTiming timing)
    {
        if (dialogueData.cutsceneEvents == null)
            return new CutsceneEvent[0];

        return dialogueData.cutsceneEvents
            .Where(c => c.timing == timing)
            .ToArray();
    }

    CutsceneEvent[] GetCutscenesByDialogueIndex(int index)
    {
        if (dialogueData.cutsceneEvents == null)
            return new CutsceneEvent[0];

        return dialogueData.cutsceneEvents
            .Where(c => c.timing == CutsceneTiming.AtDialogueIndex && c.dialogueIndex == index)
            .ToArray();
    }

    void PlayCutscenesSequentially(CutsceneEvent[] cutscenes, System.Action onComplete)
    {
        StartCoroutine(PlayCutscenesRoutine(cutscenes, onComplete));
    }

    IEnumerator PlayCutscenesRoutine(CutsceneEvent[] cutscenes, System.Action onComplete)
    {
        foreach (var cutscene in cutscenes)
        {
            bool cutsceneFinished = false;

            if (cutscene.cutsceneType == CutsceneType.Image)
            {
                cutsceneController.PlayImageCutscene(cutscene, () => cutsceneFinished = true);
            }
            else if (cutscene.cutsceneType == CutsceneType.Video)
            {
                cutsceneController.PlayVideoCutscene(cutscene, () => cutsceneFinished = true);
            }

            // รอจนกว่า Cutscene จะเล่นจบ
            while (!cutsceneFinished)
                yield return null;
        }

        onComplete?.Invoke();
    }
}