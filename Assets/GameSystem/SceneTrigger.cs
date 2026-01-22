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

    [Header("Visual Control - อย่าซ่อน GameObject หลัก!")]
    [Tooltip("GameObject ที่จะซ่อน/แสดง (sprite, particles)")]
    public GameObject[] visualObjects;
    
    [Tooltip("Collider ที่จะปิด/เปิด")]
    public CapsuleCollider portalCollider;
    
    [Tooltip("ShowPatchFollowWarpZone สำหรับแสดงเส้นทาง")]
    public ShowPatchFollowWarpZone pathGuideDisplay;

    private float lastInteractTime = -999f;
    private bool isUnlocked = false;

    private void Awake()
    {
        DebugLog("🎬 Awake() - Initializing SceneTrigger");
        
        // หา Collider อัตโนมัติถ้าไม่ได้กำหนด
        if (portalCollider == null)
        {
            portalCollider = GetComponent<CapsuleCollider>();
            if (portalCollider != null)
            {
                DebugLog($"✅ Auto-found CapsuleCollider: {portalCollider.GetType().Name}");
            }
        }
    }

    private void OnEnable()
    {
        DebugLog("🎬 OnEnable() called");
        
        // Subscribe event
        if (requireDialogue)
        {
            if (DetectiveBookManager.Instance == null)
            {
                DebugLog("⚠️ DetectiveBookManager.Instance is NULL in OnEnable!");
                return;
            }
            
            DetectiveBookManager.Instance.OnDialogueReached -= OnDialogueReachedEvent;
            DetectiveBookManager.Instance.OnDialogueReached += OnDialogueReachedEvent;
            DebugLog("✅ Subscribed to OnDialogueReached event");
        }
    }

    private void OnDisable()
    {
        DebugLog("🎬 OnDisable() called");
        
        // Unsubscribe เมื่อ disable
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnDialogueReached -= OnDialogueReachedEvent;
            DebugLog("❌ Unsubscribed from OnDialogueReached event");
        }
    }

    private void OnDestroy()
    {
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnDialogueReached -= OnDialogueReachedEvent;
        }
    }

    private void Start()
    {
        DebugLog($"🎬 Start() - requireDialogue: {requireDialogue}");
        
        // เช็คว่าต้องการ unlock หรือไม่
        if (requireDialogue)
        {
            StartCoroutine(DelayedUnlockCheck());
        }
        else
        {
            isUnlocked = true;
            ShowPortal();
        }
    }

    private IEnumerator DelayedUnlockCheck()
    {
        yield return new WaitForEndOfFrame();
        CheckUnlockStatus();
    }

    /// <summary>
    /// Event handler เมื่อมี dialogue reach
    /// </summary>
    private void OnDialogueReachedEvent(string dialogueID, int index)
    {
        DebugLog($"🔔 OnDialogueReachedEvent CALLED! dialogueID={dialogueID}, index={index}");
        
        if (!requireDialogue)
        {
            DebugLog("⚠️ Skipped: requireDialogue is false");
            return;
        }
        
        if (isUnlocked)
        {
            DebugLog("⚠️ Skipped: already unlocked");
            return;
        }

            if (!requireDialogue) return;

        if (dialogueID == requiredDialogueID && index == requiredDialogueIndex)
        {
            UnlockPortal();
        }

        string reachedKey = $"{dialogueID}_{index}";
        string requiredKey = $"{requiredDialogueID}_{requiredDialogueIndex}";

        DebugLog($"📖 Reached: {reachedKey}");
        DebugLog($"🔑 Required: {requiredKey}");
        DebugLog($"✅ Match: {reachedKey == requiredKey}");

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
        ShowPortal();
        DebugLog($"🚪 Portal Unlocked!");
    }

    /// <summary>
    /// แสดงประตู (visual + collider + path)
    /// </summary>
    private void ShowPortal()
    {
        DebugLog("👁️ ShowPortal() called");
        
        // แสดง Visual Objects
        if (visualObjects != null && visualObjects.Length > 0)
        {
            foreach (var obj in visualObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    DebugLog($"✅ Showing visual: {obj.name}");
                }
            }
        }

        // เปิด Collider
        if (portalCollider != null)
        {
            portalCollider.enabled = true;
            DebugLog("✅ Portal Collider enabled");
        }

        // แสดงเส้นทาง
        if (pathGuideDisplay != null)
        {
            pathGuideDisplay.ShowPathOnce();
            DebugLog("🗺️ Path guide activated!");
        }
    }

    /// <summary>
    /// ซ่อนประตู (visual + collider + path)
    /// </summary>
    private void HidePortal()
    {
        DebugLog("🙈 HidePortal() called");
        
        // ซ่อน Visual Objects
        if (visualObjects != null && visualObjects.Length > 0)
        {
            foreach (var obj in visualObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    DebugLog($"🙈 Hiding visual: {obj.name}");
                }
            }
        }

        // ปิด Collider
        if (portalCollider != null)
        {
            portalCollider.enabled = false;
            DebugLog("🚫 Portal Collider disabled");
        }

        // ซ่อนเส้นทาง
        if (pathGuideDisplay != null)
        {
            pathGuideDisplay.HidePath();
            DebugLog("🗺️ Path guide hidden");
        }
    }

    /// <summary>
    /// เช็คว่า dialogue key ถูก reach แล้วหรือยัง
    /// </summary>
    private void CheckUnlockStatus()
    {
        if (DetectiveBookManager.Instance == null)
        {
            DebugLog("⚠️ DetectiveBookManager not found!");
            HidePortal();
            return;
        }

        string key = $"{requiredDialogueID}_{requiredDialogueIndex}";
        isUnlocked = DetectiveBookManager.Instance.HasReachedDialogue(requiredDialogueID, requiredDialogueIndex);

        DebugLog($"🔍 Checking unlock status for: {key}");
        DebugLog($"📋 All reached dialogues: {string.Join(", ", DetectiveBookManager.Instance.reachedDialogueKeys)}");
        DebugLog($"🎯 Is Unlocked: {isUnlocked}");

        if (isUnlocked)
        {
            DebugLog($"✅ Dialogue already reached: {key} - Portal unlocked!");
            ShowPortal();
        }
        else
        {
            DebugLog($"🔒 Dialogue not reached: {key} - Portal locked!");
            HidePortal();
        }
    }

    /// <summary>
    /// เรียกใช้เพื่อเช็คสถานะใหม่
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
        if (Time.time - lastInteractTime < interactCooldown)
        {
            DebugLog("⚠️ Interact on cooldown");
            return false;
        }

        if (requireDialogue && !isUnlocked)
        {
            DebugLog($"🔒 Cannot interact - dialogue not reached: {requiredDialogueID}_{requiredDialogueIndex}");
            return false;
        }

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
        SpawnManager.targetSpawnPointName = targetSpawnPointName;
        SpawnManager.forceUseSpawnPoint = true; // ⭐ สำคัญ

        DebugLog($"📍 Set target spawn: {targetSpawnPointName}");

        if (LoadingScreen.Instance != null)
        {
            yield return StartCoroutine(LoadingScreen.Instance.LoadScene(sceneToLoad));
        }
        else
        {
            LoadScene();
        }
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