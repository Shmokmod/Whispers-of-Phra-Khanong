using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;
    
    // เก็บชื่อ spawn point ที่ต้องการไปหา
    public static string targetSpawnPointName = "";

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

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene โหลดเสร็จ: {scene.name}, Target Spawn: '{targetSpawnPointName}'");
        
        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            // รอ 1 frame เพื่อให้ Scene โหลดเสร็จสมบูรณ์
            StartCoroutine(SpawnPlayerNextFrame(targetSpawnPointName));
            targetSpawnPointName = ""; // รีเซ็ตหลังใช้งาน
        }
        else
        {
            Debug.Log("ไม่มี Target Spawn Point - ใช้ตำแหน่งปกติ");
        }
    }

    IEnumerator SpawnPlayerNextFrame(string spawnPointName)
    {
        // รอให้ Scene โหลดเสร็จสมบูรณ์
        yield return new WaitForSeconds(0.1f);
        SpawnPlayerAtPoint(spawnPointName);
    }

    void SpawnPlayerAtPoint(string spawnPointName)
    {
        Debug.Log($"กำลังหา Spawn Point: '{spawnPointName}'");
        
        // หา spawn point ที่ต้องการ
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        
        if (spawnPoint != null)
        {
            Debug.Log($"เจอ Spawn Point ที่: {spawnPoint.transform.position}");
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                Vector3 oldPos = player.transform.position;
                Debug.Log($"เจอ Player ที่ตำแหน่งเก่า: {oldPos}");
                
                // ปิด Character Controller ก่อนเคลื่อนย้าย (ถ้ามี)
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }
                
                // เคลื่อนย้าย Player
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;
                
                if (controller != null)
                {
                    controller.enabled = true;
                }
                
                Vector3 newPos = player.transform.position;
                Debug.Log($"✓ Player ถูกย้ายไปที่: {spawnPointName}");
                Debug.Log($"  ตำแหน่งเก่า: {oldPos}");
                Debug.Log($"  ตำแหน่งใหม่: {newPos}");
                Debug.Log($"  ระยะห่าง: {Vector3.Distance(oldPos, newPos)} units");
            }
            else
            {
                Debug.LogError("✗ ไม่พบ Player ใน Scene! ตรวจสอบว่า Player มี Tag 'Player' หรือไม่");
            }
        }
        else
        {
            Debug.LogError($"✗ ไม่พบ Spawn Point ชื่อ: '{spawnPointName}' ใน Scene นี้");
            
            // แสดง GameObject ทั้งหมดที่มีคำว่า "Spawn" ในชื่อ
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            Debug.Log("=== GameObject ที่มีคำว่า Spawn ===");
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.Contains("Spawn"))
                {
                    Debug.Log($"  - {obj.name}");
                }
            }
        }
    }
}