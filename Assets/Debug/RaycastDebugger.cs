using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// ใส่สคริปต์นี้ใน GameObject ใดก็ได้ แล้วกด G เพื่อดีบั๊ก Raycast
/// </summary>
public class RaycastDebugger : MonoBehaviour
{
    private void Update()
    {
        // กด G เพื่อ debug raycast ที่ตำแหน่งเมาส์
        if (Input.GetKeyDown(KeyCode.G))
        {
            DebugRaycastAtMouse();
        }
        
        // กด H เพื่อแสดง Canvas ทั้งหมด
        if (Input.GetKeyDown(KeyCode.H))
        {
            ShowAllCanvases();
        }
        
        // กด J เพื่อแสดง GraphicRaycaster ทั้งหมด
        if (Input.GetKeyDown(KeyCode.J))
        {
            ShowAllRaycasters();
        }
    }
    
    private void DebugRaycastAtMouse()
    {
        Debug.Log("=== RAYCAST DEBUG (กด G) ===");
        
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        
        Debug.Log($"🎯 Mouse Position: {Input.mousePosition}");
        Debug.Log($"📊 พบ {results.Count} objects ที่ตำแหน่งเมาส์");
        
        if (results.Count == 0)
        {
            Debug.LogWarning("⚠️ ไม่พบ UI object ใดๆ ที่ตำแหน่งเมาส์!");
            Debug.LogWarning("   → เช็คว่า EventSystem มีหรือไม่");
            Debug.LogWarning("   → เช็คว่า Canvas มี GraphicRaycaster หรือไม่");
        }
        else
        {
            Debug.Log("📋 Objects ที่ตำแหน่งเมาส์ (เรียงตามลำดับ raycast):");
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                string info = $"   {i + 1}. [{result.gameObject.name}]";
                info += $" Layer: {LayerMask.LayerToName(result.gameObject.layer)}";
                info += $" | Canvas: {GetCanvasName(result.gameObject)}";
                info += $" | SortOrder: {result.sortingOrder}";
                info += $" | Distance: {result.distance}";
                
                if (i == 0)
                    Debug.Log($"✅ {info} ← ตัวนี้จะถูกคลิก!", result.gameObject);
                else
                    Debug.Log($"   {info} ← ถูกบังโดยตัวด้านบน", result.gameObject);
            }
        }
        
        Debug.Log("=== END RAYCAST DEBUG ===\n");
    }
    
    private void ShowAllCanvases()
    {
        Debug.Log("=== ALL CANVASES (กด H) ===");
        
        Canvas[] allCanvases = FindObjectsOfType<Canvas>(true); // รวมทั้ง inactive
        Debug.Log($"📊 พบ Canvas ทั้งหมด: {allCanvases.Length}");
        
        foreach (var canvas in allCanvases)
        {
            string status = canvas.gameObject.activeInHierarchy ? "✅ ACTIVE" : "❌ INACTIVE";
            string raycaster = canvas.GetComponent<GraphicRaycaster>() != null ? "มี Raycaster" : "⚠️ ไม่มี Raycaster";
            
            Debug.Log($"{status} [{canvas.name}] SortOrder: {canvas.sortingOrder} | {raycaster}", canvas);
            
            // แสดง CanvasGroup ถ้ามี
            var canvasGroup = canvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                Debug.Log($"   └─ CanvasGroup: Alpha={canvasGroup.alpha}, " +
                         $"Interactable={canvasGroup.interactable}, " +
                         $"BlocksRaycast={canvasGroup.blocksRaycasts}", canvasGroup);
            }
        }
        
        Debug.Log("=== END CANVASES ===\n");
    }
    
    private void ShowAllRaycasters()
    {
        Debug.Log("=== ALL GRAPHIC RAYCASTERS (กด J) ===");
        
        GraphicRaycaster[] raycasters = FindObjectsOfType<GraphicRaycaster>(true);
        Debug.Log($"📊 พบ GraphicRaycaster ทั้งหมด: {raycasters.Length}");
        
        foreach (var raycaster in raycasters)
        {
            string status = raycaster.enabled ? "✅ ENABLED" : "❌ DISABLED";
            string active = raycaster.gameObject.activeInHierarchy ? "✅ ACTIVE" : "❌ INACTIVE";
            
            var canvas = raycaster.GetComponent<Canvas>();
            int sortOrder = canvas != null ? canvas.sortingOrder : -999;
            
            Debug.Log($"{status} {active} [{raycaster.gameObject.name}] " +
                     $"SortOrder: {sortOrder}", raycaster);
        }
        
        Debug.Log("=== END RAYCASTERS ===\n");
    }
    
    private string GetCanvasName(GameObject obj)
    {
        Canvas canvas = obj.GetComponentInParent<Canvas>();
        return canvas != null ? canvas.name : "None";
    }
}
