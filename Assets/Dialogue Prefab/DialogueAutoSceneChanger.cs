using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueAutoSceneChanger_NoSpawn : MonoBehaviour
{
    [Header("Dialogue Condition")]
    public string requiredDialogueID;
    public int requiredDialogueIndex;

    [Header("Scene Settings")]
    public string sceneToLoad;

    [Header("Delay Settings")]
    public float delayBeforeLoad = 1.5f;

    [Header("Debug")]
    public bool debugMode = true;

    private bool triggered = false;

    private void OnEnable()
    {
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnDialogueReached += OnDialogueReached;
        }
    }

    private void OnDisable()
    {
        if (DetectiveBookManager.Instance != null)
        {
            DetectiveBookManager.Instance.OnDialogueReached -= OnDialogueReached;
        }
    }

    private void Start()
    {
        if (DetectiveBookManager.Instance == null) return;

        if (DetectiveBookManager.Instance.HasReachedDialogue(
            requiredDialogueID,
            requiredDialogueIndex))
        {
            triggered = true;
            StartCoroutine(DelayedLoad());
        }
    }

    private void OnDialogueReached(string dialogueID, int index)
    {
        if (triggered) return;

        if (dialogueID == requiredDialogueID && index == requiredDialogueIndex)
        {
            triggered = true;
            DebugLog($"✅ Dialogue reached → delay {delayBeforeLoad}s → fade → scene");
            StartCoroutine(DelayedLoad());
        }
    }

    private IEnumerator DelayedLoad()
    {
        DebugLog($"🔄 DelayedLoad START (delay: {delayBeforeLoad}s)");
        yield return new WaitForSecondsRealtime(delayBeforeLoad);

        Time.timeScale = 1f;

        // ✅ เพิ่ม Debug ละเอียด
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"📋 DialogueAutoSceneChanger attempting to load: {sceneToLoad}");
        Debug.Log($"   LoadingScreen.Instance: {LoadingScreen.Instance != null}");
        Debug.Log($"   IsLoading: {(LoadingScreen.Instance != null ? LoadingScreen.Instance.IsLoading.ToString() : "N/A")}");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━");

        if (LoadingScreen.Instance != null)
        {
            if (!LoadingScreen.Instance.IsLoading)
            {
                DebugLog($"✅ Using LoadingScreen.LoadScene()");
                yield return StartCoroutine(
                    LoadingScreen.Instance.LoadScene(sceneToLoad)
                );
            }
            else
            {
                Debug.LogError($"❌ LoadingScreen is already loading! Using fallback");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else
        {
            Debug.LogError($"❌ LoadingScreen.Instance is NULL! Using fallback");
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void DebugLog(string msg)
    {
        if (debugMode)
        {
            Debug.Log($"[DialogueAutoSceneChanger] {msg}");
        }
    }
}