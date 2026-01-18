using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PathGuideController : MonoBehaviour
{
    [Header("References")]
    public Transform target;                 // จุดหมาย (Empty / Obj)
    public GameObject footstepPrefab;        // Prefab รอยเท้าสีทอง

    [Header("Path Settings")]
    public float stepSpacing = 1.2f;          // ระยะห่างรอยเท้า
    public float footstepYOffset = 0.05f;     // ยกขึ้นจากพื้นนิดหน่อย

    [Header("Timing Settings")]
    public float updateInterval = 0.4f;       // อัปเดต path ทุกกี่วินาที
    public float showDuration = 5f;           // โชว์ path กี่วินาที

    [Header("Debug")]
    public bool debugMode = true;             // เปิด/ปิด debug log

    private NavMeshPath navPath;
    private List<GameObject> spawnedSteps = new List<GameObject>();
    private Coroutine pathRoutine;

    void Awake()
    {
        navPath = new NavMeshPath();
        DebugLog("✅ PathGuideController Awake");
    }

    void Start()
    {
        // เช็คว่า setup ครบหรือยัง
        if (target == null)
            Debug.LogError("❌ Target is NULL! กำหนด Target ใน Inspector");
        if (footstepPrefab == null)
            Debug.LogError("❌ Footstep Prefab is NULL! กำหนด Prefab ใน Inspector");
    }

    // ===============================
    // เรียกจากปุ่ม / UI / Input
    // ===============================
    public void TogglePath()
    {
        DebugLog($"🔄 TogglePath() เรียก! pathRoutine = {(pathRoutine != null ? "กำลังทำงาน" : "NULL")}");

        if (pathRoutine != null)
        {
            DebugLog("⏹️ หยุด path");
            StopCoroutine(pathRoutine);
            ClearPath();
            pathRoutine = null;
        }
        else
        {
            DebugLog("▶️ เริ่ม path");
            pathRoutine = StartCoroutine(PathUpdater());
        }
    }

    // ===============================
    // Coroutine อัปเดต path เรื่อย ๆ
    // ===============================
    IEnumerator PathUpdater()
    {
        DebugLog($"🔁 PathUpdater เริ่ม (จะแสดง {showDuration} วินาที)");
        float timer = 0f;

        while (timer < showDuration)
        {
            ShowPath();
            yield return new WaitForSeconds(updateInterval);
            timer += updateInterval;
        }

        DebugLog("⏰ หมดเวลา - ปิด path");
        ClearPath();
        pathRoutine = null;
    }

    // ===============================
    // คำนวณ + สร้างรอยเท้า
    // ===============================
    public void ShowPath()
    {
        DebugLog("🎯 ShowPath() เรียก");

        if (target == null)
        {
            Debug.LogError("❌ Target is NULL!");
            return;
        }

        if (footstepPrefab == null)
        {
            Debug.LogError("❌ Footstep Prefab is NULL!");
            return;
        }

        ClearPath();

        DebugLog($"📍 จาก: {transform.position} → ไป: {target.position}");

        bool foundPath = NavMesh.CalculatePath(
            transform.position,
            target.position,
            NavMesh.AllAreas,
            navPath
        );

        if (!foundPath)
        {
            Debug.LogWarning("⚠️ ไม่เจอ NavMesh path! ตรวจสอบว่า:");
            Debug.LogWarning("   1. มี NavMesh Bake แล้วหรือยัง?");
            Debug.LogWarning("   2. ตำแหน่งเริ่มต้นและปลายทางอยู่บน NavMesh หรือเปล่า?");
            return;
        }

        if (navPath.corners.Length < 2)
        {
            Debug.LogWarning($"⚠️ Path สั้นเกินไป (มี {navPath.corners.Length} corners)");
            return;
        }

        DebugLog($"✅ เจอ path! มี {navPath.corners.Length} มุม");

        int totalSteps = 0;

        for (int i = 0; i < navPath.corners.Length - 1; i++)
        {
            Vector3 start = navPath.corners[i];
            Vector3 end = navPath.corners[i + 1];

            float distance = Vector3.Distance(start, end);
            int stepCount = Mathf.FloorToInt(distance / stepSpacing);

            for (int j = 0; j < stepCount; j++)
            {
                float t = j / (float)stepCount;
                Vector3 pos = Vector3.Lerp(start, end, t);
                Vector3 dir = (end - start).normalized;

                SpawnFootstep(pos, dir);
                totalSteps++;
            }
        }

        DebugLog($"👣 สร้างรอยเท้าแล้ว {totalSteps} ก้าว");
    }

    // ===============================
    // Spawn รอยเท้า
    // ===============================
    void SpawnFootstep(Vector3 position, Vector3 direction)
    {
        GameObject step = Instantiate(
            footstepPrefab,
            position + Vector3.up * footstepYOffset,
            Quaternion.LookRotation(direction)
        );

        spawnedSteps.Add(step);
    }

    // ===============================
    // ลบรอยเท้าทั้งหมด
    // ===============================
    void ClearPath()
    {
        if (spawnedSteps.Count > 0)
        {
            DebugLog($"🗑️ ลบรอยเท้า {spawnedSteps.Count} ก้าว");
        }

        for (int i = 0; i < spawnedSteps.Count; i++)
        {
            if (spawnedSteps[i] != null)
                Destroy(spawnedSteps[i]);
        }

        spawnedSteps.Clear();
    }

    void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[PathGuide] {message}");
        }
    }

    // ===============================
    // Gizmos เพื่อดู path ใน Editor
    // ===============================
    void OnDrawGizmos()
    {
        if (target == null) return;

        // วาดเส้นจากตัวเองไป target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, target.position);
        Gizmos.DrawWireSphere(target.position, 0.5f);
    }
}