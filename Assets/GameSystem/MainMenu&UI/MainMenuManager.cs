using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Play")]
    public GameObject PlayMenu;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        //if (LoadingScreen.Instance != null)
        //{
        //    StartCoroutine(LoadingScreen.Instance.LoadScene("Cutscene_Start"));
        //}
        PlayMenu.SetActive(true);




        Debug.Log("Play button clicked, loading Cutscene_Start scene");
    }

    public void BackFromPlayMenu()
    {
        PlayMenu.SetActive(false);
    }


    public void Quit()
    {
        Application.Quit();
    }

}
