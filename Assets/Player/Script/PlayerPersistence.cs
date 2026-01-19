using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;

    void Awake()
    {
        // ถ้ามี instance เก่าอยู่แล้ว
        if (instance == null)
        {
            instance = this;

            // แยก Player ออกจาก Parent (ถ้ามี)
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            // เรียก DontDestroyOnLoad กับ root object
            GameObject root = transform.root.gameObject;
            DontDestroyOnLoad(root);

            Debug.Log($"✅ [PlayerPersistence] Player set as DontDestroyOnLoad: {root.name}");

            // ตรวจสอบว่ามี Tag "Player" หรือไม่
            if (!gameObject.CompareTag("Player"))
            {
                Debug.LogWarning("⚠️ [PlayerPersistence] Player GameObject doesn't have 'Player' tag!");
            }
        }
        else
        {
            if (instance != this)
            {
                Debug.Log($"❌ [PlayerPersistence] Duplicate Player found - destroying: {gameObject.name}");
                Destroy(gameObject);
            }
        }
    }

    void OnDestroy()
    {
        // ถ้าเป็น instance หลักที่ถูกทำลาย ให้ clear reference
        if (instance == this)
        {
            instance = null;
            Debug.Log("[PlayerPersistence] Main instance destroyed");
        }
    }
}