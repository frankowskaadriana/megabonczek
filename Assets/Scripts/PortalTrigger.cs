using UnityEngine;

public class PortalTrigger : MonoBehaviour
{
    [Header("═══════════════ PORTAL SETTINGS ═══════════════")]
    public bool isActive = true;
    public GameObject portalVisual;
    public ParticleSystem portalParticles;

    private LevelSystem levelSystem;

    void Start()
    {
        if (portalVisual != null)
            portalVisual.SetActive(true);

        if (portalParticles != null)
            portalParticles.Play();

        FindLevelSystem();
    }

    void FindLevelSystem()
    {
        levelSystem = FindFirstObjectByType<LevelSystem>();

        if (levelSystem == null)
        {
            GameObject gm = GameObject.Find("GameManager");
            if (gm != null)
                levelSystem = gm.GetComponent<LevelSystem>();
        }

        if (levelSystem == null)
            Debug.LogWarning("Nie znaleziono LevelSystem! Portal nie będzie działać.");
    }

    public void SetLevelSystem(LevelSystem system)
    {
        levelSystem = system;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        if (other.CompareTag("Player") && levelSystem != null)
        {
            Debug.Log("Gracz dotknal portalu!");
            levelSystem.OnPortalEnter();

            if (portalParticles != null)
                portalParticles.Stop();

            isActive = false;
        }
    }
}