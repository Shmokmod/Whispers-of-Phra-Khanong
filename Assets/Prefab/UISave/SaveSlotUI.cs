using UnityEngine;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot 1")]
    public GameObject newGame1;
    public GameObject continue1;
    public GameObject delete1;

    [Header("Slot 2")]
    public GameObject newGame2;
    public GameObject continue2;
    public GameObject delete2;

    [Header("Slot 3")]
    public GameObject newGame3;
    public GameObject continue3;
    public GameObject delete3;

    void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        UpdateSlot(1, newGame1, continue1, delete1);
        UpdateSlot(2, newGame2, continue2, delete2);
        UpdateSlot(3, newGame3, continue3, delete3);
    }

    void UpdateSlot(int slot, GameObject newBtn, GameObject contBtn, GameObject delBtn)
    {
        bool hasSave = SaveManager.Instance.HasSave(slot);

        newBtn.SetActive(!hasSave);
        contBtn.SetActive(hasSave);
        delBtn.SetActive(hasSave);
    }

    // ===== BUTTON EVENTS =====
    public void NewGame(int slot)
    {
        SaveManager.Instance.NewGame(slot);
    }

    public void ContinueGame(int slot)
    {
        SaveManager.Instance.ContinueGame(slot);
    }

    public void DeleteSlot(int slot)
    {
        SaveManager.Instance.DeleteSlot(slot);
        RefreshUI();
    }
}
