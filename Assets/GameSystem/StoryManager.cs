using Mono.Cecil.Cil;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    [Header("Progress")]
    public int currentChapter = 1;
    public StoryPhase currentPhase = StoryPhase.Chapter1_Investigation;

    // Flags
    public HashSet<string> collectedEvidence = new HashSet<string>();
    public HashSet<string> completedDialogues = new HashSet<string>();
    public HashSet<string> discoveredClues = new HashSet<string>();
    public HashSet<string> unlockedLocations = new HashSet<string>();
    public GameObject QuestUI;
    public GameObject AccusationUI;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartChapter1();
    }

    //==========================================
    // CHAPTER 1
    //==========================================
    void StartChapter1()
    {
        currentChapter = 1;
        currentPhase = StoryPhase.Chapter1_Investigation;

        UnlockLocation("crime_scene");
        UnlockNPC("security_guard_A");

        //QuestUI.Instance?.SetObjective("ตรวจสอบที่เกิดเหตุ");
    }

    public void OnEvidenceCollected(string evidenceID)
    {
        if (collectedEvidence.Contains(evidenceID)) return;

        collectedEvidence.Add(evidenceID);

        // Hard-coded triggers
        switch (evidenceID)
        {
            case "evid_cctv_footage":
                UnlockDialogue("guard_A_about_cctv");
                //QuestUI.Instance?.UpdateObjective("ถามยามเกี่ยวกับ CCTV");
                break;

            case "evid_timecard":
                DiscoverClue("guard_arrived_early");
                break;
        }

        CheckChapter1Progress();
    }

    public void OnDialogueCompleted(string dialogueID)
    {
        if (completedDialogues.Contains(dialogueID)) return;

        completedDialogues.Add(dialogueID);

        // Hard-coded triggers
        switch (dialogueID)
        {
            case "guard_A_initial":
                UnlockLocation("warehouse");
                //QuestUI.Instance?.SetObjective("ไปตรวจสอบโกดัง");
                break;

            case "worker_C_testimony":
                UnlockEvidence("evid_security_log");
                break;
        }

        CheckChapter1Progress();
    }

    void CheckChapter1Progress()
    {
        // เงื่อนไขเปิด Detective Board Phase 1
        bool hasKeyEvidence =
            collectedEvidence.Contains("evid_cctv_footage") &&
            collectedEvidence.Contains("evid_timecard") &&
            collectedEvidence.Contains("evid_security_log");

        bool hasKeyDialogues =
            completedDialogues.Contains("guard_A_initial") &&
            completedDialogues.Contains("worker_C_testimony");

        if (hasKeyEvidence && hasKeyDialogues)
        {
            StartDetectiveBoard_Phase1();
        }
    }

    //==========================================
    // DETECTIVE BOARD PHASES
    //==========================================
    void StartDetectiveBoard_Phase1()
    {
        currentPhase = StoryPhase.DetectiveBoard_Phase1;

        //QuestUI.Instance?.SetObjective("วิเคราะห์หลักฐานที่รวบรวมได้");

        // Show notification
        //NotificationUI.Instance?.Show("Detective Board ปลดล็อกแล้ว!");

        // Open detective board
        DetectiveBoardManager.Instance?.OpenUI();
    }

    public void OnCorrectConnection(string connectionKey)
    {
        // Triggers based on specific connections
        switch (connectionKey)
        {
            case "stmt_guard_time_evid_cctv_footage":
                // ยามโกหก - ปลดล็อกบทสนทนาใหม่
                UnlockDialogue("guard_A_confrontation");
                DiscoverClue("guard_is_lying");
                break;

            case "stmt_worker_saw_guard_evid_security_log":
                // ยืนยันคำให้การคนงาน
                DiscoverClue("guard_was_at_scene");
                break;
        }
    }

    public void OnDetectiveBoardPhaseComplete(int phase)
    {
        switch (phase)
        {
            case 1:
                Phase1Complete();
                break;
            case 2:
                Phase2Complete();
                break;
            case 3:
                Phase3Complete();
                break;
        }
    }

    void Phase1Complete()
    {
        currentPhase = StoryPhase.Chapter2_Investigation;

        // Show cutscene
        //CutsceneManager.Instance?.Play("phase1_complete");

        // Unlock new content
        UnlockLocation("partner_office");
        UnlockNPC("partner_D");

        //QuestUI.Instance?.SetObjective("สอบสวนหุ้นส่วน");
    }

    void Phase2Complete()
    {
        // ...
    }

    void Phase3Complete()
    {
        // Final confrontation
        StartFinale();
    }

    //==========================================
    // FINALE
    //==========================================
    void StartFinale()
    {
        currentPhase = StoryPhase.Finale;

        // Show final accusation scene
        //AccusationUI.Instance?.Show();
    }

    //public void AccuseCharacter(string npcID)
    //{
    //    if (npcID == "partner_D")
    //    {
    //        // Correct!
    //        CutsceneManager.Instance?.Play("true_ending");
    //        GameComplete();
    //    }
    //    else
    //    {
    //        // Wrong!
    //        CutsceneManager.Instance?.Play("wrong_accusation");
    //        GameOver();
    //    }
    //}

    void GameComplete()
    {
        // Show credits, stats, etc.
        //EndingUI.Instance?.ShowTrueEnding();
    }

    void GameOver()
    {
        // Retry option
        //EndingUI.Instance?.ShowGameOver();
    }

    //==========================================
    // HELPER FUNCTIONS
    //==========================================
    void UnlockLocation(string locationID)
    {
        if (unlockedLocations.Contains(locationID)) return;

        unlockedLocations.Add(locationID);
        //MapManager.Instance?.ShowLocation(locationID);

        //NotificationUI.Instance?.Show($"สถานที่ใหม่: {GetLocationName(locationID)}");
    }

    void UnlockNPC(string npcID)
    {
        //NPCManager.Instance?.ActivateNPC(npcID);
    }

    void UnlockDialogue(string dialogueID)
    {
        //DialogueManager.Instance?.MakeAvailable(dialogueID);
    }

    void UnlockEvidence(string evidenceID)
    {
        //InventoryManager.Instance?.AddEvidence(evidenceID);
        //NotificationUI.Instance?.Show($"หลักฐานใหม่: {GameData.GetEvidence(evidenceID).itemName}");
    }

    void DiscoverClue(string clueID)
    {
        if (discoveredClues.Contains(clueID)) return;

        discoveredClues.Add(clueID);
        //JournalUI.Instance?.AddClue(clueID);

        //NotificationUI.Instance?.Show("ค้นพบเบาะแสใหม่!");
    }

    string GetLocationName(string id)
    {
        switch (id)
        {
            case "crime_scene": return "ที่เกิดเหตุ";
            case "warehouse": return "โกดัง";
            case "partner_office": return "สำนักงานหุ้นส่วน";
            default: return id;
        }
    }
}

