using UnityEngine;

public class TriggerChangeScene : MonoBehaviour
{
    public string sceneToLoad;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("มีอะไรชน Trigger: " + other.gameObject.name + " | Tag: " + other.tag);

        if (other.CompareTag("Player") && Time.time > 1f)
        {
            CutscenesChangeScene();
        }


    }

    private void CutscenesChangeScene()
    {
        StartCoroutine(LoadingScreen.Instance.LoadScene(sceneToLoad));
    }
}
