using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public static bool isPaused = false;

    void Update()
    {
        if (DialogueController.IsDialogueActive)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
            print(isPaused);
        }




        //if (SceneManager.GetActiveScene().name.Contains("Mainmenu"))
        //{
        //    isPaused = false;
        //    Time.timeScale = 1f;
        //    return;
        //}

    }





    public void Awake()
    {
        isPaused = false;
        Time.timeScale = 1f;

    }

    public void PauseGame()
    {
        if (SceneManager.GetActiveScene().name.Contains("Mainmenu"))
        {
            PauseController.isPaused = false;
            Time.timeScale = 1f;

            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);

            return;
        }

        PauseController.isPaused = !PauseController.isPaused;
        Time.timeScale = PauseController.isPaused ? 0f : 1f;

        pauseMenuUI.SetActive(PauseController.isPaused);
    }


    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // ปิด UI หยุดเกม
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}