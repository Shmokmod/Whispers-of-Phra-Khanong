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

        // 🎬 ตรวจสอบว่าต้องแสดง cutscene หรือไม่
        if (dialogueData.useCutscene && dialogueData.cutsceneImage != null)
        {
            // Pause เกมก่อน
            PauseController.isPaused = true;
            Time.timeScale = 0f;

            // แสดง cutscene ก่อน แล้วค่อยเริ่ม dialogue
            dialogueUI.ShowCutscene(
                dialogueData.cutsceneImage,
                dialogueData.cutsceneDuration,
                dialogueData.cutsceneFadeDuration,
                () => SetupDialogueAfterCutscene()
            );
        }
        else
        {
            // ไม่มี cutscene → เริ่ม dialogue ปกติ
            SetupDialogueAfterCutscene();
        }
    }

    void SetupDialogueAfterCutscene()
    {
        // ตรวจสอบว่ามีรูป 2 รูปหรือไม่
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

            Debug.Log($"leftPortrait: {dialogueData.leftPortrait?.name ?? "NULL"}");
            Debug.Log($"rightPortrait: {dialogueData.rightPortrait?.name ?? "NULL"}");
            Debug.Log($"useTwoPortraits: {useTwoPortraits}");
        }

        dialogueUI.ShowDialogue(true);

        // Pause เกม (ถ้ายังไม่ได้ pause)
        if (!PauseController.isPaused)
        {
            PauseController.isPaused = true;
            Time.timeScale = 0f;
        }

        DisplayCurrentLine();
    }

    void NextLine()
    {
        Debug.Log($"➡️ NextLine Called - Index: {dialogueIndex}, isTyping: {isTyping}");

        // ถ้ากำลังพิมพ์ → แสดงข้อความทั้งหมดทันที
        if (isTyping)
        {
            Debug.Log("⏭️ Skipping typing animation");
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

            // ✅ เช็ค Choice หลังแสดงข้อความทั้งหมด
            CheckAndDisplayChoices();
            return;
        }

        // ✅ ถ้ามี Choice อยู่แล้ว → ไม่ทำอะไร (รอให้เลือกก่อน)
        if (dialogueUI.choiceContainer.childCount > 0)
        {
            Debug.Log($"⚠️ Choices already displayed ({dialogueUI.choiceContainer.childCount} buttons). Waiting for selection.");
            return;
        }

        // ✅ เช็คว่าบรรทัดนี้เป็นบรรทัดสุดท้ายหรือไม่
        if (dialogueData.endDialogueLine != null &&
            dialogueData.endDialogueLine.Length > dialogueIndex &&
            dialogueData.endDialogueLine[dialogueIndex])
        {
            Debug.Log("🔚 End dialogue line detected");
            EndDialogue();
            return;
        }

        // ✅ ไม่มีตัวเลือก → ไปบรรทัดถัดไป
        dialogueUI.ClearChoices();
        dialogueIndex++;

        Debug.Log($"📄 Moving to next line. New Index: {dialogueIndex}");

        if (dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLine();
            CheckUnlockByDialogueIndex();
        }
        else
        {
            Debug.Log("🔚 No more dialogue lines. Ending dialogue.");
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        Debug.Log($"⌨️ TypeLine - Index: {dialogueIndex}, Text: {dialogueData.dialogueLines[dialogueIndex]}");

        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;
        Debug.Log($"✅ TypeLine Finished - Index: {dialogueIndex}");

        // ✅ หลังพิมพ์เสร็จ → เช็คว่ามี Choice หรือไม่
        CheckAndDisplayChoices();

        // Auto progress line (ถ้าไม่มี Choice)
        if (dialogueUI.choiceContainer.childCount == 0 &&
            dialogueData.autoProgressLine != null &&
            dialogueData.autoProgressLine.Length > dialogueIndex &&
            dialogueData.autoProgressLine[dialogueIndex])
        {
            Debug.Log($"⏩ Auto Progress - Waiting {dialogueData.autoProgressDelay}s");
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // ✅ ฟังก์ชันใหม่: เช็คและแสดง Choice
    void CheckAndDisplayChoices()
    {
        Debug.Log($"🔍 CheckAndDisplayChoices - Current Index: {dialogueIndex}");

        if (dialogueData.choices == null)
        {
            Debug.Log("⚠️ dialogueData.choices is NULL");
            return;
        }

        Debug.Log($"📋 Total Choices: {dialogueData.choices.Length}");

        if (dialogueData.choices.Length > 0)
        {
            foreach (DialogueChoice dialogueChoice in dialogueData.choices)
            {
                Debug.Log($"   - Choice at Index: {dialogueChoice.dialogueIndex}");

                if (dialogueChoice.dialogueIndex == dialogueIndex)
                {
                    Debug.Log($"✅ MATCH! Displaying {dialogueChoice.choice.Length} choices");
                    dialogueUI.ClearChoices();
                    DisplayChoices(dialogueChoice);
                    return;
                }
            }

            Debug.Log($"❌ No choice found for index {dialogueIndex}");
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

        // ตรวจสอบว่าใช้ระบบ 2 portraits หรือไม่
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
        Debug.Log($"🎯 DisplayChoices - Creating {choice.choice.Length} buttons");
        Debug.Log($"📦 Choice Container: {dialogueUI.choiceContainer.name}");
        Debug.Log($"   Active: {dialogueUI.choiceContainer.gameObject.activeSelf}");
        Debug.Log($"   Position: {dialogueUI.choiceContainer.position}");

        // เช็ค Layout Group
        var layoutGroup = dialogueUI.choiceContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (layoutGroup != null)
        {
            Debug.Log($"📐 HorizontalLayoutGroup found:");
            Debug.Log($"   Spacing: {layoutGroup.spacing}");
            Debug.Log($"   Child Force Expand Width: {layoutGroup.childForceExpandWidth}");
            Debug.Log($"   Enabled: {layoutGroup.enabled}");
        }
        else
        {
            Debug.LogWarning("⚠️ No HorizontalLayoutGroup found on ChoiceContainer!");
        }

        for (int i = 0; i < choice.choice.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndex[i];
            int choiceIndex = i;

            Debug.Log($"   Button {i}: '{choice.choice[i]}' → Index {nextIndex}");

            GameObject btn = dialogueUI.CreateChoiceButton(choice.choice[i],
                () => ChooseOption(choice, choiceIndex, nextIndex));

            Debug.Log($"      ├─ Created: {btn.name}");
            Debug.Log($"      ├─ Active: {btn.activeSelf}");
            Debug.Log($"      ├─ Position: {btn.transform.position}");
            Debug.Log($"      ├─ LocalPosition: {btn.transform.localPosition}");
            Debug.Log($"      └─ Parent: {btn.transform.parent.name}");
        }

        // บังคับ Rebuild Layout
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(dialogueUI.choiceContainer.GetComponent<RectTransform>());

        Debug.Log($"✅ Choice buttons created. Container child count: {dialogueUI.choiceContainer.childCount}");
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