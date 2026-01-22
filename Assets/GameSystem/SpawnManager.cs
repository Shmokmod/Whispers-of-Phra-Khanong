using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Debug = UnityEngine.Debug;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public static bool forceUseSpawnPoint = false;


    // เก็บชื่อ spawn point ที่ต้องการไปหา
    public static string targetSpawnPointName = "";

    [Header("Settings")]
    [SerializeField] private int waitFrames = 5; // จำนวนเฟรมที่รอก่อน spawn
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool lockYRotation = true; // ล็อค Y rotation สำหรับ 2.5D
    [SerializeField] private float spawnHeight = 0f; // ความสูงเริ่มต้นถ้าต้องการปรับ

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("✅ SpawnManager initialized (2.5D Mode)");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 🔹 อยู่ระดับเดียวกับ Awake / OnEnable
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DebugLog($"📍 Scene loaded: {scene.name}");

        // ❌ ถ้าโหลดจาก save ปกติ และไม่ได้บังคับ spawn → ข้าม
        if (!forceUseSpawnPoint &&
            SaveManager.Instance != null &&
            SaveManager.Instance.HasSave(PlayerPrefs.GetInt("CurrentSlot", -1)))
        {
            DebugLog("⛔ Skip SpawnManager (load from save)");
            return;
        }

        // ✅ ใช้ SpawnPoint
        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            DebugLog($"🚪 Spawn via portal → {targetSpawnPointName}");
            StartCoroutine(SpawnPlayerNextFrame(targetSpawnPointName));
        }

        // reset ทุกครั้ง
        targetSpawnPointName = "";
        forceUseSpawnPoint = false;
    }




    IEnumerator SpawnPlayerNextFrame(string spawnPointName)
    {
        // รอหลายเฟรมให้แน่ใจว่า Player ตัวซ้ำถูก Destroy เสร็จ
        for (int i = 0; i < waitFrames; i++)
        {
            DebugLog($"⏳ Waiting frame {i + 1}/{waitFrames}...");
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        SpawnPlayerAtPoint(spawnPointName);
    }

    void SpawnPlayerAtPoint(string spawnPointName)
    {
        DebugLog($"🔍 Looking for Spawn Point: '{spawnPointName}'");

        // หา spawn point ที่ต้องการ
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogError($"❌ Spawn Point '{spawnPointName}' not found!");
            ListAvailableSpawnPoints();
            return;
        }

        DebugLog($"✅ Found Spawn Point at: {spawnPoint.transform.position}");

        // หา Player
        GameObject player = FindBestPlayer();

        if (player == null)
        {
            Debug.LogError("❌ No Player found in scene!");
            return;
        }

        // ย้าย Player
        MovePlayerToSpawn(player, spawnPoint.transform);

        // อัปเดต Camera
        UpdateCinemachineTarget(player.transform);
    }

    GameObject FindBestPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        DebugLog($"========== Player Detection ==========");
        DebugLog($"Found {allPlayers.Length} Player(s)");

        for (int i = 0; i < allPlayers.Length; i++)
        {
            DebugLog($"  Player {i + 1}:");
            DebugLog($"    - Name: {allPlayers[i].name}");
            DebugLog($"    - Position: {allPlayers[i].transform.position}");
            DebugLog($"    - Scene: {allPlayers[i].scene.name}");
            DebugLog($"    - Active: {allPlayers[i].activeInHierarchy}");
        }
        DebugLog($"====================================");

        if (allPlayers.Length == 0)
        {
            Debug.LogError("❌ No Player with 'Player' tag found!");
            return null;
        }

        // ลำดับความสำคัญในการเลือก Player:
        // 1. Player ใน DontDestroyOnLoad (persistent player)
        // 2. Player ที่ active
        // 3. Player ตัวแรก

        GameObject bestPlayer = null;

        // หา DontDestroyOnLoad player ก่อน
        foreach (var p in allPlayers)
        {
            if (p.scene.name == "DontDestroyOnLoad")
            {
                bestPlayer = p;
                DebugLog($"✅ Selected persistent Player: {p.name}");
                break;
            }
        }

        // ถ้าไม่มี ให้เลือก active player
        if (bestPlayer == null)
        {
            foreach (var p in allPlayers)
            {
                if (p.activeInHierarchy)
                {
                    bestPlayer = p;
                    DebugLog($"✅ Selected active Player: {p.name}");
                    break;
                }
            }
        }

        // ถ้ายังไม่มี ใช้ตัวแรก
        if (bestPlayer == null && allPlayers.Length > 0)
        {
            bestPlayer = allPlayers[0];
            DebugLog($"✅ Selected first Player: {bestPlayer.name}");
        }

        return bestPlayer;
    }

    void MovePlayerToSpawn(GameObject player, Transform spawnTransform)
    {
        Vector3 oldPos = player.transform.position;
        DebugLog($"📦 Moving Player from {oldPos} to {spawnTransform.position}");

        // ปิด components ชั่วคราว
        MonoBehaviour playerMovement = player.GetComponent("PlayerMovement") as MonoBehaviour;
        CharacterController controller = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // ปิด Movement Script
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            DebugLog("🔒 Disabled PlayerMovement");
        }

        // ปิด Character Controller
        if (controller != null)
        {
            controller.enabled = false;
            DebugLog("🔒 Disabled CharacterController");
        }

        // Reset Rigidbody (สำหรับ 3D/2.5D)
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.None;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            DebugLog("🔄 Reset Rigidbody (3D)");
        }

        // คำนวณตำแหน่งใหม่
        Vector3 newPosition = spawnTransform.position;

        // ถ้าต้องการปรับความสูง
        if (spawnHeight != 0f)
        {
            newPosition.y += spawnHeight;
        }

        // ย้าย Player
        player.transform.position = newPosition;

        // จัดการ Rotation สำหรับ 2.5D
        if (lockYRotation)
        {
            // ล็อค Y rotation แต่รักษา X, Z จาก spawn point
            Vector3 spawnRotation = spawnTransform.eulerAngles;
            player.transform.rotation = Quaternion.Euler(spawnRotation.x, 0f, spawnRotation.z);
            DebugLog($"🔒 Locked Y rotation, using X:{spawnRotation.x}, Z:{spawnRotation.z}");
        }
        else
        {
            player.transform.rotation = spawnTransform.rotation;
        }

        DebugLog($"📍 Set position to: {player.transform.position}");
        DebugLog($"📐 Set rotation to: {player.transform.eulerAngles}");

        // Sync Physics (3D)
        Physics.SyncTransforms();

        // Wake up Rigidbody
        if (rb != null)
        {
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // ตั้ง Interpolation กลับ
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            DebugLog("⚡ Woke up Rigidbody and restored interpolation");
        }

        // เปิด Character Controller กลับ
        if (controller != null)
        {
            controller.enabled = true;
            DebugLog("🔓 Enabled CharacterController");
        }

        // เปิด Movement Script กลับ
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            DebugLog("🔓 Enabled PlayerMovement");
        }

        // Ensure Player is active
        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
            DebugLog("✅ Activated Player");
        }

        Vector3 newPos = player.transform.position;
        float distance = Vector3.Distance(oldPos, newPos);

        DebugLog($"✅ Player moved successfully!");
        DebugLog($"   Old Position: {oldPos}");
        DebugLog($"   New Position: {newPos}");
        DebugLog($"   Distance: {distance:F2} units");
    }

    void UpdateCinemachineTarget(Transform playerTransform)
    {
        // Cinemachine 3.x
        var cinemachineCameras = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsSortMode.None);

        if (cinemachineCameras.Length > 0)
        {
            foreach (var cam in cinemachineCameras)
            {
                cam.Follow = playerTransform;
                cam.LookAt = playerTransform;
                DebugLog($"📷 Updated Cinemachine 3.x: {cam.name} -> Follow: {playerTransform.name}");
            }
        }
        else
        {
            // Fallback: ลองหา Cinemachine 2.x
#if CINEMACHINE_2
            var virtualCameras = FindObjectsByType<Cinemachine.CinemachineVirtualCamera>(FindObjectsSortMode.None);
            
            if (virtualCameras.Length > 0)
            {
                foreach (var cam in virtualCameras)
                {
                    cam.Follow = playerTransform;
                    cam.LookAt = playerTransform;
                    DebugLog($"📷 Updated Cinemachine 2.x: {cam.name}");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No Cinemachine Camera found in scene");
            }
#else
            Debug.LogWarning("⚠️ No Cinemachine Camera found in scene");
#endif
        }
    }

    void ListAvailableSpawnPoints()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        Debug.Log("=== Available Spawn Points ===");
        int count = 0;
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Spawn") || obj.name.Contains("spawn"))
            {
                Debug.Log($"  - {obj.name} at {obj.transform.position}");
                count++;
            }
        }

        if (count == 0)
        {
            Debug.Log("  (No spawn points found)");
        }
        Debug.Log("==============================");
    }

    void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[SpawnManager] {message}");
        }
    }

    // Optional: Public method to manually spawn player
    public void SpawnPlayerManually(string spawnPointName)
    {
        StartCoroutine(SpawnPlayerNextFrame(spawnPointName));
    }
}