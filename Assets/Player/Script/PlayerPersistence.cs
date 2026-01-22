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
        StartCoroutine(ApplyAfterSceneReady());
    }

    IEnumerator ApplyAfterSceneReady()
    {
        yield return new WaitForEndOfFrame();

        Debug.Log($"[LOAD] Apply Save in scene");
        SaveManager.Instance.ApplyPlayerPosition(transform);
        Debug.Log($"[PLAYER] Pos After Load = {transform.position}");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
