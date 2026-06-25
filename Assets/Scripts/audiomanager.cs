using UnityEngine;
using System.Collections.Generic;

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

    [Header("═══════════════ MUZYKA TŁA ═══════════════")]
    public AudioClip backgroundMusic;
    public AudioClip bossMusic;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
    private float footstepCooldown = 0.3f;

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
            return;
        }

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume * masterVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.volume = sfxVolume * masterVolume;
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlayFootstep()
    {
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
        if (attackClips.Length > 0)
            PlaySound(attackClips[Random.Range(0, attackClips.Length)], 0.5f);
    }

    public void PlayDamage()
    {
        if (damageClips.Length > 0)
            PlaySound(damageClips[Random.Range(0, damageClips.Length)], 0.6f);
    }

    public void PlayDeath()
    {
        if (deathClip != null)
            PlaySound(deathClip, 0.7f);
    }

    public void PlayHeal()
    {
        if (healClip != null)
            PlaySound(healClip, 0.5f);
    }

    public void PlaySpecialAbility()
    {
        if (specialAbilityClip != null)
            PlaySound(specialAbilityClip, 0.8f);
    }

    public void PlayUltimate()
    {
        if (ultimateClip != null)
            PlaySound(ultimateClip, 0.9f);
    }

    public void PlayCharge()
    {
        if (chargeClip != null)
            PlaySound(chargeClip, 0.7f);
    }

    public void PlayLaser()
    {
        if (laserClip != null)
            PlaySound(laserClip, 0.8f);
    }

    public void PlayEnemyHit()
    {
        if (enemyHitClips.Length > 0)
            PlaySound(enemyHitClips[Random.Range(0, enemyHitClips.Length)], 0.4f);
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathClips.Length > 0)
            PlaySound(enemyDeathClips[Random.Range(0, enemyDeathClips.Length)], 0.5f);
    }

    public void PlayEnemyAttack()
    {
        if (enemyAttackClips.Length > 0)
            PlaySound(enemyAttackClips[Random.Range(0, enemyAttackClips.Length)], 0.5f);
    }

    public void PlayPortalOpen()
    {
        if (portalOpenClip != null)
            PlaySound(portalOpenClip, 0.7f);
    }

    public void PlayPortalClose()
    {
        if (portalCloseClip != null)
            PlaySound(portalCloseClip, 0.7f);
    }

    public void PlayLevelUp()
    {
        if (levelUpClip != null)
            PlaySound(levelUpClip, 0.8f);
    }

    public void PlayPerkSelect()
    {
        if (perkSelectClip != null)
            PlaySound(perkSelectClip, 0.6f);
    }

    public void PlayWaveStart()
    {
        if (waveStartClip != null)
            PlaySound(waveStartClip, 0.7f);
    }

    public void StartBossMusic()
    {
        if (bossMusic != null)
        {
            musicSource.clip = bossMusic;
            musicSource.Play();
        }
    }

    public void StartNormalMusic()
    {
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, Camera.main != null ? Camera.main.transform.position : Vector3.zero, volume * sfxVolume * masterVolume);
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume * masterVolume;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume;
    }
}