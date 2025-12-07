using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int currentChapter;
    public string currentPhase;
    public int detectiveBoardPhase;

    public List<string> collectedEvidence;
    public List<string> completedDialogues;
    public List<string> discoveredClues;
    public List<string> unlockedLocations;
    public List<string> completedConnections;
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance;

    private const string SAVE_KEY = "DetectiveGameSave";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SaveGame()
    {
        SaveData data = new SaveData();

        // Story progress
        data.currentChapter = StoryManager.Instance.currentChapter;
        data.currentPhase = StoryManager.Instance.currentPhase.ToString();
        data.detectiveBoardPhase = DetectiveBoardManager.Instance.currentPhase;

        // Collections
        data.collectedEvidence = new List<string>(StoryManager.Instance.collectedEvidence);
        data.completedDialogues = new List<string>(StoryManager.Instance.completedDialogues);
        data.discoveredClues = new List<string>(StoryManager.Instance.discoveredClues);
        data.unlockedLocations = new List<string>(StoryManager.Instance.unlockedLocations);
        data.completedConnections = new List<string>(DetectiveBoardManager.Instance.completedConnections);

        // Save to PlayerPrefs
        string json = JsonUtility.ToJson(data, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        if (!HasSaveData())
        {
            Debug.Log("No save data found");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        // Restore story progress
        StoryManager.Instance.currentChapter = data.currentChapter;
        StoryManager.Instance.currentPhase = (StoryPhase)System.Enum.Parse(typeof(StoryPhase), data.currentPhase);
        DetectiveBoardManager.Instance.currentPhase = data.detectiveBoardPhase;

        // Restore collections
        StoryManager.Instance.collectedEvidence = new HashSet<string>(data.collectedEvidence);
        StoryManager.Instance.completedDialogues = new HashSet<string>(data.completedDialogues);
        StoryManager.Instance.discoveredClues = new HashSet<string>(data.discoveredClues);
        StoryManager.Instance.unlockedLocations = new HashSet<string>(data.unlockedLocations);
        DetectiveBoardManager.Instance.completedConnections = new List<string>(data.completedConnections);

        Debug.Log("Game Loaded!");
    }

    public bool HasSaveData()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
    }
}
//---

//## ✅ **สรุป: สิ่งที่ต้องทำใน Unity**

//### **Week 1 (Foundation):**
//1. ✅ สร้าง Canvas และ UI Layout
//2. ✅ สร้าง Prefabs (StatementCard, EvidenceCard)
//3. ✅ เขียน Data Classes (Statement, Evidence, GameData)
//4. ✅ ทำ Drag & Drop (StatementCard, EvidenceCard)
//5. ✅ ทดสอบ UI ใช้งานได้

//### **Week 2 (Logic):**
//6. ✅ DetectiveBoardManager - Phase 1 logic
//7. ✅ Hard-code connections สำหรับ Phase 1
//8. ✅ ResultPopup
//9. ✅ Integration กับ StoryManager
//10. ✅ Save/Load system

//---

//## 🎯 **Quick Start Checklist:**
//```
//□ สร้าง Canvas (Screen Space Overlay)
//□ สร้าง StatementCard.prefab
//□ สร้าง EvidenceCard.prefab
//□ สร้าง Scripts:
//  □ Statement.cs
//  □ Evidence.cs
//  □ GameData.cs
//  □ StatementCard.cs
//  □ EvidenceCard.cs
//  □ DetectiveBoardManager.cs
//  □ ResultPopup.cs
//  □ StoryManager.cs
//  □ AudioManager.cs
//  □ SaveSystem.cs
//□ Import DOTween(Asset Store - Free)
//□ ใส่ Sprites(Portraits, Evidence icons)
//□ ตั้งค่า Audio Clips
//□ ทดสอบ Drag & Drop
//□ Hard-code Phase 1 connections
//□ Test play!