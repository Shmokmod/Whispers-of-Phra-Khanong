using UnityEngine;

public class InteractHintTrigger : MonoBehaviour
{
    private bool playerInRange;
    private string prefKey;

    void Awake()
    {
        // ใช้ Parent เป็นตัวอ้างอิงถาวร
        GameObject parent = transform.parent.gameObject;
        prefKey = "HINT_USED_" + parent.scene.name + "_" + parent.name;
    }

    void Start()
    {
        if (PlayerPrefs.GetInt(prefKey, 0) == 1)
        {
            gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.SetInt(prefKey, 1);
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
