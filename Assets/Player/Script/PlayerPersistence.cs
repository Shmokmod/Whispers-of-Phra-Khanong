using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance;

    //void Awake()
    //{
    //    if (Instance != null && Instance != this)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    Instance = this;
    //    transform.SetParent(null);
    //    DontDestroyOnLoad(gameObject);

    //    Debug.Log("[PLAYER] PlayerPersistence READY");
    //}

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    void SetPlayerVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;

        foreach (var c in GetComponentsInChildren<Collider>())
            c.enabled = visible;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[DBG] SceneLoaded fired: " + scene.name);

        string sceneName = scene.name.ToLower();
        Debug.Log($"[PLAYER] Scene loaded: {scene.name}");


        {
            var players = GameObject.FindGameObjectsWithTag("Player");

            foreach (var p in players)
            {
                // ถ้าไม่ใช่ PlayerPersistence ตัวนี้
                if (p.GetComponent<PlayerPersistence>() == null)
                {
                    Debug.Log("[PLAYER] Destroy scene player: " + p.name);
                    Destroy(p);
                }
            }

            gameObject.SetActive(true);
        }




        // ⬇️ ทำลาย Player ถ้าอยู่ใน MainMenu
        if (sceneName.Contains("mainmenu"))
        {
            Debug.Log("[PLAYER] ⚠️ In Menu Scene - Hided Player");

            // ทำลาย Instance
            if (Instance == this)
                Instance = null;

            SetPlayerVisible(false);
            return;
        }

        // ⬇️ ซ่อน Player ใน Cutscene
        if (sceneName.Contains("cutscene"))
        {
            Debug.Log("[PLAYER] 🙈 In Cutscene - Hiding Player");
            SetPlayerVisible(false);
            return;
        }

        // ⬇️ Scene เกม (Level1, GameScene, etc.)
        Debug.Log("[PLAYER] ✅ In Game Scene - Showing Player");
        SetPlayerVisible(true);

        var vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = transform;
            vcam.LookAt = transform;
        }

        // Apply saved position ถ้ากำลัง Load
        StartCoroutine(ApplyAfterSceneReady());
    }

    IEnumerator ApplyAfterSceneReady()
    {
        Debug.Log("[DBG] ApplyAfterSceneReady START");
        yield return new WaitForEndOfFrame();

        if (SaveManager.Instance != null && SaveManager.Instance.IsLoading)
        {
            Debug.Log("[DBG] Applying saved position");
            SaveManager.Instance.ApplyPlayerPosition(transform);
        }
        else
        {
            Debug.Log("[DBG] Not loading");
        }
    }

    void OnDestroy()
    {
        Debug.Log("[PLAYER] PlayerPersistence destroyed");

        if (Instance == this)
            Instance = null;
    }
}