using UnityEngine;

[System.Serializable]
public class Quest
{
    public string questID;
    public string questName;
    public string description;

    // สถานะ quest
    public bool isActive = false;
    public bool isCompleted = false;

    // เชื่อมกับ Nav
    public Vector3 targetPosition; // ตำแหน่งที่ต้องไป
    public string targetNPCID; // NPC ที่ต้องคุยด้วย (ถ้ามี)

    // เชื่อมกับ Dialogue
    public string startDialogueID; // dialogue ที่เริ่ม quest
    public string completeDialogueID; // dialogue ที่จบ quest

    // Objective แบบง่ายๆ
    public string currentObjective;
}