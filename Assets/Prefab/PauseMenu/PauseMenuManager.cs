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
        Transform playerTransform = GameObject.FindWithTag("Player").transform;
        Debug.Log($"[SAVE CALL] Player Pos = {PlayerPersistence.Instance.transform.position}");
        SaveManager.Instance.SaveGame(PlayerPersistence.Instance.transform);



    }



    public void BackMainMenu()
    {
        Debug.Log("🔥 BackMainMenu CLICKED");
        Time.timeScale = 1f;
        if (LoadingScreen.Instance == null)
        {
            Debug.LogError("❌ LoadingScreen.Instance == NULL");
            return;
        }

        Debug.Log("✅ LoadingScreen found");
        StartCoroutine(LoadingScreen.Instance.LoadScene("Mainmenu"));
        GamePauseUI.SetActive(false);
    }


    public void ApplicationQuit()
    {
        Application.Quit();
    }

}
