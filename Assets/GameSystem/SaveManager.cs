using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    string SaveKey(int slot) => $"SAVE_SLOT_{slot}";
    string SceneKey(int slot) => $"SAVE_SCENE_{slot}";
    string PosKey(int slot, string axis) => $"SAVE_POS_{slot}_{axis}";
    string DialogueKey(int slot) => $"SAVE_DIALOGUE_{slot}"; // 🆕

    string ScreenshotPath(int slot) => Application.persistentDataPath + $"/save_{slot}.png";
    string PlayTimeKey(int slot) => $"SAVE_PLAYTIME_{slot}";


    public bool IsLoading { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        // 🆕 โหลด dialogue data ก่อนเปลี่ยน scene
        LoadDialogueData(slot);

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

        // 🆕 บันทึก dialogue data
        SaveDialogueData(slot);

        // 🆕 บันทึกเวลาเล่น (หน่วยวินาที)
        float oldTime = PlayerPrefs.GetFloat(PlayTimeKey(slot), 0f);
        float currentSessionTime = Time.timeSinceLevelLoad;

        PlayerPrefs.SetFloat(PlayTimeKey(slot), oldTime + currentSessionTime);

        // 🆕 แคปหน้าจอ
        StartCoroutine(CaptureScreenshot(slot));

        // บันทึกข้อมูลทันที
        PlayerPrefs.Save();
    }

    // 🆕 ฟังก์ชันบันทึก dialogue keys
    private void SaveDialogueData(int slot)
    {
        if (DetectiveBookManager.Instance == null)
        {
            Debug.LogWarning("[SAVE] DetectiveBookManager not found!");
            return;
        }

        var keys = DetectiveBookManager.Instance.reachedDialogueKeys;

        Debug.Log($"[SAVE] Dialogue keys count: {keys.Count}");
        foreach (var key in keys)
        {
            Debug.Log($"[SAVE]   - {key}");
        }

        string joined = string.Join("|", keys);
        PlayerPrefs.SetString(DialogueKey(slot), joined);

        Debug.Log($"[SAVE] ✅ Saved to key '{DialogueKey(slot)}': {joined}");
    }

    // 🆕 ฟังก์ชันโหลด dialogue keys
    private void LoadDialogueData(int slot)
    {
        string saved = PlayerPrefs.GetString(DialogueKey(slot), "");

        if (string.IsNullOrEmpty(saved))
        {
            Debug.Log("[LOAD] No dialogue data found");
            DetectiveBookManager.pendingDialogueKeys = null;
            return;
        }

        string[] keys = saved.Split('|');

        // เก็บไว้ใน static variable (DetectiveBookManager จะรับไปใน Awake)
        DetectiveBookManager.pendingDialogueKeys = new HashSet<string>(keys);

        Debug.Log($"[LOAD] ✅ Restored {keys.Length} dialogue keys:");
        foreach (var key in keys)
        {
            Debug.Log($"[LOAD]   - {key}");
        }
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
        Rigidbody rb = player.GetComponent<Rigidbody>();

        if (controller != null)
        {
            controller.enabled = false;
            Debug.Log("🔒 Disabled CharacterController for teleport");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        player.position = savedPos;
        Physics.SyncTransforms();

        if (controller != null)
        {
            controller.enabled = true;
            Debug.Log("🔓 Enabled CharacterController");
        }

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
        PlayerPrefs.DeleteKey(DialogueKey(slot)); // 🆕
        PlayerPrefs.Save();
    }

    private IEnumerator CaptureScreenshot(int slot)
    {
        // 🔴 ปิด UI ทั้งหมด
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in canvases)
            c.enabled = false;

        yield return new WaitForEndOfFrame();

        Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] png = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(ScreenshotPath(slot), png);
        Destroy(tex);

        // 🟢 เปิด UI กลับ
        foreach (var c in canvases)
            c.enabled = true;

        Debug.Log("[SAVE] Screenshot without UI");
    }


}