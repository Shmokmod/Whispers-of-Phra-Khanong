using UnityEngine;

public class Tutorial_StartScene : MonoBehaviour
{
    public GameObject TutorialUI;
    public GameObject TutorialUI2;
    private const string StartTutorialKey = "StartTutorialShown";

    void Start()
    {
        if (PlayerPrefs.GetInt(StartTutorialKey, 0) == 0)
        {
            ShowTutorial();
            PlayerPrefs.SetInt(StartTutorialKey, 1);
            PlayerPrefs.Save();
        }
    }

    void ShowTutorial()
    {
        TutorialUI.SetActive(true);
    }

    public void ShowTutorial2()
    {
        TutorialUI.SetActive(false);
        TutorialUI2.SetActive(true);
    }

    public void CloseTutorial()
    {
        TutorialUI2.SetActive(false);
    }
}
