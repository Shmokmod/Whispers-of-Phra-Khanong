using UnityEngine;
using UnityEngine.Video;
using System.Collections;  

public class CutsceneChangeScene : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextScene = "level3"; // ✅ เพิ่มตัวแปรนี้

    private bool isEnding = false; // ✅ ป้องกันเรียกซ้ำ

    void Start()
    {
        if (videoPlayer != null)
        {
            Debug.Log("สมัครรับฟังเหตุการณ์ videoPlayer.loopPointReached");
            videoPlayer.loopPointReached += OnVideoFinished;
            Debug.Log("เริ่มเล่น Cutscene");
            videoPlayer.Play();
        }
        else
        {
            Debug.LogError("ไม่พบ VideoPlayer component บน GameObject!");
        }
    }

    public void OnVideoFinished(VideoPlayer vp)
    {
        if (isEnding) // ✅ ป้องกันเรียกซ้ำ
        {
            Debug.LogWarning("⚠️ Already ending, ignoring");
            return;
        }

        isEnding = true;
        Debug.Log($"Cutscene เล่นจบแล้ว กำลังโหลด Scene: {nextScene}");

        // ✅ ใช้ LoadingScreen แทน SceneManager
        if (LoadingScreen.Instance != null && !LoadingScreen.Instance.IsLoading)
        {
            StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}