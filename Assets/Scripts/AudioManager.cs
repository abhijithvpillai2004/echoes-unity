using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Professional, clean Audio Manager for Echoes: Dungeon of Loops.
/// Utilizes Kenny Dungeon Audio assets from Assets/Audio/Audio_Kenny (and Assets/Resources/Audio_Kenny).
/// Features synchronized audio triggering, sound deduplication / throttling, volume balancing,
/// multiple footstep variations, and clean audio mixing.
/// </summary>
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
                    GameObject go = new GameObject("AudioManager");
                    _instance = go.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
        private set => _instance = value;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource uiSource;

    [Header("Player Movement & Actions")]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip waitClip;
    [SerializeField] private AudioClip weaponPickupClip;
    [SerializeField] private AudioClip attackHitClip;
    [SerializeField] private AudioClip playerTakeDamageClip;
    [SerializeField] private AudioClip playerDeathClip;

    [Header("Dungeon Mechanics & Environment")]
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;
    [SerializeField] private AudioClip pressurePlateActivateClip;
    [SerializeField] private AudioClip pressurePlateDeactivateClip;
    [SerializeField] private AudioClip echoReplayStartClip;
    [SerializeField] private AudioClip loopResetClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip chestOpenClip;

    [Header("Enemy & Boss Sounds")]
    [SerializeField] private AudioClip enemyMoveClip;
    [SerializeField] private AudioClip enemyDeathClip;
    [SerializeField] private AudioClip bossHitClip;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip buttonClickClip;

    // Throttle tracking to prevent audio stacking/harshness when multiple units move simultaneously
    private Dictionary<string, float> lastPlayedTimes = new Dictionary<string, float>();
    private const float DEFAULT_THROTTLE = 0.04f;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
        LoadKennyClips();
    }

    private void SetupAudioSources()
    {
        if (sfxSource == null)
        {
            sfxSource = GetComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        }
        sfxSource.playOnAwake = false;

        if (uiSource == null)
        {
            AudioSource[] sources = GetComponents<AudioSource>();
            if (sources.Length > 1) uiSource = sources[1];
            else uiSource = gameObject.AddComponent<AudioSource>();
        }
        uiSource.playOnAwake = false;
    }

    /// <summary>
    /// Loads authentic Kenney audio clips from Resources or AssetDatabase if slots are empty.
    /// </summary>
    public void LoadKennyClips()
    {
        // 1. Footstep variations (footstep00 to footstep09)
        if (footstepClips == null || footstepClips.Length == 0)
        {
            List<AudioClip> steps = new List<AudioClip>();
            for (int i = 0; i <= 9; i++)
            {
                AudioClip clip = LoadClip($"footstep0{i}");
                if (clip != null) steps.Add(clip);
            }
            footstepClips = steps.ToArray();
        }

        // 2. Player Actions
        if (waitClip == null) waitClip = LoadClip("cloth1");
        if (weaponPickupClip == null) weaponPickupClip = LoadClip("drawKnife1");
        if (attackHitClip == null) attackHitClip = LoadClip("knifeSlice");
        if (playerTakeDamageClip == null) playerTakeDamageClip = LoadClip("clothBelt2") ?? LoadClip("dropLeather");
        if (playerDeathClip == null) playerDeathClip = LoadClip("bookClose");

        // 3. Dungeon Mechanics
        if (doorOpenClip == null) doorOpenClip = LoadClip("doorOpen_1");
        if (doorCloseClip == null) doorCloseClip = LoadClip("doorClose_1");
        if (pressurePlateActivateClip == null) pressurePlateActivateClip = LoadClip("metalLatch");
        if (pressurePlateDeactivateClip == null) pressurePlateDeactivateClip = LoadClip("metalClick");
        if (echoReplayStartClip == null) echoReplayStartClip = LoadClip("bookOpen");
        if (loopResetClip == null) loopResetClip = LoadClip("bookFlip1");
        if (winClip == null) winClip = LoadClip("handleCoins");
        if (chestOpenClip == null) chestOpenClip = LoadClip("creak2");

        // 4. Enemy & Boss
        if (enemyMoveClip == null) enemyMoveClip = LoadClip("cloth4") ?? LoadClip("footstep08");
        if (enemyDeathClip == null) enemyDeathClip = LoadClip("chop");
        if (bossHitClip == null) bossHitClip = LoadClip("metalPot2");

        // 5. UI
        if (buttonClickClip == null) buttonClickClip = LoadClip("metalClick");
    }

    private AudioClip LoadClip(string clipName)
    {
        // First try Resources
        AudioClip clip = Resources.Load<AudioClip>($"Audio_Kenny/{clipName}");
        if (clip != null) return clip;

#if UNITY_EDITOR
        // Fallback in Editor directly to Assets/Audio/Audio_Kenny
        clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/Audio_Kenny/{clipName}.ogg");
        if (clip != null) return clip;
#endif

        return null;
    }

    // =========================================================================
    // PUBLIC AUDIO TRIGGER METHODS
    // =========================================================================

    public void PlayFootstep()
    {
        if (footstepClips != null && footstepClips.Length > 0)
        {
            AudioClip step = footstepClips[Random.Range(0, footstepClips.Length)];
            PlayClipWithPitch(sfxSource, step, 0.35f, 0.96f, 1.05f, "footstep", 0.08f);
        }
    }

    public void PlayWait()
    {
        PlayClipWithPitch(sfxSource, waitClip, 0.3f, 0.95f, 1.05f, "wait", 0.1f);
    }

    public void PlayDoorOpen()
    {
        PlayClip(sfxSource, doorOpenClip, 0.6f, "doorOpen", 0.05f);
    }

    public void PlayDoorClose()
    {
        PlayClip(sfxSource, doorCloseClip, 0.6f, "doorClose", 0.05f);
    }

    public void PlayPressurePlateActivate()
    {
        PlayClipWithPitch(sfxSource, pressurePlateActivateClip, 0.55f, 0.98f, 1.02f, "plateActive", 0.05f);
    }

    public void PlayPressurePlateDeactivate()
    {
        PlayClipWithPitch(sfxSource, pressurePlateDeactivateClip, 0.45f, 0.88f, 0.95f, "plateDeactive", 0.05f);
    }

    public void PlayEchoReplayStart()
    {
        PlayClip(sfxSource, echoReplayStartClip, 0.55f, "echoReplay", 0.1f);
    }

    public void PlayEnemyMove()
    {
        // Throttled: if multiple enemies move during the same turn, play a clean synchronized step
        PlayClipWithPitch(sfxSource, enemyMoveClip, 0.32f, 0.80f, 0.90f, "enemyMove", 0.12f);
    }

    public void PlayWeaponPickup()
    {
        PlayClip(sfxSource, weaponPickupClip, 0.7f, "weaponPickup", 0.1f);
    }

    public void PlayAttackHit()
    {
        PlayClipWithPitch(sfxSource, attackHitClip, 0.75f, 0.95f, 1.05f, "attackHit", 0.05f);
    }

    public void PlayEnemyDeath()
    {
        PlayClip(sfxSource, enemyDeathClip, 0.75f, "enemyDeath", 0.05f);
    }

    public void PlayBossHit()
    {
        PlayClipWithPitch(sfxSource, bossHitClip, 0.75f, 0.95f, 1.05f, "bossHit", 0.05f);
    }

    public void PlayPlayerTakeDamage()
    {
        PlayClip(sfxSource, playerTakeDamageClip, 0.75f, "playerDamage", 0.1f);
    }

    public void PlayPlayerDeath()
    {
        PlayClip(sfxSource, playerDeathClip, 0.8f, "playerDeath", 0.2f);
    }

    public void PlayGameOver()
    {
        PlayPlayerDeath();
    }

    public void PlayWin()
    {
        PlayClip(sfxSource, winClip, 0.8f, "win", 0.2f);
    }

    public void PlayChestOpen()
    {
        PlayClip(sfxSource, chestOpenClip, 0.75f, "chestOpen", 0.2f);
    }

    public void PlayLoopReset()
    {
        PlayClipWithPitch(sfxSource, loopResetClip, 0.6f, 0.95f, 1.05f, "loopReset", 0.15f);
    }

    public void PlayButtonClick()
    {
        PlayClipWithPitch(uiSource, buttonClickClip, 0.5f, 0.98f, 1.04f, "btnClick", 0.04f);
    }

    // =========================================================================
    // PLAY HELPERS WITH SYNCHRONIZATION & THROTTLING
    // =========================================================================

    private void PlayClip(AudioSource source, AudioClip clip, float volume, string key = null, float throttle = DEFAULT_THROTTLE)
    {
        if (clip == null || source == null) return;
        if (!IsAllowed(key, throttle)) return;

        source.pitch = 1f;
        source.PlayOneShot(clip, volume);
    }

    private void PlayClipWithPitch(AudioSource source, AudioClip clip, float volume, float minPitch, float maxPitch, string key = null, float throttle = DEFAULT_THROTTLE)
    {
        if (clip == null || source == null) return;
        if (!IsAllowed(key, throttle)) return;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clip, volume);
    }

    private bool IsAllowed(string key, float throttle)
    {
        if (string.IsNullOrEmpty(key)) return true;

        float now = Time.unscaledTime;
        if (lastPlayedTimes.TryGetValue(key, out float lastTime))
        {
            if (now - lastTime < throttle)
            {
                return false;
            }
        }

        lastPlayedTimes[key] = now;
        return true;
    }
}
