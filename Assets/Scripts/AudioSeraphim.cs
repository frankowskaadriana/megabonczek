using UnityEngine;

public class AudioSeraphim : MonoBehaviour
{
    [Header("═══════════════ DŹWIĘKI SERAPHIM ═══════════════")]
    public AudioClip[] laserClips;
    public AudioClip[] specialClips;
    public AudioClip[] ultimateClips;
    public AudioClip[] healClips;
    public AudioClip[] chargeClips;
    public AudioClip deathClip;
    public AudioClip[] footstepClips;

    [Header("═══════════════ GŁOŚNOŚĆ ═══════════════")]
    [Range(0f, 1f)] public float laserVolume = 0.7f;
    [Range(0f, 1f)] public float specialVolume = 0.8f;
    [Range(0f, 1f)] public float ultimateVolume = 0.9f;
    [Range(0f, 1f)] public float healVolume = 0.6f;
    [Range(0f, 1f)] public float chargeVolume = 0.7f;
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float footstepVolume = 0.2f;

    [Header("═══════════════ INTERWAŁ KROKÓW ═══════════════")]
    public float footstepInterval = 0.35f;

    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private bool isMoving = false;
    private Rigidbody rb;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0.5f;
        audioSource.volume = 0.5f;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb != null)
        {
            isMoving = rb.linearVelocity.magnitude > 0.5f;
        }

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;
                PlayFootstep();
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    public void PlayLaser() => PlayRandomClip(laserClips, laserVolume);
    public void PlaySpecial() => PlayRandomClip(specialClips, specialVolume);
    public void PlayUltimate() => PlayRandomClip(ultimateClips, ultimateVolume);
    public void PlayHeal() => PlayRandomClip(healClips, healVolume);
    public void PlayCharge() => PlayRandomClip(chargeClips, chargeVolume);
    public void PlayDeath() => PlayClip(deathClip, deathVolume);
    public void PlayFootstep() => PlayRandomClip(footstepClips, footstepVolume);
    public void PlayHit() => PlayRandomClip(laserClips, laserVolume * 0.5f);

    private void PlayRandomClip(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0 || audioSource == null) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlayClip(clip, volume);
    }

    private void PlayClip(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }
}