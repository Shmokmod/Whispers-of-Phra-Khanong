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
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene โหลดเสร็จ: {scene.name}, Target Spawn: '{targetSpawnPointName}'");

        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            // เรียก coroutine ที่รอหลายเฟรมเพื่อหลีกเลี่ยง race condition กับการ Destroy/Instantiate ของ Player
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
        // รอหลายเฟรมให้แน่ใจว่า Player ตัวซ้ำถูก Destroy เสร็จ (ลด MissingReference / race)
        // (ปรับจำนวนเฟรมตามความจำเป็น; 3-5 เฟรมมักพอ)
        for (int i = 0; i < 5; i++)
            yield return null;

        // ใช้รอเสร็จอีกนิดก่อนเรียกจริง
        yield return new WaitForEndOfFrame();

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

            // หา Player ทั้งหมด
            GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log($"========== ตรวจสอบ Player ==========");
            Debug.Log($"เจอ Player ทั้งหมด: {allPlayers.Length} ตัว");

            for (int i = 0; i < allPlayers.Length; i++)
            {
                Debug.Log($"  Player ตัวที่ {i + 1}:");
                Debug.Log($"    - ชื่อ: {allPlayers[i].name}");
                Debug.Log($"    - ตำแหน่ง: {allPlayers[i].transform.position}");
                Debug.Log($"    - Scene: {allPlayers[i].scene.name}");
                Debug.Log($"    - Instance ID: {allPlayers[i].GetInstanceID()}");
            }
            Debug.Log($"====================================");

            if (allPlayers.Length > 0)
            {
                // เลือก player แบบ robust:
                // - ถ้ามี player ที่ scene.name == "DontDestroyOnLoad" ให้ใช้ตัวนั้น (persistent)
                // - ถ้าไม่มี ให้ใช้ GameObject.FindGameObjectWithTag("Player") เป็น fallback
                GameObject player = null;
                foreach (var p in allPlayers)
                {
                    if (p.scene.name == "DontDestroyOnLoad")
                    {
                        player = p;
                        break;
                    }
                }

                if (player == null)
                {
                    // fallback: ใช้ตัวแรกที่ active
                    for (int i = 0; i < allPlayers.Length; i++)
                    {
                        if (allPlayers[i].activeInHierarchy)
                        {
                            player = allPlayers[i];
                            break;
                        }
                    }
                }

                if (player == null)
                {
                    player = GameObject.FindGameObjectWithTag("Player"); // สุดท้าย fallback
                }

                if (player == null)
                {
                    Debug.LogError("✗ ไม่พบ Player ที่สามารถใช้ได้ (หลังการเลือก)");
                    return;
                }

                Vector3 oldPos = player.transform.position;
                Debug.Log($"เลือกใช้ Player: {player.name} ที่ตำแหน่งเก่า: {oldPos}");

                // ปิด PlayerMovement ชั่วคราว
                MonoBehaviour playerMovement = player.GetComponent("PlayerMovement") as MonoBehaviour;
                if (playerMovement != null)
                {
                    playerMovement.enabled = false;
                    Debug.Log("ปิด PlayerMovement ชั่วคราว");
                }

                // ปิด Character Controller ก่อนเคลื่อนย้าย (ถ้ามี)
                CharacterController controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }

                // Reset Rigidbody (ถ้ามี)
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // ปิด interpolation ชั่วคราว และหยุด velocity
                    rb.interpolation = RigidbodyInterpolation.None;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.Sleep();
                    Debug.Log("รีเซ็ต Rigidbody velocity + Sleep + ปิด Interpolation");
                }

                // เคลื่อนย้าย Player
                player.transform.position = spawnPoint.transform.position;
                player.transform.rotation = spawnPoint.transform.rotation;

                // บังคับ Physics sync และ wake up rigidbody
                Physics.SyncTransforms();

                if (rb != null)
                {
                    rb.WakeUp();
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                Debug.Log($">>> หลังย้าย transform.position = {player.transform.position}");

                if (controller != null)
                {
                    controller.enabled = true;
                }

                // เปิด PlayerMovement อีกครั้ง
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                    Debug.Log("เปิด PlayerMovement อีกครั้ง");
                }

                // อัปเดต Cinemachine Camera
                UpdateCinemachineTarget(player.transform);

                Vector3 newPos = player.transform.position;
                Debug.Log($"✓ Player ถูกย้ายไปที่: {spawnPointName}");
                Debug.Log($"  ตำแหน่งเก่า: {oldPos}");
                Debug.Log($"  ตำแหน่งใหม่: {newPos}");
                Debug.Log($"  ระยะห่าง: {Vector3.Distance(oldPos, newPos)} units");

                // เปิด Player อีกครั้ง
                player.SetActive(true);
                Debug.Log("เปิด Player แล้ว");

                Debug.Log($">>> ตำแหน่งสุดท้ายหลังเปิด Player: {player.transform.position}");
            }
            else
            {
                Debug.LogError("✗ ไม่พบ Player ใน Scene! ตรวจสอบว่า Player มี Tag 'Player' หรือไม่");
            }
        }
        else
        {
            Debug.LogError($"✗ ไม่พบ Spawn Point ชื่อ: '{spawnPointName}' ใน Scene นี้");

            // แสดง GameObject ทั้งหมดที่มีคำว่า "Spawn" ในชื่อ (debug ช่วย)
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

    void UpdateCinemachineTarget(Transform playerTransform)
    {
        // หา Cinemachine Camera ทั้งหมด (Cinemachine 3.x)
        var cinemachineCameras = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsSortMode.None);

        foreach (var cam in cinemachineCameras)
        {
            cam.Follow = playerTransform;
            cam.LookAt = playerTransform;
            Debug.Log($"อัปเดต Cinemachine: {cam.name} -> Follow: {playerTransform.name}");
        }

        if (cinemachineCameras.Length == 0)
        {
            Debug.LogWarning("ไม่พบ Cinemachine Camera ใน Scene");
        }
    }
}
