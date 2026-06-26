using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("═══════════════ MASTER SETTINGS ═══════════════")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("═══════════════ DŹWIĘKI GRACZA ═══════════════")]
    public AudioClip[] footstepClips;
    public AudioClip[] attackClips;
    public AudioClip[] damageClips;
    public AudioClip deathClip;
    public AudioClip healClip;

    [Header("═══════════════ DŹWIĘKI UMIEJĘTNOŚCI ═══════════════")]
    public AudioClip specialAbilityClip;
    public AudioClip ultimateClip;
    public AudioClip chargeClip;
    public AudioClip laserClip;

    [Header("═══════════════ DŹWIĘKI PRZECIWNIKÓW ═══════════════")]
    public AudioClip[] enemyHitClips;
    public AudioClip[] enemyDeathClips;
    public AudioClip[] enemyAttackClips;

    [Header("═══════════════ DŹWIĘKI ŚRODOWISKA ═══════════════")]
    public AudioClip portalOpenClip;
    public AudioClip portalCloseClip;
    public AudioClip levelUpClip;
    public AudioClip perkSelectClip;
    public AudioClip waveStartClip;
    public AudioClip waveCompleteClip;

    [Header("═══════════════ MUZYKA TŁA ═══════════════")]
    public AudioClip backgroundMusic;
    public AudioClip bossMusic;
    public AudioClip combatMusic;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    private float footstepCooldown = 0.3f;
    private bool isBossFight = false;
    private bool isCombat = false;
    private bool isInitialized = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad działa tylko na root GameObject
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                // Jeśli nie jest root, przenieś do root
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
            }
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Initialize()
    {
        if (isInitialized) return;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume * masterVolume;

        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        isInitialized = true;
        Debug.Log("🎵 AudioManager gotowy!");
    }

    void Update()
    {
        if (!isInitialized) return;

        if (!isBossFight && isCombat && combatMusic != null && musicSource.clip != combatMusic)
        {
            musicSource.clip = combatMusic;
            musicSource.Play();
        }
        else if (!isBossFight && !isCombat && backgroundMusic != null && musicSource.clip != backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    // ===== DŹWIĘKI GRACZA =====

    public void PlayFootstep()
    {
        if (!isInitialized) return;
        if (footstepClips.Length == 0) return;

        if (!lastPlayTime.ContainsKey("Footstep"))
            lastPlayTime["Footstep"] = 0;

        if (Time.time - lastPlayTime["Footstep"] >= footstepCooldown)
        {
            lastPlayTime["Footstep"] = Time.time;
            PlaySound(footstepClips[Random.Range(0, footstepClips.Length)], 0.3f);
        }
    }

    public void PlayAttack()
    {
        if (!isInitialized) return;
        if (attackClips.Length > 0)
            PlaySound(attackClips[Random.Range(0, attackClips.Length)], 0.5f);
    }

    public void PlayDamage()
    {
        if (!isInitialized) return;
        if (damageClips.Length > 0)
            PlaySound(damageClips[Random.Range(0, damageClips.Length)], 0.6f);
    }

    public void PlayDeath()
    {
        if (!isInitialized) return;
        if (deathClip != null)
            PlaySound(deathClip, 0.7f);
    }

    public void PlayHeal()
    {
        if (!isInitialized) return;
        if (healClip != null)
            PlaySound(healClip, 0.5f);
    }

    // ===== DŹWIĘKI UMIEJĘTNOŚCI =====

    public void PlaySpecialAbility()
    {
        if (!isInitialized) return;
        if (specialAbilityClip != null)
            PlaySound(specialAbilityClip, 0.8f);
    }

    public void PlayUltimate()
    {
        if (!isInitialized) return;
        if (ultimateClip != null)
        {
            PlaySound(ultimateClip, 0.9f);
        }
    }

    public void PlayCharge()
    {
        if (!isInitialized) return;
        if (chargeClip != null)
            PlaySound(chargeClip, 0.7f);
    }

    public void PlayLaser()
    {
        if (!isInitialized) return;
        if (laserClip != null)
            PlaySound(laserClip, 0.8f);
    }

    // ===== DŹWIĘKI PRZECIWNIKÓW =====

    public void PlayEnemyHit()
    {
        if (!isInitialized) return;
        if (enemyHitClips.Length > 0)
            PlaySound(enemyHitClips[Random.Range(0, enemyHitClips.Length)], 0.4f);
    }

    public void PlayEnemyDeath()
    {
        if (!isInitialized) return;
        if (enemyDeathClips.Length > 0)
            PlaySound(enemyDeathClips[Random.Range(0, enemyDeathClips.Length)], 0.5f);
    }

    public void PlayEnemyAttack()
    {
        if (!isInitialized) return;
        if (enemyAttackClips.Length > 0)
            PlaySound(enemyAttackClips[Random.Range(0, enemyAttackClips.Length)], 0.5f);
    }

    // ===== DŹWIĘKI ŚRODOWISKA =====

    public void PlayPortalOpen()
    {
        if (!isInitialized) return;
        if (portalOpenClip != null)
            PlaySound(portalOpenClip, 0.7f);
    }

    public void PlayPortalClose()
    {
        if (!isInitialized) return;
        if (portalCloseClip != null)
            PlaySound(portalCloseClip, 0.7f);
    }

    public void PlayLevelUp()
    {
        if (!isInitialized) return;
        if (levelUpClip != null)
            PlaySound(levelUpClip, 0.8f);
    }

    public void PlayPerkSelect()
    {
        if (!isInitialized) return;
        if (perkSelectClip != null)
            PlaySound(perkSelectClip, 0.6f);
    }

    public void PlayWaveStart()
    {
        if (!isInitialized) return;
        if (waveStartClip != null)
        {
            PlaySound(waveStartClip, 0.7f);
            SetCombatMode(true);
        }
    }

    public void PlayWaveComplete()
    {
        if (!isInitialized) return;
        if (waveCompleteClip != null)
        {
            PlaySound(waveCompleteClip, 0.6f);
            SetCombatMode(false);
        }
    }

    // ===== MUZYKA =====

    public void StartBossMusic()
    {
        if (!isInitialized) return;
        if (bossMusic != null && !isBossFight)
        {
            isBossFight = true;
            isCombat = false;
            musicSource.clip = bossMusic;
            musicSource.Play();
            Debug.Log("🎵 MUZYKA BOSSA!");
        }
    }

    public void StopBossMusic()
    {
        if (!isInitialized) return;
        if (isBossFight)
        {
            isBossFight = false;
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
                Debug.Log("🎵 Powrót do muzyki tła");
            }
        }
    }

    public void SetCombatMode(bool inCombat)
    {
        if (!isInitialized) return;
        isCombat = inCombat;
    }

    // ===== METODY POMOCNICZE =====

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        if (Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume * sfxVolume * masterVolume);
        else
            AudioSource.PlayClipAtPoint(clip, Vector3.zero, volume * sfxVolume * masterVolume);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume * masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume * masterVolume;
    }

    public void OnEnemySpawned()
    {
        if (!isInitialized) return;
        if (!isCombat && !isBossFight)
            SetCombatMode(true);
    }

    public void OnEnemyDied()
    {
        if (!isInitialized) return;
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        if (enemies.Length == 0 && isCombat)
        {
            SetCombatMode(false);
        }
    }

    public void OnBossSpawned()
    {
        StartBossMusic();
    }

    public void OnBossDied()
    {
        StopBossMusic();
    }
}