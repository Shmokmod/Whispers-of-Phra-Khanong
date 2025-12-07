using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "NextScene";

    [Header("Spawn Point Settings")]
    [Tooltip("ชื่อของ Spawn Point ที่จะไปหาใน Scene ปลายทาง")]
    public string targetSpawnPointName = "SpawnPoint_FromA";    

    [Header("Interaction Settings")]
    public KeyCode interactKey = KeyCode.E;
    public GameObject interactionIcon;

    private bool isPlayerInside = false;

    void Start()
    {
        if (interactionIcon != null)
        {
            interactionIcon.SetActive(false);
        }
    }

    void Update()
    {
        if (isPlayerInside && Input.GetKeyDown(interactKey))
        {
            LoadScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;

            if (interactionIcon != null)
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    void LoadScene()
    {
        // บันทึกชื่อ spawn point ที่ต้องการไป
        SpawnManager.targetSpawnPointName = targetSpawnPointName;

        // โหลด Scene
        SceneManager.LoadScene(sceneToLoad);
    }
}