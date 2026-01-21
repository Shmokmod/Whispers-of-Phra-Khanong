using UnityEngine;
using UnityEngine.Video;

public class CutsceneEnd : MonoBehaviour
{
    public string nextScene = "GameScene";
    private bool isEnding = false;

    void Start()
    {
        VideoPlayer vp = GetComponent<VideoPlayer>();
        vp.loopPointReached += OnVideoEnd;
    }

    void Update()
    {
        if (isEnding) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Skip cutscene and load next scene");
            EndCutscene();
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        Debug.Log("Cutscene ended, loading next scene");
        EndCutscene();
    }

    void EndCutscene()
    {
        if (isEnding) return;
        isEnding = true;

        StartCoroutine(LoadingScreen.Instance.LoadScene(nextScene));
    }
}
