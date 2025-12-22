using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest List")]
    public List<Quest> allQuests = new List<Quest>();

    private Quest currentQuest;

    [Header("Events")]
    public UnityEvent<Quest> onQuestStarted;
    public UnityEvent<Quest> onQuestCompleted;
    public UnityEvent<Quest> onQuestUpdated;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // รับ quest ใหม่
    public void StartQuest(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);

        if (quest != null && !quest.isActive && !quest.isCompleted)
        {
            quest.isActive = true;
            currentQuest = quest;

            Debug.Log($"Quest Started: {quest.questName}");

            onQuestStarted?.Invoke(quest);

            // เชื่อม Nav ไปยังเป้าหมาย
            if (quest.targetPosition != Vector3.zero)
            {
                SetNavTarget(quest.targetPosition);
            }
        }
    }

    // อัพเดท objective
    public void UpdateQuestObjective(string newObjective)
    {
        if (currentQuest != null && currentQuest.isActive)
        {
            currentQuest.currentObjective = newObjective;
            onQuestUpdated?.Invoke(currentQuest);
        }
    }

    // จบ quest
    public void CompleteQuest(string questID)
    {
        Quest quest = allQuests.Find(q => q.questID == questID);

        if (quest != null && quest.isActive)
        {
            quest.isActive = false;
            quest.isCompleted = true;

            Debug.Log($"Quest Completed: {quest.questName}");

            onQuestCompleted?.Invoke(quest);

            // ล้าง nav
            ClearNavTarget();

            if (currentQuest == quest)
                currentQuest = null;
        }
    }

    public Quest GetCurrentQuest()
    {
        return currentQuest;
    }

    // เชื่อมกับระบบ Nav - แก้ตรงนี้ให้เชื่อมกับ Nav System ของคุณ
    private void SetNavTarget(Vector3 position)
    {
        // เรียกใช้ Nav system ของคุณที่นี่
        // ตัวอย่าง: YourNavSystem.Instance.SetTarget(position);
        Debug.Log($"Nav target set to: {position}");
    }

    private void ClearNavTarget()
    {
        // ปิด Nav
        // ตัวอย่าง: YourNavSystem.Instance.ClearTarget();
        Debug.Log("Nav target cleared");
    }

    // เชื่อมกับ Dialogue System - เรียกฟังก์ชันนี้จาก Dialogue System
    public void OnDialogueTrigger(string dialogueID)
    {
        // เช็คว่า dialogue นี้เริ่ม quest ไหม
        Quest questToStart = allQuests.Find(q => q.startDialogueID == dialogueID);
        if (questToStart != null)
        {
            StartQuest(questToStart.questID);
        }

        // เช็คว่า dialogue นี้จบ quest ไหม
        Quest questToComplete = allQuests.Find(q => q.completeDialogueID == dialogueID);
        if (questToComplete != null)
        {
            CompleteQuest(questToComplete.questID);
        }
    }
}