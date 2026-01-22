using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public static bool forceUseSpawnPoint = false;
    public static string targetSpawnPointName = "";

    [Header("Settings")]
    [SerializeField] private int waitFrames = 3;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool lockYRotation = true;
    [SerializeField] private float spawnHeight = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            DebugLog("✅ SpawnManager initialized");
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 🆕 รายชื่อ scene ที่ไม่ต้อง spawn
        string[] skipScenes = {
            "_PersistentManagers",
            "MainMenu",
            "Mainmenu",
            "MainMenu Scene",
            "Menu",
            "StartMenu",
            "Cutscene_Start"
        };

        foreach (string skipScene in skipScenes)
        {
            if (scene.name.Equals(skipScene, System.StringComparison.OrdinalIgnoreCase))
            {
                DebugLog($"⏭ Skipping spawn for: {scene.name}");
                return;
            }
        }

        // ถ้า scene มีคำว่า "Menu" หรือ "Cutscene" ก็ข้าม
        if (scene.name.Contains("Menu") || scene.name.Contains("Cutscene"))
        {
            DebugLog($"⏭ Skipping spawn for menu/cutscene: {scene.name}");
            return;
        }

        DebugLog($"📍 Scene loaded: {scene.name}");

        // 🆕 กรณีที่ 1: Load จาก Save
        if (SaveManager.Instance != null && SaveManager.Instance.IsLoading)
        {
            DebugLog("💾 Loading from save data...");
            StartCoroutine(SpawnAndApplySavePosition());
            return;
        }

        // กรณีที่ 2: ใช้ Portal/Spawn Point
        if (forceUseSpawnPoint && !string.IsNullOrEmpty(targetSpawnPointName))
        {
            DebugLog($"🚪 Spawn via portal → {targetSpawnPointName}");
            StartCoroutine(SpawnPlayerAtPointDelayed(targetSpawnPointName));
            forceUseSpawnPoint = false;
            targetSpawnPointName = "";
            return;
        }

        // กรณีที่ 3: Spawn ปกติ (ตำแหน่งเริ่มต้นใน scene)
        DebugLog("🎮 Normal spawn - using default position");
        StartCoroutine(SetupCameraDelayed());
    }

    // 🆕 Coroutine สำหรับ Load จาก Save
    IEnumerator SpawnAndApplySavePosition()
    {
        // รอให้ Player spawn จาก scene
        for (int i = 0; i < waitFrames; i++)
        {
            yield return null;
        }

        GameObject player = FindBestPlayer();

        if (player == null)
        {
            Debug.LogError("❌ No Player found to apply save position!");
            yield break;
        }

        // ใช้ SaveManager ตั้งตำแหน่ง
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ApplyPlayerPosition(player.transform);
            SaveManager.Instance.FinishLoading();
            DebugLog($"✅ Applied save position: {player.transform.position}");
        }

        // อัพเดทกล้อง
        UpdateCinemachineTarget(player.transform);
    }

    // Spawn ที่ Spawn Point เฉพาะ
    IEnumerator SpawnPlayerAtPointDelayed(string spawnPointName)
    {
        for (int i = 0; i < waitFrames; i++)
        {
            yield return null;
        }

        SpawnPlayerAtPoint(spawnPointName);
    }

    // Setup กล้องกรณี spawn ปกติ
    IEnumerator SetupCameraDelayed()
    {
        for (int i = 0; i < waitFrames; i++)
        {
            yield return null;
        }

        GameObject player = FindBestPlayer();

        if (player != null)
        {
            UpdateCinemachineTarget(player.transform);
            DebugLog($"✅ Camera setup for normal spawn at {player.transform.position}");
        }
        else
        {
            DebugLog("⚠️ No player found for camera setup (normal for menu scenes)");
        }
    }

    void SpawnPlayerAtPoint(string spawnPointName)
    {
        DebugLog($"🔍 Looking for Spawn Point: '{spawnPointName}'");

        GameObject spawnPoint = GameObject.Find(spawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogError($"❌ Spawn Point '{spawnPointName}' not found!");
            ListAvailableSpawnPoints();
            return;
        }

        DebugLog($"✅ Found Spawn Point at: {spawnPoint.transform.position}");

        GameObject player = FindBestPlayer();

        if (player == null)
        {
            Debug.LogError("❌ No Player found in scene!");
            return;
        }

        MovePlayerToSpawn(player, spawnPoint.transform);
        UpdateCinemachineTarget(player.transform);
    }

    GameObject FindBestPlayer()
    {
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        if (allPlayers.Length == 0)
        {
            // 🆕 ไม่ใช่ Error ถ้าไม่มี Player ใน scene บางตัว
            DebugLog("⚠️ No Player found in this scene (this may be normal for menus)");
            return null;
        }

        DebugLog($"========== Player Detection ==========");
        DebugLog($"Found {allPlayers.Length} Player(s)");

        for (int i = 0; i < allPlayers.Length; i++)
        {
            DebugLog($"  Player {i + 1}:");
            DebugLog($"    - Name: {allPlayers[i].name}");
            DebugLog($"    - Position: {allPlayers[i].transform.position}");
            DebugLog($"    - Active: {allPlayers[i].activeInHierarchy}");
        }
        DebugLog($"====================================");

        // เลือก Player ที่ active
        foreach (var p in allPlayers)
        {
            if (p.activeInHierarchy)
            {
                DebugLog($"✅ Selected Player: {p.name}");
                return p;
            }
        }

        // ถ้าไม่มีที่ active ใช้ตัวแรก
        DebugLog($"✅ Selected first Player: {allPlayers[0].name}");
        return allPlayers[0];
    }

    void MovePlayerToSpawn(GameObject player, Transform spawnTransform)
    {
        Vector3 oldPos = player.transform.position;
        DebugLog($"📦 Moving Player from {oldPos} to {spawnTransform.position}");

        MonoBehaviour playerMovement = player.GetComponent("PlayerMovement") as MonoBehaviour;
        CharacterController controller = player.GetComponent<CharacterController>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            DebugLog("🔒 Disabled PlayerMovement");
        }

        if (controller != null)
        {
            controller.enabled = false;
            DebugLog("🔒 Disabled CharacterController");
        }

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.None;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
            DebugLog("🔄 Reset Rigidbody");
        }

        Vector3 newPosition = spawnTransform.position;

        if (spawnHeight != 0f)
        {
            newPosition.y += spawnHeight;
        }

        player.transform.position = newPosition;

        if (lockYRotation)
        {
            Vector3 spawnRotation = spawnTransform.eulerAngles;
            player.transform.rotation = Quaternion.Euler(spawnRotation.x, 0f, spawnRotation.z);
            DebugLog($"🔒 Locked Y rotation");
        }
        else
        {
            player.transform.rotation = spawnTransform.rotation;
        }

        DebugLog($"📍 Set position to: {player.transform.position}");

        Physics.SyncTransforms();

        if (rb != null)
        {
            rb.WakeUp();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            DebugLog("⚡ Woke up Rigidbody");
        }

        if (controller != null)
        {
            controller.enabled = true;
            DebugLog("🔓 Enabled CharacterController");
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            DebugLog("🔓 Enabled PlayerMovement");
        }

        if (!player.activeInHierarchy)
        {
            player.SetActive(true);
            DebugLog("✅ Activated Player");
        }

        DebugLog($"✅ Player moved successfully to {player.transform.position}");
    }

    void UpdateCinemachineTarget(Transform playerTransform)
    {
        var cinemachineCameras = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsSortMode.None);

        if (cinemachineCameras.Length > 0)
        {
            foreach (var cam in cinemachineCameras)
            {
                cam.Follow = playerTransform;
                cam.LookAt = playerTransform;

                // บังคับ refresh
                cam.enabled = false;
                cam.enabled = true;

                DebugLog($"📷 Updated Cinemachine: {cam.name} -> {playerTransform.name}");
            }
        }
        else
        {
            DebugLog("⚠️ No Cinemachine Camera found in scene");
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

    public void SpawnPlayerManually(string spawnPointName)
    {
        StartCoroutine(SpawnPlayerAtPointDelayed(spawnPointName));
    }
}