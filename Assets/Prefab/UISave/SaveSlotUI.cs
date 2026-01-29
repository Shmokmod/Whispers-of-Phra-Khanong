using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot 1")]
    public GameObject newGame1;
    public GameObject continue1;
    public GameObject delete1;
    public Image preview1;
    public TMP_Text time1;

    [Header("Slot 2")]
    public GameObject newGame2;
    public GameObject continue2;
    public GameObject delete2;
    public Image preview2;
    public TMP_Text time2;

    [Header("Slot 3")]
    public GameObject newGame3;
    public GameObject continue3;
    public GameObject delete3;
    public Image preview3;
    public TMP_Text time3;

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        UpdateSlot(1, newGame1, continue1, delete1, preview1, time1);
        UpdateSlot(2, newGame2, continue2, delete2, preview2, time2);
        UpdateSlot(3, newGame3, continue3, delete3, preview3, time3);
    }

    void UpdateSlot(
        int slot,
        GameObject newBtn,
        GameObject contBtn,
        GameObject delBtn,
        Image preview,
        TMP_Text timeText
    )
    {
        bool hasSave = SaveManager.Instance.HasSave(slot);

        newBtn.SetActive(!hasSave);
        contBtn.SetActive(hasSave);
        delBtn.SetActive(hasSave);

        if (!hasSave)
        {
            preview.enabled = false;
            timeText.text = "--:--";
            return;
        }

        // โหลดรูป
        Sprite img = LoadSaveImage(slot);
        if (img != null)
        {
            preview.sprite = img;
            preview.enabled = true;
        }
        else
        {
            preview.enabled = false;
        }

        // โหลดเวลาเล่น
        float time = PlayerPrefs.GetFloat($"SAVE_PLAYTIME_{slot}", 0f);
        int min = Mathf.FloorToInt(time / 60f);
        int sec = Mathf.FloorToInt(time % 60f);
        timeText.text = $"{min:D2}:{sec:D2}";
    }

    // ===== BUTTON EVENTS =====
    public void NewGame(int slot)
    {
        PlayerPrefs.SetInt("CurrentSlot", slot);

        DetectiveBookManager.Instance.ClearAllProgress();
        DetectiveBookManager.Instance.InitAfterSlotSelected();

        // reset tutorial
        PlayerPrefs.DeleteKey("StartTutorialShown");
        PlayerPrefs.DeleteKey("TutorialShown");

        // reset hint (ถ้าใช้ key list)
        string listKey = "HINT_KEY_LIST";
        string list = PlayerPrefs.GetString(listKey, "");

        foreach (string key in list.Split('|'))
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith($"HINT_{slot}_"))
                PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.DeleteKey(listKey);
        PlayerPrefs.Save();

        SaveManager.Instance.NewGame(slot);
    }


    public void ContinueGame(int slot)
    {
        PlayerPrefs.SetInt("CurrentSlot", slot);

        DetectiveBookManager.Instance.InitAfterSlotSelected();
        SaveManager.Instance.ContinueGame(slot);
    }

    public void DeleteSlot(int slot)
    {
        SaveManager.Instance.DeleteSlot(slot);
        RefreshUI();
    }

    // ===== LOAD PREVIEW IMAGE =====
    Sprite LoadSaveImage(int slot)
    {
        string path = Application.persistentDataPath + $"/save_{slot}.png";
        if (!System.IO.File.Exists(path)) return null;

        byte[] data = System.IO.File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(data);

        return Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f)
        );
    }
}
