using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicFocus : MonoBehaviour
{
    [SerializeField] private Transform playerTarget; // ลาก Player มาใส่
    [SerializeField] private Transform focusPoint; // จุดที่ต้องการโฟกัส (ตัวละคร/ศีรษะ)
    [SerializeField] private float focusOffset = 0f;
    [SerializeField] private float minFocusDistance = 2f; // ระยะโฟกัสขั้นต่ำ

    private Volume volume;
    private DepthOfField depthOfField;

    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet<DepthOfField>(out depthOfField);

        // ถ้าไม่ได้กำหนด focusPoint ให้ใช้ playerTarget
        if (focusPoint == null && playerTarget != null)
        {
            focusPoint = playerTarget;
        }

        // ถ้าไม่มีทั้งคู่ ลองหาจาก Tag
        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                focusPoint = playerTarget;
            }
        }
    }

    void Update()
    {
        if (focusPoint != null)
        {
            // คำนวณระยะตามแนว forward ของกล้อง (Depth จากกล้อง)
            Vector3 directionToTarget = focusPoint.position - transform.position;
            float focusDepth = Vector3.Dot(directionToTarget, transform.forward);

            // จำกัดระยะขั้นต่ำ
            focusDepth = Mathf.Max(focusDepth, minFocusDistance);

            depthOfField.focusDistance.value = focusDepth + focusOffset;
        }
    }
}
