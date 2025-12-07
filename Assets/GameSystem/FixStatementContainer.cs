using UnityEngine;

// ✅ ใส่ Script นี้ใน Statement Container
// กด Play → Right Click → "Fix Now!"

public class FixStatementContainer : MonoBehaviour
{
    [ContextMenu("Fix Now!")]
    void FixNow()
    {
        RectTransform rt = GetComponent<RectTransform>();

        Debug.Log("=== BEFORE FIX ===");
        Debug.Log($"Anchors: Min={rt.anchorMin}, Max={rt.anchorMax}");
        Debug.Log($"Size: {rt.rect.size}");

        // ✅ แก้ Anchors
        rt.anchorMin = new Vector2(0f, 0f);      // ซ้ายล่าง
        rt.anchorMax = new Vector2(0.5f, 1f);    // กึ่งกลางบน (ครึ่งซ้าย)
        rt.pivot = new Vector2(0.5f, 0.5f);

        // ✅ แก้ Offset
        rt.offsetMin = new Vector2(80, 120);     // Left, Bottom
        rt.offsetMax = new Vector2(-20, -120);   // Right, Top

        // ✅ ลบ Grid Layout Group
        var grid = GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(grid);
#else
            Destroy(grid);
#endif
            Debug.Log("✅ Removed Grid Layout Group");
        }

        // ✅ ลบ Content Size Fitter
        var fitter = GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (fitter != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(fitter);
#else
            Destroy(fitter);
#endif
            Debug.Log("✅ Removed Content Size Fitter");
        }

        // ✅ ลบ Layout Element
        var layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(layoutElement);
#else
            Destroy(layoutElement);
#endif
            Debug.Log("✅ Removed Layout Element");
        }

        Debug.Log("=== AFTER FIX ===");
        Debug.Log($"Anchors: Min={rt.anchorMin}, Max={rt.anchorMax}");
        Debug.Log($"Size: {rt.rect.size}");
        Debug.Log("✅✅✅ Statement Container FIXED! ✅✅✅");

        // ✅ Refresh การ์ดทั้งหมด
        if (DetectiveBoardManager.Instance != null)
        {
            Debug.Log("Refreshing card positions...");
            DetectiveBoardManager.Instance.RefreshCardPositions();
        }
    }

    void Start()
    {
        // ✅ Auto-fix เมื่อเริ่มเกม
        Invoke(nameof(FixNow), 0.1f);
    }
}

/*
=== วิธีใช้ ===

1. Copy script นี้ทั้งหมด
2. สร้างไฟล์ใหม่ใน Unity: FixStatementContainer.cs
3. Paste โค้ด
4. Add Component นี้ให้กับ Statement Container
5. กด Play
6. Script จะ Auto-fix ให้เลย!

หรือ:
- Right Click Component → "Fix Now!"

=== หลังจากแก้เสร็จ ===
- ลบ Component นี้ออกได้
- หรือปล่อยไว้ก็ได้ มันจะ Auto-fix ทุกครั้งที่เปิดเกม
*/