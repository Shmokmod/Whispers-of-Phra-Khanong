using UnityEngine;
using System.Collections;

public class soundFootSteps : MonoBehaviour
{
    public AudioSource Sound;
    private Coroutine footstepRoutine;

    void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
            Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if (footstepRoutine == null)
                footstepRoutine = StartCoroutine(FootStep());
        }
        else
        {
            if (footstepRoutine != null)
            {
                StopCoroutine(footstepRoutine);
                footstepRoutine = null;
            }
            Sound.Stop();
        }
    }

    IEnumerator FootStep()
    {
        while (true)
        {
            Sound.volume = Random.Range(0.2f, 0.4f);
            Sound.pitch = Random.Range(0.9f, 1.2f);
            Sound.Play();

            float delay = Input.GetKey(KeyCode.LeftShift) ? 0.4f : 0.5f;
            yield return new WaitForSeconds(delay);
        }
    }

}
