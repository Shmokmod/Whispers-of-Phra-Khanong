using UnityEngine;
using System.Collections;
public class SceneDialogueStarter : MonoBehaviour
{
    public NPC targetNPC;

    IEnumerator Start()
    {
        yield return null;
        targetNPC.ForceStartDialogue();
    }

}
