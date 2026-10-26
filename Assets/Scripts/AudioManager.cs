using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("AudioManager");
                    if (prefab != null)
                    {
                        GameObject go = Instantiate(prefab);
                        go.name = "AudioManager";
                        _instance = go.GetComponent<AudioManager>();
                    }
                    else
                    {
                        GameObject go = new GameObject("AudioManager");
                        _instance = go.AddComponent<AudioManager>();
                    }
                }
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        if (_instance == null)
        {
            var inst = Instance;
            if (inst != null) DontDestroyOnLoad(inst.gameObject);
        }
    }

    [Header("Master Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    [Header("BGM Volume Controls")]
    [Range(0f, 1f)] public float titleMusicVolume = 0.8f;
    [Range(0f, 1f)] public float gameplayMusicVolume = 0.6f;

    [Header("SFX Individual Volume Controls")]
    [Range(0f, 1f)] public float footstepVolume = 0.65f;
    [Range(0f, 1f)] public float waitVolume = 0.65f;
    [Range(0f, 1f)] public float buttonClickVolume = 0.85f;
    [Range(0f, 1f)] public float doorOpenVolume = 0.85f;
    [Range(0f, 1f)] public float doorCloseVolume = 0.85f;
    [Range(0f, 1f)] public float pressurePlateVolume = 1.0f;
    [Range(0f, 1f)] public float chestOpenVolume = 1.0f;
    [Range(0f, 1f)] public float swordSwingVolume = 0.9f;
    [Range(0f, 1f)] public float itemCollectVolume = 1.0f;
    [Range(0f, 1f)] public float victoryVolume = 1.0f;
    [Range(0f, 1f)] public float gameOverVolume = 1.0f;
    [Range(0f, 1f)] public float transitionVolume = 0.9f;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips (Drag custom audio here to swap)")]
    public AudioClip bgmTitle;
    public AudioClip bgmGameplay;

    [Header("SFX Clips (Drag custom audio here to swap)")]
    public AudioClip sfxButtonClick;
    public AudioClip sfxChestOpen;
    public AudioClip sfxDoorClose;
    public AudioClip sfxDoorOpen;
    public AudioClip sfxFootstep;
    public AudioClip sfxWait;
    public AudioClip sfxGameOver;
    public AudioClip sfxItemCollect;
    public AudioClip sfxPressurePlate;
    public AudioClip sfxSwordSwing;
    public AudioClip sfxVictory;
    public AudioClip sfxVictoryFinal;
    public AudioClip sfxTransition1;
    public AudioClip sfxTransition2;

    private AudioListener fallbackListener;
    public enum BgmType { None, Title, Gameplay }
    private BgmType currentBgmType = BgmType.None;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            EnsureAudioListener();
            LoadClipsIfNull();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        HandleSceneMusic(SceneManager.GetActiveScene().name);
        HookButtonsInScene();
    }

    public void ApplyLiveVolume()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            float targetVolume = (currentBgmType == BgmType.Title ? titleMusicVolume : gameplayMusicVolume) * masterVolume;
            bgmSource.volume = targetVolume;
        }
    }

    private void Update()
    {
        ApplyLiveVolume();
    }

    private void OnValidate()
    {
        // 1. If user moves sliders on the Prefab in Project window while game is playing:
        if (Application.isPlaying && _instance != null && _instance != this)
        {
            _instance.masterVolume = this.masterVolume;
            _instance.titleMusicVolume = this.titleMusicVolume;
            _instance.gameplayMusicVolume = this.gameplayMusicVolume;
            _instance.footstepVolume = this.footstepVolume;
            _instance.waitVolume = this.waitVolume;
            _instance.buttonClickVolume = this.buttonClickVolume;
            _instance.doorOpenVolume = this.doorOpenVolume;
            _instance.doorCloseVolume = this.doorCloseVolume;
            _instance.pressurePlateVolume = this.pressurePlateVolume;
            _instance.chestOpenVolume = this.chestOpenVolume;
            _instance.swordSwingVolume = this.swordSwingVolume;
            _instance.itemCollectVolume = this.itemCollectVolume;
            _instance.victoryVolume = this.victoryVolume;
            _instance.gameOverVolume = this.gameOverVolume;
            _instance.transitionVolume = this.transitionVolume;
            _instance.ApplyLiveVolume();
        }
        // 2. If user moves sliders on the live GameObject in Hierarchy while playing:
        else if (Application.isPlaying && this == _instance)
        {
            ApplyLiveVolume();
#if UNITY_EDITOR
            SyncToPrefabAsset();
#endif
        }
    }

