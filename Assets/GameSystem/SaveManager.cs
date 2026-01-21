using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    string SaveKey(int slot) => $"SAVE_SLOT_{slot}";

    public void NewGame(int slot)
    {
        PlayerPrefs.DeleteKey(SaveKey(slot));
        PlayerPrefs.SetInt("CurrentSlot", slot);
        SceneManager.LoadScene("GameScene");
    }

    public void ContinueGame(int slot)
    {
        if (!PlayerPrefs.HasKey(SaveKey(slot))) return;

        PlayerPrefs.SetInt("CurrentSlot", slot);
        SceneManager.LoadScene("GameScene");
    }

    public void SaveGame(int data)
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot");
        PlayerPrefs.SetInt(SaveKey(slot), data);
        PlayerPrefs.Save();
    }

    public int LoadGame()
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot");
        return PlayerPrefs.GetInt(SaveKey(slot), 0);
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("Level1");
    }




}
