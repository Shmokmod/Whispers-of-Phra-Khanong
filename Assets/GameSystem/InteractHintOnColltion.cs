using UnityEngine;

public class InteractHintOnCokktion : MonoBehaviour
{
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

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        SaveAndHide();
    }

    void SaveAndHide()
    {
        PlayerPrefs.SetInt(prefKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}
