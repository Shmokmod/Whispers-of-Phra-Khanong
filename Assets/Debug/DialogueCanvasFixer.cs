using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quick Fix: ใส่สคริปต์นี้ใน DialogueController GameObject
/// มันจะบังคับให้ Dialogue Canvas อยู่ด้านบนสุดและ Cutscene Canvas ถูกปิดอย่างถูกต้อง
/// </summary>
public class DialogueCanvasFixer : MonoBehaviour
{
    [Header("Assign These")]
    public Canvas dialogueCanvas;
    public Canvas cutsceneCanvas;
    
    [Header("Settings")]
    public int dialogueCanvasSortOrder = 100; // ควรสูงที่สุด
    public int cutsceneCanvasSortOrder = 50;
    
    private void Start()
    {
        FixCanvasSettings();
    }
    
    [ContextMenu("Fix Canvas Settings")]
    public void FixCanvasSettings()
    {
        Debug.Log("=== FIXING CANVAS SETTINGS ===");
        
        // Fix Dialogue Canvas
        if (dialogueCanvas != null)
        {
            dialogueCanvas.sortingOrder = dialogueCanvasSortOrder;
            dialogueCanvas.overrideSorting = true;
            
            var dialogueRaycaster = dialogueCanvas.GetComponent<GraphicRaycaster>();
            if (dialogueRaycaster != null)
            {
                dialogueRaycaster.enabled = true;
                Debug.Log($"✅ Dialogue Canvas: Sort Order = {dialogueCanvasSortOrder}, Raycaster ENABLED");
            }
            
            var dialogueGroup = dialogueCanvas.GetComponent<CanvasGroup>();
            if (dialogueGroup != null)
            {
                dialogueGroup.interactable = true;
                dialogueGroup.blocksRaycasts = true;
                Debug.Log($"✅ Dialogue CanvasGroup: interactable=true, blocksRaycasts=true");
            }
        }
        else
        {
            Debug.LogError("❌ Dialogue Canvas not assigned!");
        }
        
        // Fix Cutscene Canvas
        if (cutsceneCanvas != null)
        {
            cutsceneCanvas.sortingOrder = cutsceneCanvasSortOrder;
            cutsceneCanvas.overrideSorting = true;
            
            var cutsceneRaycaster = cutsceneCanvas.GetComponent<GraphicRaycaster>();
            if (cutsceneRaycaster != null)
            {
                cutsceneRaycaster.enabled = false; // DISABLE เพื่อไม่ให้บัง
                Debug.Log($"✅ Cutscene Canvas: Sort Order = {cutsceneCanvasSortOrder}, Raycaster DISABLED");
            }
            
            // ปิด Canvas ถ้ามันเปิดอยู่
            if (cutsceneCanvas.gameObject.activeInHierarchy)
            {
                cutsceneCanvas.gameObject.SetActive(false);
                Debug.Log($"✅ Cutscene Canvas: CLOSED");
            }
        }
        else
        {
            Debug.LogError("❌ Cutscene Canvas not assigned!");
        }
        
        Debug.Log("=== END FIX ===");
    }
    
    // เช็คทุก frame (เพื่อป้องกัน cutscene canvas บัง)
    private void LateUpdate()
    {
        if (cutsceneCanvas != null && cutsceneCanvas.gameObject.activeInHierarchy)
        {
            var raycaster = cutsceneCanvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null && raycaster.enabled)
            {
                raycaster.enabled = false;
                Debug.LogWarning("⚠️ Force disabled Cutscene Canvas Raycaster!");
            }
        }
    }
}
