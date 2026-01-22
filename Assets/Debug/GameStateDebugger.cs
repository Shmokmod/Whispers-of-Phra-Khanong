using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateDebugger : MonoBehaviour
{
    float lastTimeScale;
    bool lastPaused;
    bool lastIsLoading;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Debug.Log("[DEBUG] GameStateDebugger started");
    }

    void Update()
    {
        // --- TimeScale ---
        if (Time.timeScale != lastTimeScale)
        {
            Debug.Log($"[DEBUG][TimeScale] {lastTimeScale} → {Time.timeScale} | Scene: {SceneManager.GetActiveScene().name}");
            lastTimeScale = Time.timeScale;
        }

        // --- Pause ---
        if (PauseController.isPaused != lastPaused)
        {
            Debug.Log($"[DEBUG][Pause] {lastPaused} → {PauseController.isPaused}");
            lastPaused = PauseController.isPaused;
        }

        // --- Save / Load ---
        if (SaveManager.Instance != null &&
            SaveManager.Instance.IsLoading != lastIsLoading)
        {
            Debug.Log($"[DEBUG][IsLoading] {lastIsLoading} → {SaveManager.Instance.IsLoading}");
            lastIsLoading = SaveManager.Instance.IsLoading;
        }

        // --- Key Debug ---
        if (Input.GetKeyDown(KeyCode.F9))
        {
            DumpState("F9 Manual Check");
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
        DumpState($"SceneLoaded: {scene.name}");
    }

    void DumpState(string reason)
    {
        Debug.Log(
            $"[DEBUG][DUMP] {reason}\n" +
            $"- Scene: {SceneManager.GetActiveScene().name}\n" +
            $"- TimeScale: {Time.timeScale}\n" +
            $"- Pause: {PauseController.isPaused}\n" +
            $"- IsLoading: {(SaveManager.Instance != null ? SaveManager.Instance.IsLoading : false)}\n" +
            $"- DialogueActive: {(DialogueController.instance != null && DialogueController.instance.dialogueUI.activeSelf)}"
        );
    }
}