public enum StoryPhase
{
    Chapter1_Investigation,
    DetectiveBoard_Phase1,
    Chapter2_Investigation,
    DetectiveBoard_Phase2,
    Chapter3_Investigation,
    DetectiveBoard_Phase3,
    Finale
}
//```

//---

//## 🎨 **8. Unity Inspector Setup**

//### **StatementCard Prefab:**
//```
//StatementCard(GameObject)
//├── RectTransform(Width: 300, Height: 180)
//├── CanvasGroup
//├── Image (CardBackground)
//│   └── Color: (0.95, 0.95, 0.95)
//├── Portrait(Image)
//│   └── Size: 60x60, top-left corner
//├── SpeakerText (TextMeshPro)
//│   └── Font Size: 18, Bold
//├── ContentText (TextMeshPro)
//│   └── Font Size: 14, Text Area
//├── StatusIcon (Image)
//│   └── Size: 30x30, top-right corner
//└── GlowEffect (Image)
//    └── Color: Yellow, Alpha: 0.3, initially inactive
//```

//### **EvidenceCard Prefab:**
//```
//EvidenceCard (GameObject)
//├── RectTransform (Width: 250, Height: 280)
//├── Image(CardBackground)
//├── TypeBorder(Image - outline)
//├── Icon(Image)
//│   └── Size: 120x120, centered top
//├── NameText (TextMeshPro)
//│   └── Font Size: 16, Bold
//├── DescriptionText (TextMeshPro)
//│   └── Font Size: 12
//├── DropZoneHighlight(Image)
//│   └── Outline, initially inactive
//└── GlowEffect (Image)
//    └── initially inactive
//```

//### **DetectiveBoard Manager (Inspector):**
//```
//DetectiveBoardManager
//├── UI Panels
//│   ├── Board UI: [Assign]
//│   ├── Statement Container: [Assign]
//│   ├── Evidence Container: [Assign]
//│   ├── Result Popup: [Assign]
//│   └── Close Button: [Assign]
//├── Header
//│   ├── Phase Title Text: [Assign]
//│   └── Objective Text: [Assign]
//├── Progress
//│   ├── Progress Bar: [Assign]
//│   └── Progress Text: [Assign]
//├── Prefabs
//│   ├── Statement Card Prefab: [Assign]
//│   └── Evidence Card Prefab: [Assign]
//└── Current State
//    ├── Current Phase: 1
//    ├── Correct Connections This Phase: 0
//    └── Required Connections This Phase: 2