#if UNITY_EDITOR
    private void SyncToPrefabAsset()
    {
        GameObject prefab = Resources.Load<GameObject>("AudioManager");
        if (prefab != null)
        {
            AudioManager am = prefab.GetComponent<AudioManager>();
            if (am != null)
            {
                am.masterVolume = this.masterVolume;
                am.titleMusicVolume = this.titleMusicVolume;
                am.gameplayMusicVolume = this.gameplayMusicVolume;
                am.footstepVolume = this.footstepVolume;
                am.waitVolume = this.waitVolume;
                am.buttonClickVolume = this.buttonClickVolume;
                am.doorOpenVolume = this.doorOpenVolume;
                am.doorCloseVolume = this.doorCloseVolume;
                am.pressurePlateVolume = this.pressurePlateVolume;
                am.chestOpenVolume = this.chestOpenVolume;
                am.swordSwingVolume = this.swordSwingVolume;
                am.itemCollectVolume = this.itemCollectVolume;
                am.victoryVolume = this.victoryVolume;
                am.gameOverVolume = this.gameOverVolume;
                am.transitionVolume = this.transitionVolume;
                EditorUtility.SetDirty(prefab);
            }
        }
    }
#endif

    private void EnsureAudioListener()
    {
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners == null || listeners.Length == 0)
        {
            if (fallbackListener == null)
            {
                fallbackListener = gameObject.AddComponent<AudioListener>();
            }
            fallbackListener.enabled = true;
        }
        else if (listeners.Length > 1 && fallbackListener != null)
        {
            fallbackListener.enabled = false;
        }
    }

    private void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    private void LoadClipsIfNull()
    {
        if (bgmTitle == null) bgmTitle = Resources.Load<AudioClip>("Audio/BGM/BGM_Title");
        if (bgmGameplay == null) bgmGameplay = Resources.Load<AudioClip>("Audio/BGM/BGM_Gameplay");

        if (sfxButtonClick == null) sfxButtonClick = Resources.Load<AudioClip>("Audio/SFX/SFX_ButtonClick");
        if (sfxChestOpen == null) sfxChestOpen = Resources.Load<AudioClip>("Audio/SFX/SFX_ChestOpen");
        if (sfxDoorClose == null) sfxDoorClose = Resources.Load<AudioClip>("Audio/SFX/SFX_DoorClose");
        if (sfxDoorOpen == null) sfxDoorOpen = Resources.Load<AudioClip>("Audio/SFX/SFX_DoorOpen");
        if (sfxFootstep == null) sfxFootstep = Resources.Load<AudioClip>("Audio/SFX/SFX_Footstep");
        if (sfxWait == null) sfxWait = Resources.Load<AudioClip>("Audio/SFX/SFX_Wait");
        if (sfxGameOver == null) sfxGameOver = Resources.Load<AudioClip>("Audio/SFX/SFX_GameOver");
        if (sfxItemCollect == null) sfxItemCollect = Resources.Load<AudioClip>("Audio/SFX/SFX_ItemCollect");
        if (sfxPressurePlate == null) sfxPressurePlate = Resources.Load<AudioClip>("Audio/SFX/SFX_PressurePlate");
        if (sfxSwordSwing == null) sfxSwordSwing = Resources.Load<AudioClip>("Audio/SFX/SFX_SwordSwing");
        if (sfxVictory == null) sfxVictory = Resources.Load<AudioClip>("Audio/SFX/SFX_Victory");
        if (sfxVictoryFinal == null) sfxVictoryFinal = Resources.Load<AudioClip>("Audio/SFX/SFX_VictoryFinal");
        if (sfxTransition1 == null) sfxTransition1 = Resources.Load<AudioClip>("Audio/SFX/SFX_Transition_1");
        if (sfxTransition2 == null) sfxTransition2 = Resources.Load<AudioClip>("Audio/SFX/SFX_Transition_2");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureAudioListener();
        HandleSceneMusic(scene.name);
        HookButtonsInScene();
    }

    private void HandleSceneMusic(string sceneName)
    {
        string lower = sceneName.ToLower();
        if (lower.Contains("complete"))
        {
            StopBGM();
        }
        else if (lower.Contains("title") || lower.Contains("menu") || lower.Contains("select"))
        {
            PlayTitleBGM();
        }
        else if (lower.Contains("level") || lower.Contains("sample"))
        {
            PlayGameplayBGM();
        }
    }

    public void HookButtonsInScene()
    {
        Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.onClick.RemoveListener(PlayButtonClick);
                btn.onClick.AddListener(PlayButtonClick);
            }
        }
    }

    public void PlayBGM(AudioClip clip, float volumeScale, BgmType bgmType, bool loop = true)
    {
        if (clip == null) return;
        if (bgmSource == null) InitializeAudioSources();

        currentBgmType = bgmType;
        float actualVol = volumeScale * masterVolume;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
        {
            bgmSource.volume = actualVol;
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = actualVol;
        bgmSource.Play();
    }

    public void PlayTitleBGM()
    {
        LoadClipsIfNull();
        PlayBGM(bgmTitle, titleMusicVolume, BgmType.Title, true);
    }

    public void PlayGameplayBGM()
    {
        LoadClipsIfNull();
        PlayBGM(bgmGameplay, gameplayMusicVolume, BgmType.Gameplay, true);
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
        currentBgmType = BgmType.None;
    }

    public void PlaySFX(AudioClip clip, float individualVolume)
    {
        if (clip == null) return;
        if (sfxSource == null) InitializeAudioSources();
        float finalVolume = individualVolume * masterVolume;
        sfxSource.PlayOneShot(clip, finalVolume);
    }

    // Individual Play Methods using their respective volume sliders
    public void PlayButtonClick() => PlaySFX(sfxButtonClick, buttonClickVolume);
    public void PlayFootstep() => PlaySFX(sfxFootstep, footstepVolume);
    public void PlayWait() => PlaySFX(sfxWait, waitVolume);
    public void PlayDoorOpen() => PlaySFX(sfxDoorOpen, doorOpenVolume);
    public void PlayDoorClose() => PlaySFX(sfxDoorClose, doorCloseVolume);
    public void PlayPressurePlate() => PlaySFX(sfxPressurePlate, pressurePlateVolume);
    public void PlayChestOpen() => PlaySFX(sfxChestOpen, chestOpenVolume);
    public void PlaySwordSwing() => PlaySFX(sfxSwordSwing, swordSwingVolume);
    public void PlayItemCollect() => PlaySFX(sfxItemCollect, itemCollectVolume);

    public void PlayGameOver()
    {
        StopBGM();
        PlaySFX(sfxGameOver, gameOverVolume);
    }

    public void PlayVictory(bool isFinal = false)
    {
        StopBGM();
        AudioClip clipToPlay = (isFinal && sfxVictoryFinal != null) ? sfxVictoryFinal : sfxVictory;
        PlaySFX(clipToPlay, victoryVolume);
    }

    public void PlayLevelTransitionSFX()
    {
        LoadClipsIfNull();
        if (sfxSource != null) sfxSource.Stop();
        StopBGM();
        AudioClip clipToPlay = (Random.value > 0.5f) ? sfxTransition2 : sfxTransition1;
        if (clipToPlay == null) clipToPlay = sfxTransition2 ?? sfxTransition1;
        if (clipToPlay != null)
        {
            Debug.Log($"<color=cyan>[AudioManager] Playing Level Transition SFX: {clipToPlay.name} at volume {transitionVolume}</color>");
            PlaySFX(clipToPlay, transitionVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] No transition audio clip found to play!");
        }
    }
}
