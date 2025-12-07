using UnityEngine;
using UnityEngine.InputSystem;

public class InteractorDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; // The interactable object currently in range
    public GameObject interactionIcon; // UI icon to show when an interactable is in range

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    //public void OnInteract()//(InputAction.CallbackContext context)
    //{
    //    if (Input.GetButtonDown("Jump") && interactableInRange != null && interactableInRange.CanInteract()) //(context.performed && interactableInRange != null && interactableInRange.CanInteract())
    //    {
    //        interactableInRange.Interact()
    //        print("Press Button");

    //    }
    //}

    void Update()
    {
        if (Input.GetButtonDown("Jump") && interactableInRange != null && interactableInRange.CanInteract())
        {
            interactableInRange.Interact();
            Debug.Log("Press Button");
            interactionIcon.SetActive(false);
        }
    }


    private void OnTriggerEnter(Collider collision)
    {
        if(collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
            print("InteractDetect In Range");
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }




}
