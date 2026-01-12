using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using JetBrains.Annotations;

public class TriggerCutscene : MonoBehaviour
{
    public GameObject CutsceneImage;
    public CanvasGroup CutsceneCanvasGroup;
    public bool CutscenePlayed = false;

    [Header("Fade Settings")]
    public float FadeInDuration = 1f;    // ค่อยๆ เฟดเข้า
    public float FadeOutDuration = 1f;   // ค่อยๆ เฟดออก

    [Header("Cutscene Duration")]
    public float CutsceneDuration = 5f;  // ⭐ ระยะเวลาที่แสดง Cutscene (วินาที)

    void Start()
    {
        CutscenePlayed = false;

        if (CutsceneCanvasGroup != null)
        {
            CutsceneCanvasGroup.alpha = 0f;
            CutsceneCanvasGroup.gameObject.SetActive(false);
        }

        if (CutsceneImage != null)
        {
            CutsceneImage.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("มีอะไรชน Trigger: " + other.gameObject.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player") && !CutscenePlayed)
        {
            Playcutscene();
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
        Debug.Log("✅ เล่น Cutscene แล้ว!");

        StartCoroutine(PlayCutsceneSequence());
    }


    private IEnumerator PlayCutsceneSequence()
    {
        // 🟢 หยุดเวลาตอนเริ่ม Cutscene
        Time.timeScale = 0f;

        // 1. Fade In (ใช้ SetUpdate(true) เพื่อไม่ให้โดน timeScale)
        CutsceneImage.SetActive(true);
        CutsceneCanvasGroup.gameObject.SetActive(true);
        CutsceneCanvasGroup.alpha = 0f;
        CutsceneCanvasGroup.DOFade(1f, FadeInDuration).SetUpdate(true); // ⭐ สำคัญ!

        yield return new WaitForSecondsRealtime(FadeInDuration); // ⭐ ใช้ Realtime!

        // 2. แสดง Cutscene
        yield return new WaitForSecondsRealtime(CutsceneDuration);

        // 3. Fade Out
        CutsceneCanvasGroup.DOFade(0f, FadeOutDuration).SetUpdate(true);
        yield return new WaitForSecondsRealtime(FadeOutDuration);

        CutsceneCanvasGroup.gameObject.SetActive(false);
        CutsceneImage.SetActive(false);

        // 🟢 คืนค่าเวลาปกติ
        Time.timeScale = 1f;
    }
}