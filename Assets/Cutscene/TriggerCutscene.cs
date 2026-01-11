using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TriggerCutscene : MonoBehaviour
{
    public GameObject CutsceneImage;
    public CanvasGroup CutsceneCanvasGroup;
    public bool CutscenePlayed = false;
    public float FadeDuration = 1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CutscenePlayed = false;
    }

    // Update is called once per frame

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Playcutscene();
            // เรียกใช้งาน Cutscene Video Player หรือ Logic ที่เกี่ยวข้องที่นี่
        }
    }

    public void Playcutscene()
    {
        if (CutsceneCanvasGroup == null || CutsceneImage == null)
        {
            Debug.LogError("CutsceneCanvasGroup หรือ CutsceneImage ไม่ได้ถูกกำหนดค่า!");
            return;
        }

        CutscenePlayed = true;
        Debug.Log("Player เข้าสู่โซน Cutscene");
        CutsceneImage.gameObject.SetActive(true);
        CutsceneCanvasGroup.DOFade(1f, FadeDuration);
    }

    void Update()
    {
        
    }
}
