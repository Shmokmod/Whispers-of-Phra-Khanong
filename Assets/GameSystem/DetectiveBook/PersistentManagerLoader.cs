using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ใส่ script นี้ใน Scene แรกของเกม (MainMenu หรือ Scene เริ่มต้น)
/// จะโหลด Scene "_PersistentManagers" ที่มี DetectiveBookManager
/// </summary>
public class PersistentManagerLoader : MonoBehaviour
{
    [Header("Manager Scene Settings")]
    [Tooltip("ชื่อ Scene ที่มี Manager ทั้งหมด")]
    public string managerSceneName = "_PersistentManagers";

    [Tooltip("โหลดครั้งเดียวตอนเริ่มเกม")]
    public bool loadOnce = true;

    private static bool hasLoaded = false;

    void Awake()
    {
        // ✅ ถ้าตั้งค่าให้โหลดครั้งเดียว และโหลดไปแล้ว → ข้าม
        if (loadOnce && hasLoaded)
        {
            Debug.Log("📚 Manager Scene already loaded, skipping...");
            return;
        }

        // ✅ เช็คว่า Manager Scene โหลดแล้วหรือยัง
        Scene managerScene = SceneManager.GetSceneByName(managerSceneName);

        if (!managerScene.isLoaded)
        {
            Debug.Log($"📚 Loading Manager Scene: {managerSceneName}");
            SceneManager.LoadSceneAsync(managerSceneName, LoadSceneMode.Additive);
            hasLoaded = true;
        }
        else
        {
            Debug.Log($"📚 Manager Scene '{managerSceneName}' is already loaded.");
        }
    }
}