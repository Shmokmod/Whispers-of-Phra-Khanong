using UnityEngine;

public static class PostLoadReset
{
    public static void ResetAll()
    {
        Time.timeScale = 1f;
        PauseController.isPaused = false;

        if (DialogueController.instance != null)
        {
            DialogueController.instance.ShowDialogue(false);
            DialogueController.instance.ClearChoices();
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.FinishLoading();

    }
}
