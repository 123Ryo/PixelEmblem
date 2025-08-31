using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    [Header("背景音樂設定")]
    public AudioClip backgroundMusic;
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;

    public bool loop = true;
    public bool dontDestroyOnLoad = true;

    private AudioSource audioSource;

    void Awake()
    {
        // 單例設計，避免多個實例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = loop;
        audioSource.playOnAwake = false;

        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlayMusic(AudioClip clip, bool shouldLoop = false)
    {
        if (clip == null) return;

        audioSource.clip = clip;
        audioSource.loop = shouldLoop;
        audioSource.Play();
    }

    public void PlayVictoryMusic()
    {
        PlayMusic(victoryMusic, false); // 勝利音樂不循環
    }

    public void PlayDefeatMusic()
    {
        PlayMusic(defeatMusic, false); // 失敗音樂不循環
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}
