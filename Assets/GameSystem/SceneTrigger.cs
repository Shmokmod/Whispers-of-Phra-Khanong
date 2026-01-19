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

    [Header("Optional Settings")]
    [SerializeField] private float interactCooldown = 0.5f;
    [SerializeField] private bool debugMode = true;

    private float lastInteractTime = -999f;

    public bool CanInteract()
    {
        // เช็ค cooldown
        if (Time.time - lastInteractTime < interactCooldown)
        {
            DebugLog("⚠️ Interact on cooldown");
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.75f);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1.5f,
            $"→ {sceneToLoad}\nSpawn: {targetSpawnPointName}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.yellow },
                fontSize = 12,
                fontStyle = FontStyle.Bold
            }
        );
#endif
    }
}