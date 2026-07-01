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

    [Header("═══════════════ DŹWIĘKI OGÓLNE ═══════════════")]
    public AudioClip[] footstepClips;
    public AudioClip[] damageClips;
    public AudioClip deathClip;
    public AudioClip healClip;

    [Header("═══════════════ DŹWIĘKI - GÓRAL (Mountain Man) ═══════════════")]
    public AudioClip[] goralAttackClips;
    public AudioClip goralStompClip;
    public AudioClip goralSpecialClip;
    public AudioClip goralUltimateClip;

    [Header("═══════════════ DŹWIĘKI - SERAPHIM (Anioł) ═══════════════")]
    public AudioClip[] seraphimAttackClips;
    public AudioClip seraphimLaserClip;
    public AudioClip seraphimHealClip;
    public AudioClip seraphimChargeClip;
    public AudioClip seraphimSpecialClip;
    public AudioClip seraphimUltimateClip;

    [Header("═══════════════ DŹWIĘKI - PASTERZ (Shepherd) ═══════════════")]
    public AudioClip[] shepherdAttackClips;
    public AudioClip shepherdBarkClip;
    public AudioClip shepherdSheepSpawnClip;
    public AudioClip shepherdSpecialClip;

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

    [Header("═══════════════ MUZYKA ═══════════════")]
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

    // === WYKRYWANIE POSTACI ===
    private string currentCharacter = "None";
    private GameObject playerObject;
    private float characterCheckTimer = 0f;
    private float characterCheckInterval = 0.5f;

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

        characterCheckTimer += Time.deltaTime;
        if (characterCheckTimer >= characterCheckInterval)
        {
            characterCheckTimer = 0f;
            DetectCharacter();
        }

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

    // ============================================
    // WYKRYWANIE POSTACI
    // ============================================

    void DetectCharacter()
    {
        if (playerObject == null)
        {
            playerObject = GameObject.FindWithTag("Player");
            if (playerObject != null)
            {
                UpdateCharacter();
            }
            return;
        }

        string newCharacter = GetCharacterType();
        if (newCharacter != currentCharacter)
        {
            currentCharacter = newCharacter;
            Debug.Log($"🎵 Wykryto postać: {currentCharacter}");
        }
    }

    string GetCharacterType()
    {
        if (playerObject == null) return "None";

        if (playerObject.GetComponent<AbilitiesMountainMan>() != null)
            return "Goral";
        else if (playerObject.GetComponent<AbilitiesSeraphim>() != null)
            return "Seraphim";
        else if (playerObject.GetComponent<ShepherdAbilities>() != null)
            return "Shepherd";

        return "None";
    }

    void UpdateCharacter()
    {
        currentCharacter = GetCharacterType();
        Debug.Log($"🎵 Wykryto postać: {currentCharacter}");
    }

    public void SetCharacter(string characterName)
    {
        currentCharacter = characterName;
        Debug.Log($"🎵 Ręcznie ustawiono postać: {currentCharacter}");
    }

    public string GetCurrentCharacter()
    {
        return currentCharacter;
    }

    // ============================================
    // DŹWIĘKI OGÓLNE
    // ============================================

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

    // ============================================
    // DŹWIĘKI ATAKU - ZALEŻNE OD POSTACI
    // ============================================

    public void PlayAttack()
    {
        if (!isInitialized) return;

        switch (currentCharacter)
        {
            case "Goral":
                if (goralAttackClips.Length > 0)
                    PlaySound(goralAttackClips[Random.Range(0, goralAttackClips.Length)], 0.6f);
                break;

            case "Seraphim":
                if (seraphimAttackClips.Length > 0)
                    PlaySound(seraphimAttackClips[Random.Range(0, seraphimAttackClips.Length)], 0.5f);
                break;

            case "Shepherd":
                if (shepherdAttackClips.Length > 0)
                    PlaySound(shepherdAttackClips[Random.Range(0, shepherdAttackClips.Length)], 0.5f);
                break;
        }
    }

    // ============================================
    // DŹWIĘKI UMIEJĘTNOŚCI - ZALEŻNE OD POSTACI
    // ============================================

    public void PlayStomp()
    {
        if (!isInitialized) return;
        if (currentCharacter == "Goral" && goralStompClip != null)
            PlaySound(goralStompClip, 0.8f);
    }

    public void PlaySpecialAbility()
    {
        if (!isInitialized) return;

        switch (currentCharacter)
        {
            case "Goral":
                if (goralSpecialClip != null)
                    PlaySound(goralSpecialClip, 0.8f);
                break;

            case "Seraphim":
                if (seraphimSpecialClip != null)
                    PlaySound(seraphimSpecialClip, 0.8f);
                break;

            case "Shepherd":
                if (shepherdSpecialClip != null)
                    PlaySound(shepherdSpecialClip, 0.8f);
                break;
        }
    }

    public void PlayUltimate()
    {
        if (!isInitialized) return;

        switch (currentCharacter)
        {
            case "Goral":
                if (goralUltimateClip != null)
                    PlaySound(goralUltimateClip, 0.9f);
                break;

            case "Seraphim":
                if (seraphimUltimateClip != null)
                    PlaySound(seraphimUltimateClip, 0.9f);
                break;
        }
    }

    public void PlayCharge()
    {
        if (!isInitialized) return;
        if (currentCharacter == "Seraphim" && seraphimChargeClip != null)
            PlaySound(seraphimChargeClip, 0.7f);
    }

    public void PlayLaser()
    {
        if (!isInitialized) return;
        if (currentCharacter == "Seraphim" && seraphimLaserClip != null)
            PlaySound(seraphimLaserClip, 0.8f);
    }

    public void PlayHeal()
    {
        if (!isInitialized) return;

        if (currentCharacter == "Seraphim" && seraphimHealClip != null)
            PlaySound(seraphimHealClip, 0.6f);
        else if (healClip != null)
            PlaySound(healClip, 0.5f);
    }

    public void PlayBark()
    {
        if (!isInitialized) return;
        if (currentCharacter == "Shepherd" && shepherdBarkClip != null)
            PlaySound(shepherdBarkClip, 0.7f);
    }

    public void PlaySheepSpawn()
    {
        if (!isInitialized) return;
        if (currentCharacter == "Shepherd" && shepherdSheepSpawnClip != null)
            PlaySound(shepherdSheepSpawnClip, 0.5f);
    }

    // ============================================
    // DŹWIĘKI PRZECIWNIKÓW
    // ============================================

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

    // ============================================
    // DŹWIĘKI ŚRODOWISKA
    // ============================================

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

    // ============================================
    // MUZYKA
    // ============================================

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

    // ============================================
    // METODY POMOCNICZE
    // ============================================

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

    // ============================================
    // METODY DLA WAVESPAWNER
    // ============================================

    public void OnEnemySpawned()
    {
        if (!isInitialized) return;
        if (!isCombat && !isBossFight)
            SetCombatMode(true);
    }

    public void OnEnemyDied()
    {
        if (!isInitialized) return;
        BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None);
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

    // ============================================
    // METODY DLA GAME MANAGER
    // ============================================

    public void PlayGameOver()
    {
        if (!isInitialized) return;
        if (deathClip != null)
            PlaySound(deathClip, 0.8f);
    }

    public void PlayVictory()
    {
        if (!isInitialized) return;
        if (levelUpClip != null)
            PlaySound(levelUpClip, 0.9f);
    }
}