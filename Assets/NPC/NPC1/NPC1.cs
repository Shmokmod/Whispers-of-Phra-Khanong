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

    private bool waitingForChoice = false;

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    void Start()
    {
        dialogueUI = DialogueController.instance;
        animator = GetComponent<Animator>();

        // ✅ Validate dialogueData
        if (dialogueData == null)
        {
            Debug.LogError($"❌ [{gameObject.name}] dialogueData is NULL!");
            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogError($"❌ [{gameObject.name}] DialogueController.instance is NULL!");
            return;
        }

        // ✅ Setup default sprite
        if (dialogueUI.npcPortraitImage != null && dialogueUI.npcPortraitImage.sprite == null)
            dialogueUI.npcPortraitImage.sprite = defaultSprite;

        FixCanvasSettings();
    }

    void FixCanvasSettings()
    {
        if (dialogueUI == null) return;

        Canvas canvas = dialogueUI.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }
    }

    public void Interact()
    {
        if (dialogueData == null)
        {
            Debug.LogError($"❌ [{gameObject.name}] Cannot interact - dialogueData is NULL!");
            return;
        }

        if (dialogueUI == null)
        {
            Debug.LogError($"❌ [{gameObject.name}] Cannot interact - dialogueUI is NULL!");
            return;
        }

        if (PauseController.isPaused && !isDialogueActive)
            return;

        if (isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    void StartDialogue()
    {
        Debug.Log($"🎬 === START DIALOGUE: {dialogueData.npcName} ===");

        isDialogueActive = true;
        dialogueIndex = 0;
        waitingForChoice = false;

        // ✅ Quest Integration
        if (!string.IsNullOrEmpty(dialogueData.dialogueID))
        {
            QuestManager.Instance?.OnDialogueTrigger(dialogueData.dialogueID);
        }

        // ✅ Cutscene
        if (dialogueData.useCutscene && dialogueData.cutsceneImage != null)
        {
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
        Debug.Log($"🖼️ Setting up portraits...");

        // ✅ ตรวจสอบว่าใช้ระบบ 2 portraits หรือไม่
        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits)
        {
            Debug.Log($"   Using TWO portraits mode");
            dialogueUI.SetupPortraits(dialogueData.leftPortrait, dialogueData.rightPortrait);

            // ✅ IMPORTANT: ต้องเซ็ต active speaker ตั้งแต่แรก
            if (dialogueData.speakerPerLine != null && dialogueData.speakerPerLine.Length > 0)
            {
                dialogueUI.SetActiveSpeaker(dialogueData.speakerPerLine[0]);
            }
        }
        else
        {
            Debug.Log($"   Using SINGLE portrait mode");
            Sprite portrait = dialogueData.npcPortrait != null ? dialogueData.npcPortrait : defaultSprite;

            if (portrait == null)
            {
                Debug.LogWarning($"⚠️ No portrait sprite available for {dialogueData.npcName}");
            }

            dialogueUI.SetNPCinfo(dialogueData.npcName, portrait);

            // ✅ CRITICAL FIX: ต้องเปิด portrait ด้วยตัวเอง
            if (dialogueUI.npcPortraitImage != null)
            {
                dialogueUI.npcPortraitImage.gameObject.SetActive(true);
                dialogueUI.npcPortraitImage.enabled = true;

                // ตั้งค่า alpha ให้แน่ใจว่ามองเห็น
                if (dialogueUI.npcPortraitImage.GetComponent<CanvasGroup>() != null)
                {
                    dialogueUI.npcPortraitImage.GetComponent<CanvasGroup>().alpha = 1f;
                }

                Debug.Log($"   ✅ Portrait activated: {dialogueUI.npcPortraitImage.gameObject.activeSelf}");
            }
        }

        // ✅ แสดง dialogue UI
        dialogueUI.ShowDialogue(true);

        // ✅ Pause game
        if (!PauseController.isPaused)
        {
            PauseController.isPaused = true;
            Time.timeScale = 0f;
        }

        Debug.Log($"📝 Starting first line (index 0)");
        DisplayCurrentLine();
    }

    void NextLine()
    {
        // ✅ ถ้ากำลังรอ Choice → ไม่ทำอะไร
        if (waitingForChoice)
        {
            return;
        }

        // ✅ ถ้ากำลังพิมพ์ → Skip animation
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
            CheckAndDisplayChoices();
            return;
        }

        // ✅ เช็คว่าเป็นบรรทัดสุดท้ายหรือไม่ (แต่ไม่จบทันที ถ้ามี Choice)
        bool isEndLine = dialogueData.endDialogueLine != null &&
                        dialogueIndex < dialogueData.endDialogueLine.Length &&
                        dialogueData.endDialogueLine[dialogueIndex];

        if (isEndLine && !HasChoiceAtCurrentIndex())
        {
            EndDialogue();
            return;
        }

        // ✅ ไปบรรทัดถัดไป
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

    // ✅ ฟังก์ชันช่วยเช็คว่ามี Choice หรือไม่
    bool HasChoiceAtCurrentIndex()
    {
        if (dialogueData.choices == null) return false;

        foreach (DialogueChoice choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
                return true;
        }
        return false;
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        string currentLine = dialogueData.dialogueLines[dialogueIndex];

        foreach (char letter in currentLine)
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text + letter);
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        // ✅ หลังพิมพ์เสร็จ → เช็ค Choice
        CheckAndDisplayChoices();

        // ✅ Auto progress (ถ้าไม่มี Choice และตั้งค่าไว้)
        if (!waitingForChoice &&
            dialogueData.autoProgressLine != null &&
            dialogueIndex < dialogueData.autoProgressLine.Length &&
            dialogueData.autoProgressLine[dialogueIndex])
        {
            yield return new WaitForSecondsRealtime(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void CheckAndDisplayChoices()
    {
        if (dialogueData.choices == null || dialogueData.choices.Length == 0)
        {
            waitingForChoice = false;
            return;
        }

        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                Debug.Log($"✅ Displaying {dialogueChoice.choice.Length} choices at index {dialogueIndex}");
                dialogueUI.ClearChoices();
                DisplayChoices(dialogueChoice);
                waitingForChoice = true;
                return;
            }
        }

        waitingForChoice = false;
    }

    public void EndDialogue()
    {
        Debug.Log($"🔚 === END DIALOGUE: {dialogueData.npcName} ===");

        StopAllCoroutines();
        isDialogueActive = false;
        waitingForChoice = false;

        dialogueUI.SetDialogueText("");
        dialogueUI.ClearChoices();

        // ✅ Quest Integration
        if (!string.IsNullOrEmpty(dialogueData.dialogueID))
        {
            QuestManager.Instance?.OnDialogueTrigger(dialogueData.dialogueID);
        }

        // ✅ ซ่อน portraits
        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        if (useTwoPortraits)
        {
            dialogueUI.HideAllPortraits();
        }
        else
        {
            if (dialogueUI.npcPortraitImage != null)
            {
                dialogueUI.npcPortraitImage.gameObject.SetActive(false);
            }
        }

        dialogueUI.ShowDialogue(false);

        // ✅ Resume game
        PauseController.isPaused = false;
        Time.timeScale = 1f;
    }

    void DisplayCurrentLine()
    {
        StopAllCoroutines();

        bool useTwoPortraits = (dialogueData.leftPortrait != null && dialogueData.rightPortrait != null);

        // ✅ Update speaker
        if (useTwoPortraits &&
            dialogueData.speakerPerLine != null &&
            dialogueIndex < dialogueData.speakerPerLine.Length)
        {
            SpeakerPosition speaker = dialogueData.speakerPerLine[dialogueIndex];
            dialogueUI.SetActiveSpeaker(speaker);

            string speakerName = GetSpeakerName(speaker);
            dialogueUI.SetSpeakerName(speakerName);

            Debug.Log($"📢 Speaker: {speakerName} ({speaker})");
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
                return "ตัวละคร A"; // เปลี่ยนเป็นชื่อจริงได้
            case SpeakerPosition.Right:
                return "ตัวละคร B"; // เปลี่ยนเป็นชื่อจริงได้
            default:
                return dialogueData.npcName;
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        Debug.Log($"🎯 Creating {choice.choice.Length} choice buttons");

        dialogueUI.ClearChoices();

        for (int i = 0; i < choice.choice.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndex[i];
            int choiceIndex = i;

            GameObject btn = dialogueUI.CreateChoiceButton(
                choice.choice[i],
                () => ChooseOption(choice, choiceIndex, nextIndex)
            );

            if (btn != null)
            {
                btn.SetActive(true);
            }
        }

        StartCoroutine(RebuildLayoutNextFrame());
    }

    IEnumerator RebuildLayoutNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();

        RectTransform containerRect = dialogueUI.choiceContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(containerRect);
        }
    }

    void ChooseOption(DialogueChoice choice, int choiceIndex, int nextIndex)
    {
        Debug.Log($"🎯 Choice selected: {choiceIndex} → Next index: {nextIndex}");

        waitingForChoice = false;
        dialogueUI.ClearChoices();
        dialogueIndex = nextIndex;

        // ✅ Unlock statements/evidence
        if (choice.statementToUnlock != null && choiceIndex < choice.statementToUnlock.Length)
        {
            string id = choice.statementToUnlock[choiceIndex];

            if (!string.IsNullOrEmpty(id))
            {
                if (GameDataRuntime.Instance.GetStatement(id) != null)
                {
                    GameDataRuntime.Instance.UnlockAndSaveStatement(id);
                }
                else if (GameDataRuntime.Instance.GetEvidence(id) != null)
                {
                    GameDataRuntime.Instance.UnlockAndSaveEvidence(id);
                }
            }
        }

        // ✅ เช็คว่าเป็นบรรทัดจบหรือไม่
        bool isEndLine = dialogueData.endDialogueLine != null &&
                        dialogueIndex < dialogueData.endDialogueLine.Length &&
                        dialogueData.endDialogueLine[dialogueIndex];

        if (isEndLine)
        {
            DisplayCurrentLine();
            StartCoroutine(EndDialogueAfterDelay(dialogueData.autoProgressDelay));
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    IEnumerator EndDialogueAfterDelay(float delay)
    {
        while (isTyping)
        {
            yield return null;
        }

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
                }
            }
        }
    }
}