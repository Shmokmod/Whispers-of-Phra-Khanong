using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject GamePauseUI;
    public PauseController Pause;

    public void Resume()
    {
        Pause.ResumeGame();
    }

    public void Save()
    {
        // 🆕 หา Player โดยตรงแทนการใช้ Instance
        GameObject playerObject = GameObject.FindWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogError("❌ Cannot find Player to save!");
            return;
        }

        Transform playerTransform = playerObject.transform;

        Debug.Log($"[SAVE CALL] Player Pos = {playerTransform.position}");

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame(playerTransform);
            Debug.Log("✅ Game saved successfully!");
        }
        else
        {
            Debug.LogError("❌ SaveManager.Instance is null!");
        }
    }

    public void BackMainMenu()
    {
        Pause.ResumeGame();
        Time.timeScale = 1f;
        StartCoroutine(LoadingScreen.Instance.LoadScene("Mainmenu"));
    }

    public void ApplicationQuit()
    {
        Application.Quit();
    }
}