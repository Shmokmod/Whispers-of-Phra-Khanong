using UnityEngine;

public class TriggerFog : MonoBehaviour
{
    public GameObject Fog;
    public float fadeDuration = 1f;

    private Material fogMat;
    private Color fogVisible;     // หมอกทึบ
    private Color fogInvisible;   // หมอกจาง
    private Color startColor;
    private Color targetColor;

    private float fadeTimer;
    private bool isFading;

    void Start()
    {
        fogMat = Fog.GetComponent<MeshRenderer>().material;

        fogVisible = fogMat.color;
        fogInvisible = fogVisible;
        fogInvisible.a = 0f;

        targetColor = fogVisible;
        isFading = false;
    }

    void Update()
    {
        if (!isFading) return;

        fadeTimer += Time.deltaTime;
        float t = fadeTimer / fadeDuration;

        fogMat.color = Color.Lerp(startColor, targetColor, t);

        if (t >= 1f)
        {
            fogMat.color = targetColor;
            isFading = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(fogInvisible); // Fade Out
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartFade(fogVisible); // Fade In
        }
    }

    void StartFade(Color newTarget)
    {
        startColor = fogMat.color;
        targetColor = newTarget;
        fadeTimer = 0f;
        isFading = true;
    }
}
