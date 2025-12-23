using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using GameSystem;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCdialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;
    private Animator animator;


    [SerializeField] private Sprite defaultSprite;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    void Start()
    {
        dialogueUI = DialogueController.instance;
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
        isDialogueActive = true;
        dialogueIndex = 0;

        // ✅ ตรวจสอบว่ามีรูป 2 รูปหรือไม่
        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits)
        {
            // ใช้ระบบ 2 portraits
            dialogueUI.SetupPortraits(dialogueData.leftPortrait, dialogueData.rightPortrait);
        }
        else
        {
            // ใช้ระบบเดิม (1 portrait)
            Sprite portrait = dialogueData.npcPortrait != null ? dialogueData.npcPortrait : defaultSprite;
            dialogueUI.SetNPCinfo(dialogueData.npcName, portrait);
            dialogueUI.npcPortraitImage.SetNativeSize();
            dialogueUI.npcPortraitImage.gameObject.SetActive(true);

            // ✅ เพิ่ม Debug
            Debug.Log($"leftPortrait: {dialogueData.leftPortrait?.name ?? "NULL"}");
            Debug.Log($"rightPortrait: {dialogueData.rightPortrait?.name ?? "NULL"}");
            Debug.Log($"useTwoPortraits: {useTwoPortraits}");
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

        // ไม่มีตัวเลือก → ไปบรรทัดถัดไป
        dialogueUI.ClearChoices();

        dialogueIndex++;

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
            CheckUnlockByDialogueIndex();
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

        // Auto progress line
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
        StopAllCoroutines();
        isDialogueActive = false;

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();

        // ซ่อนรูปทั้งหมด
        if (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null)
        {
            dialogueUI.HideAllPortraits();
        }
        else
        {
            dialogueUI.npcPortraitImage.gameObject.SetActive(false);
        }

        dialogueUI.ShowDialogue(false);

        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();

        // ✅ ตรวจสอบว่าใช้ระบบ 2 portraits หรือไม่
        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits && dialogueData.speakerPerLine != null &&
            dialogueIndex < dialogueData.speakerPerLine.Length)
        {
            // ใช้ระบบ 2 portraits
            SpeakerPosition speaker = dialogueData.speakerPerLine[dialogueIndex];
            dialogueUI.SetActiveSpeaker(speaker);

            // เปลี่ยนชื่อตามตำแหน่ง
            string speakerName = GetSpeakerName(speaker);
            dialogueUI.SetSpeakerName(speakerName);
        }
        else
        {
            // ใช้ระบบเดิม
            dialogueUI.SetSpeakerName(dialogueData.npcName);
        }

        StartCoroutine(TypeLine());
    }

    string GetSpeakerName(SpeakerPosition position)
    {
        switch (position)
        {
            case SpeakerPosition.Left:
                return "ตัวละคร A"; // แก้ไขชื่อตามที่ต้องการ
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
}