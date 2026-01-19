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

    private NavMeshPath navPath;
    private List<GameObject> spawnedSteps = new List<GameObject>();
    private Coroutine pathRoutine;

    void Awake()
    {
        navPath = new NavMeshPath();
    }

    // ===============================
    // เรียกจากปุ่ม / UI / Input
    // ===============================
    public void TogglePath()
    {
        if (pathRoutine != null)
        {
            StopCoroutine(pathRoutine);
            ClearPath();
            pathRoutine = null;
        }
        else
        {
            pathRoutine = StartCoroutine(PathUpdater());
        }
    }

    // ===============================
    // Coroutine อัปเดต path เรื่อย ๆ
    // ===============================
    IEnumerator PathUpdater()
    {
        float timer = 0f;

        while (timer < showDuration)
        {
            ShowPath();
            yield return new WaitForSeconds(updateInterval);
            timer += updateInterval;
        }

        ClearPath();
        pathRoutine = null;
    }

    // ===============================
    // คำนวณ + สร้างรอยเท้า
    // ===============================
    void ShowPath()
    {
        if (target == null || footstepPrefab == null)
            return;

        ClearPath();

        bool foundPath = NavMesh.CalculatePath(
            transform.position,
            target.position,
            NavMesh.AllAreas,
            navPath
        );

        if (!foundPath || navPath.corners.Length < 2)
            return;

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
            }
        }
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
        for (int i = 0; i < spawnedSteps.Count; i++)
        {
            if (spawnedSteps[i] != null)
                Destroy(spawnedSteps[i]);
        }

        spawnedSteps.Clear();
    }
}
