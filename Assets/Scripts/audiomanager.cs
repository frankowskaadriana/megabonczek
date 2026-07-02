using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("═══════════════ MASTER SETTINGS ═══════════════")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("═══════════════ MUZYKA ═══════════════")]
    public AudioClip backgroundMusic;
    public AudioClip bossMusic;
    public AudioClip combatMusic;

    [Header("═══════════════ DŹWIĘKI ŚRODOWISKA ═══════════════")]
    public AudioClip levelUpClip;
    public AudioClip perkSelectClip;
    public AudioClip waveStartClip;
    public AudioClip waveCompleteClip;
    public AudioClip portalOpenClip;
    public AudioClip portalCloseClip;
    public AudioClip victoryClip;
    public AudioClip gameOverClip;

    [Header("═══════════════ DŹWIĘKI LASERA ═══════════════")]
    public AudioClip laserClip;

    [Header("═══════════════ DŹWIĘKI PRZECIWNIKÓW ═══════════════")]
    public AudioClip[] enemyHitClips;
    public AudioClip[] enemyDeathClips;
    public AudioClip[] enemyAttackClips;

    [Header("═══════════════ DŹWIĘKI GRACZA (OGÓLNE) ═══════════════")]
    public AudioClip deathClip;
    public AudioClip healClip;
    public AudioClip[] damageClips;

    // ============================================================
    // GŁOŚNOŚĆ POSZCZEGÓLNYCH DŹWIĘKÓW
    // ============================================================
    [Header("═══════════════ GŁOŚNOŚĆ DŹWIĘKÓW ═══════════════")]
    [Range(0f, 1f)] public float musicVolumeMultiplier = 1f;
    [Range(0f, 1f)] public float levelUpVolume = 0.8f;
    [Range(0f, 1f)] public float perkSelectVolume = 0.6f;
    [Range(0f, 1f)] public float waveStartVolume = 0.7f;
    [Range(0f, 1f)] public float waveCompleteVolume = 0.6f;
    [Range(0f, 1f)] public float portalOpenVolume = 0.7f;
    [Range(0f, 1f)] public float portalCloseVolume = 0.7f;
    [Range(0f, 1f)] public float victoryVolume = 0.9f;
    [Range(0f, 1f)] public float gameOverVolume = 0.8f;
    [Range(0f, 1f)] public float laserVolume = 0.8f;
    [Range(0f, 1f)] public float enemyHitVolume = 0.4f;
    [Range(0f, 1f)] public float enemyDeathVolume = 0.5f;
    [Range(0f, 1f)] public float enemyAttackVolume = 0.5f;
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float healVolume = 0.5f;
    [Range(0f, 1f)] public float damageVolume = 0.6f;

    // ============================================================
    // FADE IN / FADE OUT DLA KAŻDEGO DŹWIĘKU OSOBNO
    // ============================================================
    [Header("═══════════════ FADE IN DLA KAŻDEGO DŹWIĘKU ═══════════════")]
    [Range(0f, 2f)] public float levelUpFadeIn = 0.1f;
    [Range(0f, 2f)] public float perkSelectFadeIn = 0.1f;
    [Range(0f, 2f)] public float waveStartFadeIn = 0.1f;
    [Range(0f, 2f)] public float waveCompleteFadeIn = 0.1f;
    [Range(0f, 2f)] public float portalOpenFadeIn = 0.1f;
    [Range(0f, 2f)] public float portalCloseFadeIn = 0.1f;
    [Range(0f, 2f)] public float victoryFadeIn = 0.2f;
    [Range(0f, 2f)] public float gameOverFadeIn = 0.2f;
    [Range(0f, 2f)] public float laserFadeIn = 0.05f;
    [Range(0f, 2f)] public float enemyHitFadeIn = 0.05f;
    [Range(0f, 2f)] public float enemyDeathFadeIn = 0.05f;
    [Range(0f, 2f)] public float enemyAttackFadeIn = 0.05f;
    [Range(0f, 2f)] public float deathFadeIn = 0.1f;
    [Range(0f, 2f)] public float healFadeIn = 0.05f;
    [Range(0f, 2f)] public float damageFadeIn = 0.05f;

    [Header("═══════════════ FADE OUT DLA KAŻDEGO DŹWIĘKU ═══════════════")]
    [Range(0f, 2f)] public float levelUpFadeOut = 0.2f;
    [Range(0f, 2f)] public float perkSelectFadeOut = 0.2f;
    [Range(0f, 2f)] public float waveStartFadeOut = 0.2f;
    [Range(0f, 2f)] public float waveCompleteFadeOut = 0.2f;
    [Range(0f, 2f)] public float portalOpenFadeOut = 0.2f;
    [Range(0f, 2f)] public float portalCloseFadeOut = 0.2f;
    [Range(0f, 2f)] public float victoryFadeOut = 0.3f;
    [Range(0f, 2f)] public float gameOverFadeOut = 0.3f;
    [Range(0f, 2f)] public float laserFadeOut = 0.1f;
    [Range(0f, 2f)] public float enemyHitFadeOut = 0.1f;
    [Range(0f, 2f)] public float enemyDeathFadeOut = 0.1f;
    [Range(0f, 2f)] public float enemyAttackFadeOut = 0.1f;
    [Range(0f, 2f)] public float deathFadeOut = 0.2f;
    [Range(0f, 2f)] public float healFadeOut = 0.1f;
    [Range(0f, 2f)] public float damageFadeOut = 0.1f;

    [Header("═══════════════ USTAWIENIA FADE MUZYKI ═══════════════")]
    [Range(0f, 3f)] public float musicFadeIn = 0.5f;
    [Range(0f, 3f)] public float musicFadeOut = 0.5f;
    [Range(0f, 3f)] public float bossMusicFadeIn = 0.5f;
    [Range(0f, 3f)] public float bossMusicFadeOut = 0.5f;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private bool isInitialized = false;
    private bool isBossFight = false;
    private bool isCombat = false;
    private Coroutine currentFadeCoroutine;

    // ============================================================
    // AWAKE
    // ============================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
            else
            {
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
        musicSource.volume = musicVolume * masterVolume * musicVolumeMultiplier;

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
            StartCoroutine(FadeMusic(combatMusic, musicFadeIn));
        }
        else if (!isBossFight && !isCombat && backgroundMusic != null && musicSource.clip != backgroundMusic)
        {
            StartCoroutine(FadeMusic(backgroundMusic, musicFadeIn));
        }
    }

    // ============================================================
    // FADE MUZYKI
    // ============================================================
    private IEnumerator FadeMusic(AudioClip newClip, float fadeDuration)
    {
        if (musicSource.clip == newClip) yield break;

        float startVolume = musicSource.volume;
        float timer = 0f;
        while (timer < musicFadeOut)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / musicFadeOut);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.Play();

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume * musicVolumeMultiplier, timer / fadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume * masterVolume * musicVolumeMultiplier;
    }

    // ============================================================
    // FADE DLA SFX (POJEDYNCZY DŹWIĘK)
    // ============================================================
    private IEnumerator FadeSFX(AudioClip clip, float volume, float fadeIn, float fadeOut)
    {
        if (clip == null) yield break;

        sfxSource.volume = 0f;
        sfxSource.PlayOneShot(clip, 0f);

        float timer = 0f;
        float targetVolume = volume * sfxVolume * masterVolume;

        while (timer < fadeIn)
        {
            timer += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(0f, targetVolume, timer / fadeIn);
            yield return null;
        }

        sfxSource.volume = targetVolume;

        float clipLength = clip.length;
        float waitTime = Mathf.Max(0f, clipLength - fadeIn - fadeOut);
        yield return new WaitForSeconds(waitTime);

        timer = 0f;
        float startVolume = sfxSource.volume;
        while (timer < fadeOut)
        {
            timer += Time.deltaTime;
            sfxSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOut);
            yield return null;
        }

        sfxSource.volume = 0f;
        sfxSource.Stop();
    }

    // ============================================================
    // METODY ODTWARZANIA Z FADE IN/OUT
    // ============================================================

    public void PlayLevelUp() => StartCoroutine(FadeSFX(levelUpClip, levelUpVolume, levelUpFadeIn, levelUpFadeOut));
    public void PlayPerkSelect() => StartCoroutine(FadeSFX(perkSelectClip, perkSelectVolume, perkSelectFadeIn, perkSelectFadeOut));
    public void PlayWaveStart() => StartCoroutine(FadeSFX(waveStartClip, waveStartVolume, waveStartFadeIn, waveStartFadeOut));
    public void PlayWaveComplete() => StartCoroutine(FadeSFX(waveCompleteClip, waveCompleteVolume, waveCompleteFadeIn, waveCompleteFadeOut));
    public void PlayPortalOpen() => StartCoroutine(FadeSFX(portalOpenClip, portalOpenVolume, portalOpenFadeIn, portalOpenFadeOut));
    public void PlayPortalClose() => StartCoroutine(FadeSFX(portalCloseClip, portalCloseVolume, portalCloseFadeIn, portalCloseFadeOut));
    public void PlayVictory() => StartCoroutine(FadeSFX(victoryClip, victoryVolume, victoryFadeIn, victoryFadeOut));
    public void PlayGameOver() => StartCoroutine(FadeSFX(gameOverClip, gameOverVolume, gameOverFadeIn, gameOverFadeOut));
    public void PlayLaser() => StartCoroutine(FadeSFX(laserClip, laserVolume, laserFadeIn, laserFadeOut));
    public void PlayDeath() => StartCoroutine(FadeSFX(deathClip, deathVolume, deathFadeIn, deathFadeOut));
    public void PlayHeal() => StartCoroutine(FadeSFX(healClip, healVolume, healFadeIn, healFadeOut));
    public void PlayDamage() => StartCoroutine(FadeSFXArray(damageClips, damageVolume, damageFadeIn, damageFadeOut));

    // DŹWIĘKI PRZECIWNIKÓW
    public void PlayEnemyHit() => StartCoroutine(FadeSFXArray(enemyHitClips, enemyHitVolume, enemyHitFadeIn, enemyHitFadeOut));
    public void PlayEnemyDeath() => StartCoroutine(FadeSFXArray(enemyDeathClips, enemyDeathVolume, enemyDeathFadeIn, enemyDeathFadeOut));
    public void PlayEnemyAttack() => StartCoroutine(FadeSFXArray(enemyAttackClips, enemyAttackVolume, enemyAttackFadeIn, enemyAttackFadeOut));

    // ============================================================
    // FADE DLA TABLICY DŹWIĘKÓW
    // ============================================================
    private IEnumerator FadeSFXArray(AudioClip[] clips, float volume, float fadeIn, float fadeOut)
    {
        if (clips == null || clips.Length == 0) yield break;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        yield return StartCoroutine(FadeSFX(clip, volume, fadeIn, fadeOut));
    }

    // ============================================================
    // ODTWARZANIE PRZEZ NAZWĘ
    // ============================================================
    public void PlaySoundByName(string soundName)
    {
        if (!isInitialized) return;

        switch (soundName)
        {
            case "LevelUp": PlayLevelUp(); break;
            case "PerkSelect": PlayPerkSelect(); break;
            case "WaveStart": PlayWaveStart(); break;
            case "WaveComplete": PlayWaveComplete(); break;
            case "PortalOpen": PlayPortalOpen(); break;
            case "PortalClose": PlayPortalClose(); break;
            case "Victory": PlayVictory(); break;
            case "GameOver": PlayGameOver(); break;
            case "Laser": PlayLaser(); break;
            case "Death": PlayDeath(); break;
            case "Heal": PlayHeal(); break;
            case "Damage": PlayDamage(); break;
            case "EnemyHit": PlayEnemyHit(); break;
            case "EnemyDeath": PlayEnemyDeath(); break;
            case "EnemyAttack": PlayEnemyAttack(); break;
            default:
                Debug.LogWarning($"⚠️ Nieznany dźwięk: {soundName}");
                break;
        }
    }

    // ============================================================
    // !!! METODY DLA MUZYKI !!!
    // ============================================================

    public void PlayBackgroundMusic()
    {
        if (!isInitialized) return;
        if (backgroundMusic == null) return;

        if (isBossFight) return; // Boss ma priorytet

        isCombat = false;
        StartCoroutine(FadeMusic(backgroundMusic, musicFadeIn));
        Debug.Log("🎵 Odtwarzam muzykę tła");
    }

    public void PlayCombatMusic()
    {
        if (!isInitialized) return;
        if (combatMusic == null) return;
        if (isBossFight) return; // Boss ma priorytet

        isCombat = true;
        StartCoroutine(FadeMusic(combatMusic, musicFadeIn));
        Debug.Log("🎵 Odtwarzam muzykę walki");
    }

    public void StartBossMusic()
    {
        if (!isInitialized) return;
        if (bossMusic == null) return;

        isBossFight = true;
        isCombat = false;
        StartCoroutine(FadeMusic(bossMusic, bossMusicFadeIn));
        Debug.Log("🎵 Odtwarzam muzykę bossa");
    }

    public void StopBossMusic()
    {
        if (!isInitialized) return;

        isBossFight = false;

        if (isCombat && combatMusic != null)
        {
            StartCoroutine(FadeMusic(combatMusic, musicFadeIn));
        }
        else if (backgroundMusic != null)
        {
            StartCoroutine(FadeMusic(backgroundMusic, musicFadeIn));
        }
        Debug.Log("🎵 Wyłączono muzykę bossa");
    }

    // ============================================================
    // METODY DLA characterSelector.cs
    // ============================================================
    public void OnCharacterSelected()
    {
        if (!isInitialized) return;

        // Jeśli są wrogowie - włącz combat music, jeśli nie - wróć do tła
        if (isCombat && combatMusic != null)
        {
            PlayCombatMusic();
        }
        else if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }

        Debug.Log("🎵 Postać wybrana - przełączono muzykę!");
    }

    // ============================================================
    // METODY DLA leszy.cs
    // ============================================================
    public void OnBossSpawn()
    {
        StartBossMusic();
    }

    public void OnBossDeath()
    {
        StopBossMusic();
    }

    // ============================================================
    // METODY DLA enemyHealth.cs i waveSpawner.cs
    // ============================================================
    public void OnEnemySpawned()
    {
        if (!isInitialized) return;
        if (!isCombat && !isBossFight)
        {
            PlayCombatMusic();
        }
    }

    public void OnEnemyDied()
    {
        if (!isInitialized) return;
        PlayEnemyDeath();

        // Sprawdź czy są jeszcze jacyś wrogowie
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
        if (enemies.Length == 0 && isCombat && !isBossFight)
        {
            isCombat = false;
            PlayBackgroundMusic();
        }
    }

    public void SetCombatMode(bool inCombat)
    {
        if (isBossFight) return;

        isCombat = inCombat;
        if (inCombat)
        {
            PlayCombatMusic();
        }
        else
        {
            PlayBackgroundMusic();
        }
    }

    // ============================================================
    // USTAWIANIE GŁOŚNOŚCI
    // ============================================================
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume * musicVolumeMultiplier;
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
        musicSource.volume = musicVolume * masterVolume * musicVolumeMultiplier;
    }

    public void SetMusicVolumeMultiplier(float volume)
    {
        musicVolumeMultiplier = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume * masterVolume * musicVolumeMultiplier;
    }

    // ============================================================
    // ZATRZYMYWANIE
    // ============================================================
    public void StopAllSFX() => sfxSource.Stop();
    public void StopMusic() => musicSource.Stop();
    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();

    // ============================================================
    // LISTA WSZYSTKICH DŹWIĘKÓW (DLA UI)
    // ============================================================
    public string[] GetAllSoundNames()
    {
        return new string[]
        {
            "LevelUp", "PerkSelect", "WaveStart", "WaveComplete",
            "PortalOpen", "PortalClose", "Victory", "GameOver",
            "Laser", "Death", "Heal", "Damage",
            "EnemyHit", "EnemyDeath", "EnemyAttack"
        };
    }
}