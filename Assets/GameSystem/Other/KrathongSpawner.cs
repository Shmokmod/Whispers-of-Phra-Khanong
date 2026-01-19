using Unity.Cinemachine;
using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public Transform Spawnerlocation;
    public GameObject prefabToSpawn; // ลาก prefab มาใส่ใน Inspector
    public float minZ = -10f; // ค่า Z ต่ำสุด
    public float maxZ = 10f;  // ค่า Z สูงสุด
    public float spawnInterval = 2f; // spawn ทุกๆ 2 วินาที

    void Start()
    {
        // เริ่ม spawn หลังจาก 0 วินาที, ทุกๆ spawnInterval วินาที
        InvokeRepeating("SpawnRandomObject", 0f, spawnInterval);
    }

    void SpawnRandomObject()
    {
        // สุ่มค่า Z
        float randomZ = Random.Range(minZ, maxZ);
        Spawnerlocation.position = new Vector3(Spawnerlocation.position.x, Spawnerlocation.position.y, randomZ);

        // กำหนดตำแหน่ง (X และ Y คงที่, Z สุ่ม)
        Vector3 spawnPosition = Spawnerlocation.position;

        // สร้าง prefab
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
}