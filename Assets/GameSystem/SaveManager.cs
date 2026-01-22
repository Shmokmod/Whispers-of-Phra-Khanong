using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    string SaveKey(int slot) => $"SAVE_SLOT_{slot}";
    string SceneKey(int slot) => $"SAVE_SCENE_{slot}";
    string PosKey(int slot, string axis) => $"SAVE_POS_{slot}_{axis}";

 

    public bool IsLoading { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void FinishLoading()
    {
        IsLoading = false;
    }


    // ===== CHECK =====
    public bool HasSave(int slot)
    {
        return PlayerPrefs.HasKey(SaveKey(slot));
    }

    // ===== NEW GAME =====
    public void NewGame(int slot)
    {
        DeleteSlot(slot);
        PlayerPrefs.SetInt("CurrentSlot", slot);
        PlayerPrefs.DeleteKey($"DetectiveBook_UnlockedNotes_{slot}");
        StartCoroutine(LoadingScreen.Instance.LoadScene("Cutscene_Start"));
    }

    // ===== CONTINUE =====
    public void ContinueGame(int slot)
    {
        if (!HasSave(slot)) return;

        IsLoading = true;

        PlayerPrefs.SetInt("CurrentSlot", slot);
        string scene = PlayerPrefs.GetString(SceneKey(slot), "GameScene");
        StartCoroutine(LoadingScreen.Instance.LoadScene(scene));
    }

    // ===== SAVE =====
    public void SaveGame(Transform player)
    {
        Debug.Log($"[SAVE] Pos = {player.position}");

        int slot = PlayerPrefs.GetInt("CurrentSlot");

        PlayerPrefs.SetInt(SaveKey(slot), 1);
        PlayerPrefs.SetString(SceneKey(slot), SceneManager.GetActiveScene().name);

        PlayerPrefs.SetFloat(PosKey(slot, "X"), player.position.x);
        PlayerPrefs.SetFloat(PosKey(slot, "Y"), player.position.y);
        PlayerPrefs.SetFloat(PosKey(slot, "Z"), player.position.z);

        PlayerPrefs.Save();
    }


    // ===== LOAD PLAYER =====
    public void ApplyPlayerPosition(Transform player)
    {
        int slot = PlayerPrefs.GetInt("CurrentSlot", -1);
        if (slot == -1) return;

        float x = PlayerPrefs.GetFloat(PosKey(slot, "X"), player.position.x);
        float y = PlayerPrefs.GetFloat(PosKey(slot, "Y"), player.position.y);
        float z = PlayerPrefs.GetFloat(PosKey(slot, "Z"), player.position.z);

        Vector3 savedPos = new Vector3(x, y, z);

        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        player.position = savedPos;

        if (controller != null) controller.enabled = true;

        

        Debug.Log($"[LOAD APPLY] Pos = {player.position}");
    }



    // ===== DELETE =====
    public void DeleteSlot(int slot)
    {
        PlayerPrefs.DeleteKey(SaveKey(slot));
        PlayerPrefs.DeleteKey(SceneKey(slot));
        PlayerPrefs.DeleteKey(PosKey(slot, "X"));
        PlayerPrefs.DeleteKey(PosKey(slot, "Y"));
        PlayerPrefs.DeleteKey(PosKey(slot, "Z"));
        PlayerPrefs.Save();
    }
}
