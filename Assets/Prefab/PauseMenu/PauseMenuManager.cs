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
        Pause.ResumeGame();   // 🔴 สำคัญ

        Time.timeScale = 1f;
        StartCoroutine(LoadingScreen.Instance.LoadScene("Mainmenu"));
    }



    public void ApplicationQuit()
    {
        Application.Quit();
    }

}
