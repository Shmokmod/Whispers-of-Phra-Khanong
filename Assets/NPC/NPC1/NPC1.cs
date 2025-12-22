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

    // ✅ เพิ่มตัวแปรเช็คว่ากำลังรอ Choice อยู่หรือไม่
    private bool waitingForChoice = false;

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

        // ✅ ตรวจสอบและแก้ไข Canvas settings
        FixCanvasSettings();
    }

    void FixCanvasSettings()
    {
        Canvas canvas = dialogueUI.GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            // ตั้งค่า Render Mode
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogWarning("⚠️ Canvas RenderMode ไม่ถูกต้อง! กำลังแก้ไขเป็น ScreenSpaceOverlay");
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }

            // ตรวจสอบ Canvas Scaler
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;

                Debug.Log("✅ Canvas Scaler ถูกตั้งค่าแล้ว");
            }

            // เพิ่ม Graphic Raycaster ถ้าไม่มี
            if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                Debug.Log("✅ เพิ่ม GraphicRaycaster แล้ว");
            }
        }
        else
        {
            Debug.LogError("❌ ไม่พบ Canvas!");
        }
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
        waitingForChoice = false;

        // ✅ เพิ่มบรรทัดนี้ - บอก QuestManager ว่า dialogue เริ่มแล้ว
        if (!string.IsNullOrEmpty(dialogueData.dialogueID))
        {
            QuestManager.Instance?.OnDialogueTrigger(dialogueData.dialogueID);
        }
        // ✅ เพิ่มจบ

        if (dialogueData.useCutscene && dialogueData.cutsceneImage != null)
        {
            // ... โค้ดเดิมต่อ
            PauseController.isPaused = true;
            Time.timeScale = 0f;

            dialogueUI.ShowCutscene(
                dialogueData.cutsceneImage,
                dialogueData.cutsceneDuration,
                dialogueData.cutsceneFadeDuration,
                () => SetupDialogueAfterCutscene()
            );
        }
        else
        {
            SetupDialogueAfterCutscene();
        }
    }

    void SetupDialogueAfterCutscene()
    {
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

        if (!PauseController.isPaused)
        {
            PauseController.isPaused = true;
            Time.timeScale = 0f;
        }

        DisplayCurrentLine();
    }

    void NextLine()
    {
        Debug.Log($"➡️ NextLine Called - Index: {dialogueIndex}, isTyping: {isTyping}, waitingForChoice: {waitingForChoice}");

        // ✅ ถ้ากำลังรอ Choice → ไม่ทำอะไร (ต้องเลือกก่อน)
        if (waitingForChoice)
        {
            Debug.Log("⚠️ Waiting for choice selection. Ignoring NextLine.");
            return;
        }

        // ถ้ากำลังพิมพ์ → แสดงข้อความทั้งหมดทันที
        if (isTyping)
        {
            Debug.Log("⏭️ Skipping typing animation");
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;

            // เช็ค Choice หลังแสดงข้อความทั้งหมด
            CheckAndDisplayChoices();
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

        // ไปบรรทัดถัดไป
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

        // หลังพิมพ์เสร็จ → เช็คว่ามี Choice หรือไม่
        CheckAndDisplayChoices();

        // Auto progress (ถ้าไม่มี Choice)
        if (!waitingForChoice && // ✅ เพิ่มเงื่อนไข
            dialogueData.autoProgressLine != null &&
            dialogueData.autoProgressLine.Length > dialogueIndex &&
            dialogueData.autoProgressLine[dialogueIndex])
        {
            Debug.Log($"⏩ Auto Progress - Waiting {dialogueData.autoProgressDelay}s");
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // ✅ ฟังก์ชันเช็คและแสดง Choice (ปรับปรุง)
    void CheckAndDisplayChoices()
    {
        Debug.Log($"🔍 CheckAndDisplayChoices - Current Index: {dialogueIndex}");

        if (dialogueData.choices == null || dialogueData.choices.Length == 0)
        {
            Debug.Log("⚠️ No choices defined");
            waitingForChoice = false; // ✅ ไม่มี Choice
            return;
        }

        Debug.Log($"📋 Total Choices: {dialogueData.choices.Length}");

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                Debug.Log($"✅ MATCH! Displaying {dialogueChoice.choice.Length} choices at index {dialogueIndex}");
                dialogueUI.ClearChoices();
                DisplayChoices(dialogueChoice);
                waitingForChoice = true; // ✅ ตั้งค่ารอ Choice
                return;
            }
        }

        Debug.Log($"❌ No choice found for index {dialogueIndex}");
        waitingForChoice = false; // ✅ ไม่มี Choice
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        waitingForChoice = false; // ✅ Reset

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();

        if (!string.IsNullOrEmpty(dialogueData.dialogueID))
        {
            QuestManager.Instance?.OnDialogueTrigger(dialogueData.dialogueID);
        }

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
        Debug.Log($"🎯 DisplayChoices - Creating {choice.choice.Length} buttons");

        // ✅ STEP 1: Clear ก่อน (ป้องกันปุ่มเก่าค้างอยู่)
        dialogueUI.ClearChoices();

        // ✅ STEP 2: สร้างปุ่มทั้งหมด
        for (int i = 0; i < choice.choice.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndex[i];
            int choiceIndex = i;

            Debug.Log($"   Button {i}: '{choice.choice[i]}' → Index {nextIndex}");

            GameObject btn = dialogueUI.CreateChoiceButton(choice.choice[i],
                () => ChooseOption(choice, choiceIndex, nextIndex));

            // ✅ บังคับให้ active ทันที
            btn.SetActive(true);

            Debug.Log($"      ├─ Created: {btn.name}");
            Debug.Log($"      ├─ Active: {btn.activeSelf}");
        }

        // ✅ STEP 3: Force Layout Rebuild (แบบถูกวิธี)
        StartCoroutine(RebuildLayoutNextFrame());

        Debug.Log($"✅ Choice buttons created. Container child count: {dialogueUI.choiceContainer.childCount}");
    }

    // ✅ Coroutine สำหรับ Rebuild Layout ในเฟรมถัดไป
    IEnumerator RebuildLayoutNextFrame()
    {
        // รอให้ UI render ก่อน
        yield return null;

        // Force rebuild ทุกอย่าง
        Canvas.ForceUpdateCanvases();

        RectTransform containerRect = dialogueUI.choiceContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);

            // Log ตำแหน่งหลัง rebuild
            Debug.Log("🔄 Layout Rebuilt:");
            for (int i = 0; i < dialogueUI.choiceContainer.childCount; i++)
            {
                Transform child = dialogueUI.choiceContainer.GetChild(i);
                Debug.Log($"   Button {i}: Position = {child.position}, LocalPos = {child.localPosition}");
            }
        }
    }

    void ChooseOption(DialogueChoice choice, int choiceIndex, int nextIndex)
    {
        Debug.Log($"🎯 ChooseOption Called - Choice: {choiceIndex}, Next Index: {nextIndex}");

        waitingForChoice = false; // ✅ Reset ก่อน
        dialogueUI.ClearChoices();
        dialogueIndex = nextIndex;

        // ✅ เช็คว่าเป็นบรรทัดจบหรือไม่
        if (dialogueData.endDialogueLine != null &&
            dialogueData.endDialogueLine.Length > dialogueIndex &&
            dialogueData.endDialogueLine[dialogueIndex])
        {
            Debug.Log("🔚 Choice leads to end dialogue");

            // แสดงบรรทัดสุดท้ายก่อนจบ
            DisplayCurrentLine();

            // ✅ ใช้ Coroutine เพื่อรอให้แสดงข้อความเสร็จก่อนปิด
            StartCoroutine(EndDialogueAfterDelay(dialogueData.autoProgressDelay));
            return;
        }

        DisplayCurrentLine();

        // Unlock statements/evidence
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

    // ✅ Coroutine สำหรับรอก่อนจบ dialogue
    IEnumerator EndDialogueAfterDelay(float delay)
    {
        // รอให้ข้อความพิมพ์เสร็จ
        while (isTyping)
        {
            yield return null;
        }

        // รอเวลาตามที่กำหนด
        yield return new WaitForSecondsRealtime(delay);

        EndDialogue();
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