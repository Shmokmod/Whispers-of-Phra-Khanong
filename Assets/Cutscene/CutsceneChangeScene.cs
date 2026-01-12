using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class CutsceneChangeScene : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (videoPlayer != null)
        {
            // สมัครรับฟังเหตุการณ์เมื่อวิดีโอเล่นจบ
            Debug.Log("สมัครรับฟังเหตุการณ์ videoPlayer.loopPointReached");
            videoPlayer.loopPointReached += OnVideoFinished;
            // เริ่มเล่นวิดีโอ (ถ้ายังไม่ได้เล่น)
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
        Debug.Log("Cutscene เล่นจบแล้ว กำลังโหลด Scene: level3");
        SceneManager.LoadScene("level3");
    }

    void OnDestroy()
    {
        // Unsubscribe เมื่อ Object ถูกทำลาย
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
            
        }
    }
}