using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class InteractorDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    [Header("Detector Settings")]
    public float detectRadius = 1.0f;     // ใช้เช็กตอนวาป

    void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (interactionIcon != null)
            interactionIcon.SetActive(false);
        else
            Debug.LogWarning("[InteractorDetector] interactionIcon ยังไม่ได้เซ็ต!");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this == null || gameObject == null)
            return;

        interactableInRange = null;

        if (interactionIcon != null)
            interactionIcon.SetActive(false);

        StartCoroutine(RecheckInteractableAfterWarp());
    }

    IEnumerator RecheckInteractableAfterWarp()
    {
        // รอให้ Player ถูกวางเรียบร้อย
        yield return null;
        yield return null;

        Physics.SyncTransforms();

        // เช็กว่ามี interactable อยู่รอบตัวมั้ย (กรณี spawn ใน trigger)
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRadius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                if (interactable.CanInteract())
                {
                    interactableInRange = interactable;
                    if (interactionIcon != null)
                        interactionIcon.SetActive(true);

                    Debug.Log("[InteractorDetector] Auto-detected interactable after warp: " + hit.name);
                    yield break;
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Jump") && interactableInRange != null)
        {
            if (interactableInRange.CanInteract())
            {
                interactableInRange.Interact();
                if (interactionIcon != null)
                    interactionIcon.SetActive(false);
            }
            else
            {
                interactableInRange = null;
                if (interactionIcon != null)
                    interactionIcon.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision == null) return;
        if (collision.gameObject == gameObject) return;
        if (collision.CompareTag("Player")) return;
        if (collision.transform.IsChildOf(transform)) return;

        if (collision.TryGetComponent(out IInteractable interactable))
        {
            if (interactable.CanInteract())
            {
                interactableInRange = interactable;
                if (interactionIcon != null)
                    interactionIcon.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision == null) return;

        if (collision.TryGetComponent(out IInteractable interactable) &&
            interactable == interactableInRange)
        {
            interactableInRange = null;
            if (interactionIcon != null)
                interactionIcon.SetActive(false);
        }
    }
}
