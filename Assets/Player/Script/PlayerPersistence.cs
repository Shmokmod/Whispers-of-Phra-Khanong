using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;

    void Awake()
    {
        // ถ้ามี instance เก่าอยู่แล้ว -> ถ้าเป็นตัวเดิมให้เก็บไว้ ถ้าไม่ใช่ให้ทำลายตัวนี้
        if (instance == null)
        {
            instance = this;

            // แยก Player ออกจาก Parent (ถ้ามี) เพื่อให้แน่ใจว่าเป็น root GameObject
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // เรียก DontDestroyOnLoad กับ root object (ปลอดภัยกับ nested cases)
            GameObject root = transform.root.gameObject;
            DontDestroyOnLoad(root);
            Debug.Log("✓ Player ถูกตั้งค่า DontDestroyOnLoad (root) : " + root.name);
        }
        else
        {
            if (instance != this)
            {
                Debug.Log("✗ พบ Player ซ้ำ - ทำลายตัวซ้ำ");
                Destroy(gameObject);
            }
        }
    }
}
