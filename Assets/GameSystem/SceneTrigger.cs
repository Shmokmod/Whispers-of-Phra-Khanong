using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour, IInteractable
{
    [Header("Scene Settings")]
    [Tooltip("ชื่อ Scene ที่ต้องโหลด")]
    public string sceneToLoad = "NextScene";

    [Header("Spawn Point Settings")]
    [Tooltip("ชื่อ Spawn Point ใน Scene ปลายทาง")]
    public string targetSpawnPointName = "SpawnPoint_FromA";

    // ----------------------------------------------------------
    // IInteractable Implementation
    // ----------------------------------------------------------
    public bool CanInteract()
    {
        // ประตูวาป interact ได้เสมอ
        return true;
    }

    public void Interact()
    {
        Debug.Log($"[SceneTrigger] Interact -> Loading Scene: {sceneToLoad}, Spawn: {targetSpawnPointName}");

        // ส่งชื่อ spawn point ให้ Scene ปลายทาง
        SpawnManager.targetSpawnPointName = targetSpawnPointName;

        // ถ้ามีระบบ Fade / LoadingScreen
        if (LoadingScreen.Instance != null)
        {
            StartCoroutine(LoadingScreen.Instance.FadeTransition(() =>
            {
                SceneManager.LoadScene(sceneToLoad);
            }, 1f)); // delay หลังโหลดเสร็จ 1 วินาที
        }
        else
        {
            // ไม่มี LoadingScreen -> โหลดตรงๆ
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    // ----------------------------------------------------------
    // ตัวนี้ไม่มี OnTriggerEnter/Exit เพราะ PlayerDetector จะจัดการเอง
    // ประตูวาปแค่ต้องมี Collider + isTrigger = true
    // ----------------------------------------------------------
}
