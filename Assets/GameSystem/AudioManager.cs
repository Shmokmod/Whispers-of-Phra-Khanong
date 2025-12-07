using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("BGM")]
    public AudioClip menuBGM;
    public AudioClip detectiveTheme;
    public AudioClip tensionTheme;

    [Header("SFX")]
    public AudioClip cardPickup;
    public AudioClip cardDrop;
    public AudioClip cardVerified;
    public AudioClip cardContradicted;
    public AudioClip correct;
    public AudioClip wrong;
    public AudioClip hint;
    public AudioClip phaseComplete;

    private AudioClip previousBGM;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayBGM(string bgmName)
    {
        previousBGM = bgmSource.clip;

        AudioClip clip = null;
        switch (bgmName)
        {
            case "menu": clip = menuBGM; break;
            case "detective_theme": clip = detectiveTheme; break;
            case "tension": clip = tensionTheme; break;
        }

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void ResumePreviousBGM()
    {
        if (previousBGM != null)
        {
            bgmSource.clip = previousBGM;
            bgmSource.Play();
        }
    }

    public void PlaySFX(string sfxName)
    {
        AudioClip clip = null;
        switch (sfxName)
        {
            case "card_pickup": clip = cardPickup; break;
            case "card_drop": clip = cardDrop; break;
            case "card_verified": clip = cardVerified; break;
            case "card_contradicted": clip = cardContradicted; break;
            case "correct": clip = correct; break;
            case "wrong": clip = wrong; break;
            case "hint": clip = hint; break;
            case "phase_complete": clip = phaseComplete; break;
        }

        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}