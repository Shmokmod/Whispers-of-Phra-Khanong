using UnityEngine;

public class InteractHintTrigger : MonoBehaviour
{
    bool playerInRange;
    string prefKey;

    void Awake()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", 0);
        GameObject parent = transform.parent.gameObject;

        prefKey = $"HINT_{slot}_{parent.scene.name}_{parent.name}";
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

            string listKey = "HINT_KEY_LIST";
            string list = PlayerPrefs.GetString(listKey, "");

            if (!list.Contains(prefKey))
            {
                PlayerPrefs.SetString(listKey, list + "|" + prefKey);
            }

            PlayerPrefs.Save();


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
