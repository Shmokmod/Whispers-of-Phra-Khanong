using UnityEngine;

public class ShowPatchFollowWarpZone : MonoBehaviour
{
    [Header("References")]
    [Tooltip("GameObject ที่มี PathGuideController ติดอยู่")]
    public GameObject WarpZomeObject;

    [Header("Optional - Direct Reference")]
    [Tooltip("หรือลาก PathGuideController มาตรง ๆ ก็ได้")]
    public PathGuideController pathGuide;

    private void Start()
    {
        // ถ้าไม่ได้กำหนด pathGuide แต่มี WarpZomeObject
        // ให้ลอง GetComponent อัตโนมัติ
        if (pathGuide == null && WarpZomeObject != null)
        {
            pathGuide = WarpZomeObject.GetComponent<PathGuideController>();
            
            if (pathGuide == null)
            {
                Debug.LogError("❌ PathGuideController not found on WarpZomeObject!");
            }
        }
    }

    /// <summary>
    /// แสดง path ไปยัง Warp Zone
    /// </summary>
    public void ShowPatchFollowWarp()
    {
        Debug.Log("🔍 ShowPatchFollowWarp() ถูกเรียกแล้ว!");
        Debug.Log($"📌 pathGuide = {(pathGuide != null ? "มี" : "NULL")}");
        Debug.Log($"📌 WarpZomeObject = {(WarpZomeObject != null ? "มี" : "NULL")}");
        
        // วิธีที่ 1: ใช้ pathGuide ที่กำหนดไว้
        if (pathGuide != null)
        {
            pathGuide.TogglePath(); // เปิด/ปิด path
            Debug.Log("✅ Showing path to warp zone");
        }
        // วิธีที่ 2: ลอง GetComponent จาก WarpZomeObject
        else if (WarpZomeObject != null)
        {
            PathGuideController controller = WarpZomeObject.GetComponent<PathGuideController>();
            
            if (controller != null)
            {
                controller.TogglePath();
                Debug.Log("✅ Showing path to warp zone (via GetComponent)");
            }
            else
            {
                Debug.LogError("❌ PathGuideController not found on WarpZomeObject!");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ WarpZomeObject is not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// แค่แสดง path ครั้งเดียว (ไม่ toggle)
    /// </summary>
    public void ShowPathOnce()
    {
        if (pathGuide != null)
        {
            pathGuide.ShowPath();
        }
        else if (WarpZomeObject != null)
        {
            PathGuideController controller = WarpZomeObject.GetComponent<PathGuideController>();
            controller?.ShowPath();
        }
    }

    /// <summary>
    /// ปิด path
    /// </summary>
    public void HidePath()
    {
        if (pathGuide != null)
        {
            // ถ้า path กำลังแสดงอยู่ ให้ toggle ปิด
            if (pathGuide.GetComponent<PathGuideController>() != null)
            {
                pathGuide.TogglePath();
            }
        }
    }
}