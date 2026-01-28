using UnityEngine;

public class Tutorial_Trigger : MonoBehaviour
{
    public GameObject TutorialUI;
    private const string TutorialKey = "TutorialShown";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (PlayerPrefs.GetInt(TutorialKey, 0) == 0)
        {
            ShowTutorial();
            PlayerPrefs.SetInt(TutorialKey, 1);
            PlayerPrefs.Save();
        }
    }

    private void ShowTutorial()
    {
        TutorialUI.SetActive(true);
        Time.timeScale = 0f; // Pause the game
    }

    public void CloseTutorial()
    {
        TutorialUI.SetActive(false);
        Time.timeScale = 1f; // Resume the game
    }
}
