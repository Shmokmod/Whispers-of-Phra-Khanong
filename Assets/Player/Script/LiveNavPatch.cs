using UnityEngine;

public class LiveNavPatch : MonoBehaviour
{
    public PathGuideController pathGuide;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pathGuide = GetComponent<PathGuideController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            pathGuide.TogglePath();
        }
    }





}
