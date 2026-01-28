using UnityEngine;

public class CreditOnclick : MonoBehaviour
{
    public string sceneToLoad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }





    public void OnClick()
    {
        StartCoroutine(LoadingScreen.Instance.LoadScene(sceneToLoad));
    }
}
