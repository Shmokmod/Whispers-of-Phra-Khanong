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
            DebugLog("✅ Dialogue reached → delay → fade → cutscene");
            StartCoroutine(DelayedLoad());
        }
    }

    private IEnumerator DelayedLoad()
    {
        yield return new WaitForSecondsRealtime(delayBeforeLoad);

        Time.timeScale = 1f; // 🔥 สำคัญมาก

        if (LoadingScreen.Instance != null && !LoadingScreen.Instance.IsLoading)
        {
            yield return StartCoroutine(
                LoadingScreen.Instance.LoadScene(sceneToLoad)
            );
        }
        else
        {
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
