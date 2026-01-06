using UnityEngine;

public class TriggerFog : MonoBehaviour
{
    public GameObject Fog;
    public float timeToFade = 1f;

    private Material fogMat;
    private Color fogVisible;   // หมอกปกติ
    private Color fogInvisible; // หมอกจาง
    private Color targetColor;

    void Start()
    {
        fogMat = Fog.GetComponent<MeshRenderer>().material;

        fogVisible = fogMat.color;           // alpha เดิม (เช่น 1)
        fogInvisible = fogVisible;
        fogInvisible.a = 0f;                 // จางหาย

        targetColor = fogVisible;             // ค่าเริ่มต้น
    }

    void Update()
    {
        fogMat.color = Color.Lerp(
            fogMat.color,
            targetColor,
            timeToFade * Time.deltaTime
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetColor = fogInvisible; // Fade Out
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetColor = fogVisible; // Fade In
        }
    }
}
