using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Transform NewTarget;
    public PathGuideController patchguild;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void navPatchchangeTraget()
    {
        patchguild.target = NewTarget;
        if (NewTarget = null)
        {
            Debug.Log("ไม่พบ GameObject ชื่อ NewTarget");
        }
    }


}
