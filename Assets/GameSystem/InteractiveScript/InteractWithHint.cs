using GameSystem;
using UnityEngine;

public class InteractWithHint : MonoBehaviour, IInteractable
{
    public bool isOpened { get; private set; }
    public string hintID { get; private set; }
    public GameObject itemPrefab;
    public Sprite openSpriteHint;
    public GameObject GotItemUI;

    [Header("Evidence to Unlock")]
    public string[] evidenceIDsToUnlock; // ✅ เพิ่มตรงนี้

    void Start()
    {
        hintID ??= GoableHelper.GenerateUniqueID(gameObject);
    }

    public bool CanInteract()
    {
        return !isOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;
        OpenHint();
    }

    private void OpenHint()
    {
        SetOpened(true);

        // ✅ Unlock evidence ที่กำหนด
        UnlockEvidence();

        if (itemPrefab)
        {
            print("Dropped hint item");
            GotItemUI.SetActive(true);
            PauseController.isPaused = true;
        }
    }

    void UnlockEvidence()
    {
        if (evidenceIDsToUnlock == null || evidenceIDsToUnlock.Length == 0)
        {
            Debug.LogWarning("❌ ไม่มี Evidence ID ที่ต้อง unlock");
            return;
        }

        foreach (string evidID in evidenceIDsToUnlock)
        {
            EvidenceData evid = GameDataRuntime.Instance.GetEvidence(evidID);
            if (evid != null)
            {
                evid.isUnlocked = true; // ✅ Unlock
                GameDataRuntime.Instance.SaveUnlockedEvidence(evidID);
                Debug.Log($"✅ Evidence Unlocked: {evidID}");
            }
            else
            {
                Debug.LogWarning($"❌ Evidence ไม่พบ: {evidID}");
            }
        }
    }

    public void SetOpened(bool opened)
    {
        isOpened = opened;
        if (isOpened == opened)
        {
            //GetComponent<SpriteRenderer>().sprite = openSpriteHint;
        }
    }

    public void CloseGotitemUI()
    {
        GotItemUI.SetActive(false);
        PauseController.isPaused = false;
    }
}