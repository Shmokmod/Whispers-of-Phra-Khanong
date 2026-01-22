using UnityEngine;
using System.Collections;

public class PlayerLoader : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("🟢 Apply After Scene Ready");
        SaveManager.Instance.ApplyPlayerPosition(transform);
        Debug.Log($"[LOAD AFTER FRAME] Pos = {transform.position}");
    }
}

