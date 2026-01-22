using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public static bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
                print(isPaused);
        }
    }

    public void PauseGame()
    {
        if (SceneManager.GetActiveScene().name == "Mainmenu")
        {
            pauseMenuUI.SetActive(false);
            return;
        }

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
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