using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Debug = UnityEngine.Debug;

public class SceneTrigger : MonoBehaviour, IInteractable
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ที่ต้องโหลด")]
    public string sceneToLoad = "NextScene";

    [Header("Spawn Point Settings")]
    [Tooltip("ชื่อ Spawn Point ใน Scene ปลายทาง")]
    public string targetSpawnPointName = "SpawnPoint_FromA";

    [Header("Unlock Requirements")]
    [Tooltip("ต้อง reach dialogue key นี้ก่อนถึงจะวาปได้")]
    public string requiredDialogueID = "";
    [Tooltip("Index ของ dialogue ที่ต้อง reach")]
    public int requiredDialogueIndex = 0;
    [Tooltip("ถ้าเปิดใช้งาน จะเช็คว่า reach dialogue แล้วหรือยัง")]
    public bool requireDialogue = false;

    [Header("Optional Settings")]
    [SerializeField] private float interactCooldown = 0.5f;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool hideWhenLocked = true; // ซ่อน GameObject ถ้ายังไม่ unlock

    private float lastInteractTime = -999f;
    private bool isUnlocked = false;

    private void Awake()
    {
        // Subscribe to dialogue events
        if (requireDialogue && DetectiveBookManager.Instance != null)
        {
            // ฟังเหตุการณ์ว่ามี dialogue reach ใหม่หรือไม่
            DetectiveBookManager.Instance.OnDialogueReached += OnDialogueReachedEvent;
            DebugLog("✅ Subscribed to OnDialogueReached event");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe เมื่อ destroy
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnDialogueReached -= OnDialogueReachedEvent;
        }
    }

    private void Start()
    {
        // เช็คว่าต้องการ unlock หรือไม่
        if (requireDialogue)
        {
            CheckUnlockStatus();
        }
        else
        {
            isUnlocked = true;
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Event handler เมื่อมี dialogue reach
    /// </summary>
    private void OnDialogueReachedEvent(string dialogueID, int index)
    {
        if (!requireDialogue || isUnlocked) return;

        string reachedKey = $"{dialogueID}_{index}";
        string requiredKey = $"{requiredDialogueID}_{requiredDialogueIndex}";

        DebugLog($"📖 Dialogue Reached Event: {reachedKey}");
        DebugLog($"🔑 Required Key: {requiredKey}");

        if (reachedKey == requiredKey)
        {
            DebugLog("✅ Required dialogue reached! Unlocking portal...");
            UnlockPortal();
        }
    }

    /// <summary>
    /// ปลดล็อกประตู
    /// </summary>
    private void UnlockPortal()
    {
        isUnlocked = true;
        gameObject.SetActive(true);
        DebugLog($"🚪 Portal Unlocked! GameObject is now active.");
    }

    /// <summary>
    /// เช็คว่า dialogue key ถูก reach แล้วหรือยัง
    /// </summary>
    private void CheckUnlockStatus()
    {
        if (DetectiveBookManager.Instance == null)
        {
            DebugLog("⚠️ DetectiveBookManager not found!");
            if (hideWhenLocked)
            {
                gameObject.SetActive(false);
            }
            return;
        }

        string key = $"{requiredDialogueID}_{requiredDialogueIndex}";
        isUnlocked = DetectiveBookManager.Instance.HasReachedDialogue(requiredDialogueID, requiredDialogueIndex);

        if (isUnlocked)
        {
            DebugLog($"✅ Dialogue already reached: {key} - Portal unlocked!");
            gameObject.SetActive(true);
        }
        else
        {
            DebugLog($"🔒 Dialogue not reached: {key} - Portal locked!");
            if (hideWhenLocked)
            {
                gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// เรียกใช้เพื่อเช็คสถานะใหม่ (เช่น หลังจาก dialogue เกิดขึ้น)
    /// </summary>
    public void RefreshUnlockStatus()
    {
        if (requireDialogue)
        {
            CheckUnlockStatus();
        }
    }

    public bool CanInteract()
    {
        // เช็ค cooldown
        if (Time.time - lastInteractTime < interactCooldown)
        {
            DebugLog("⚠️ Interact on cooldown");
            return false;
        }

        // เช็คว่า unlock แล้วหรือยัง
        if (requireDialogue && !isUnlocked)
        {
            DebugLog($"🔒 Cannot interact - dialogue not reached: {requiredDialogueID}_{requiredDialogueIndex}");
            return false;
        }

        // ตรวจสอบว่า LoadingScreen ไม่กำลัง loading อยู่
        if (LoadingScreen.Instance != null && LoadingScreen.Instance.IsLoading())
        {
            DebugLog("⚠️ Cannot interact - loading in progress");
            return false;
        }

        return true;
    }

    public void Interact()
    {
        if (!CanInteract())
        {
            return;
        }

        lastInteractTime = Time.time;

        DebugLog($"🚪 Interact -> Loading Scene: {sceneToLoad}, Spawn: {targetSpawnPointName}");
        StartCoroutine(LoadSceneWithTransition());
    }

    private IEnumerator LoadSceneWithTransition()
    {
        // ส่งชื่อ spawn point ให้ SpawnManager
        SpawnManager.targetSpawnPointName = targetSpawnPointName;
        DebugLog($"📍 Set target spawn: {targetSpawnPointName}");

        // ใช้ LoadSceneAsync
        if (LoadingScreen.Instance != null)
        {
            yield return StartCoroutine(LoadingScreen.Instance.LoadSceneAsync(sceneToLoad));
        }
        else
        {
            DebugLog("⚠️ LoadingScreen not found, loading directly");
            LoadScene();
        }

        DebugLog("✅ Scene transition complete");
    }

    private void LoadScene()
    {
        if (Application.CanStreamedLevelBeLoaded(sceneToLoad))
        {
            DebugLog($"✅ Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError($"❌ Scene '{sceneToLoad}' not found in Build Settings!");
            Debug.LogError("💡 Add scene in File > Build Settings");
            lastInteractTime = -999f;
        }
    }

    private void DebugLog(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[SceneTrigger] {message}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = requireDialogue ? Color.red : Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.75f);

#if UNITY_EDITOR
        string lockInfo = requireDialogue ? $"\n🔒 Requires: {requiredDialogueID}_{requiredDialogueIndex}" : "";
        
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"→ {sceneToLoad}\nSpawn: {targetSpawnPointName}{lockInfo}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = requireDialogue ? Color.red : Color.yellow },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            }
        );
#endif
    }
}