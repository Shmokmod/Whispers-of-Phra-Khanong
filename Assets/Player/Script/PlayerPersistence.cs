using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (transform.parent != null)
                transform.SetParent(null);

            DontDestroyOnLoad(gameObject);

            Debug.Log("[PLAYER] PlayerPersistence instance created");
        }
        else
        {
            Debug.Log("[PLAYER] Duplicate PlayerPersistence destroyed");
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
        string sceneName = scene.name.ToLower();
        Debug.Log($"[PLAYER] Scene loaded: {scene.name}");

        // ⬇️ ทำลาย Player ถ้าอยู่ใน MainMenu
        if (sceneName.Contains("mainmenu") || sceneName.Contains("menu"))
        {
            Debug.Log("[PLAYER] ⚠️ In Menu Scene - Destroying Player");

            // ทำลาย Instance
            if (Instance == this)
                Instance = null;

            Destroy(gameObject);
            return;
        }

        // ⬇️ ซ่อน Player ใน Cutscene
        if (sceneName.Contains("cutscene"))
        {
            Debug.Log("[PLAYER] 🙈 In Cutscene - Hiding Player");
            gameObject.SetActive(false);
            return;
        }

        // ⬇️ Scene เกม (Level1, GameScene, etc.)
        Debug.Log("[PLAYER] ✅ In Game Scene - Showing Player");
        gameObject.SetActive(true);

        // Apply saved position ถ้ากำลัง Load
        StartCoroutine(ApplyAfterSceneReady());
    }

    IEnumerator ApplyAfterSceneReady()
    {
        yield return new WaitForEndOfFrame();

        if (SaveManager.Instance != null && SaveManager.Instance.IsLoading)
        {
            Debug.Log("[LOAD] Apply Save in scene");
            SaveManager.Instance.ApplyPlayerPosition(transform);
            Debug.Log($"[PLAYER] Pos After Load = {transform.position}");
        }
        else
        {
            Debug.Log("[PLAYER] Not loading from save - keeping default position");
        }
    }

    void OnDestroy()
    {
        Debug.Log("[PLAYER] PlayerPersistence destroyed");

        if (Instance == this)
            Instance = null;
    }
}