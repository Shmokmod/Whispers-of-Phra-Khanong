using UnityEngine;

public class SpriteToQuad : MonoBehaviour
{
    public SpriteRenderer spriteRenderer; // ลาก SpriteRenderer ที่มี Animation มาใส่
    private Material quadMaterial;

    void Start()
    {
        // เอา Material จาก Quad (MeshRenderer)
        quadMaterial = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        // อัพเดท Texture ให้ตาม Sprite ปัจจุบัน
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            quadMaterial.mainTexture = spriteRenderer.sprite.texture;

            // ปรับ UV ให้ตรงกับ Sprite ใน Atlas
            Rect rect = spriteRenderer.sprite.textureRect;
            Texture2D tex = spriteRenderer.sprite.texture;

            quadMaterial.mainTextureScale = new Vector2(
                rect.width / tex.width,
                rect.height / tex.height
            );

            quadMaterial.mainTextureOffset = new Vector2(
                rect.x / tex.width,
                rect.y / tex.height
            );
        }
    }
}