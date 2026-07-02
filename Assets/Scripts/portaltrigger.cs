using UnityEngine;
using System.Collections;

public class PortalTrigger : MonoBehaviour
{
    [Header("═══════════════ USTAWIENIA PORTALU ═══════════════")]
    public float activationDelay = 2f;
    public GameObject portalVisual;
    public GameObject portalON; // Obiekt który się włącza po aktywacji

    [Header("═══════════════ REFERENCJE ═══════════════")]
    public GameManager gameManager;

    private bool isActive = false;
    private bool isUsed = false;
    private bool isActivating = false;
    private float activationTimer = 0f;

    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();

        // Ukryj PortalON na starcie
        if (portalON != null)
            portalON.SetActive(false);

        // Ukryj wizualizację
        if (portalVisual != null)
            portalVisual.SetActive(false);

        // Ustaw tag na "PortalTrigger"
        gameObject.tag = "PortalTrigger";

        Debug.Log("🌀 Portal gotowy! Czekam na aktywację...");
    }

    void Update()
    {
        // Sprawdź czy gra się skończyła (czas minął)
        if (gameManager != null && gameManager.IsGameFinished())
        {
            if (!isActive && !isActivating)
            {
                StartCoroutine(ActivatePortal());
            }
        }
    }

    IEnumerator ActivatePortal()
    {
        isActivating = true;
        activationTimer = 0f;

        Debug.Log("🌀 Portal aktywuje się...");

        // === WŁĄCZ PORTALON ===
        if (portalON != null)
        {
            portalON.SetActive(true);
            Debug.Log("🌀 PORTALON WŁĄCZONY!");
        }

        // Pokaż wizualizację
        if (portalVisual != null)
        {
            portalVisual.SetActive(true);
            portalVisual.transform.localScale = Vector3.zero;

            // Animacja pojawiania się
            float progress = 0f;
            while (progress < 1f)
            {
                progress += Time.deltaTime / activationDelay;
                portalVisual.transform.localScale = Vector3.one * Mathf.Lerp(0f, 1f, progress);
                yield return null;
            }
        }

        // Aktywuj portal
        isActive = true;
        isActivating = false;

        // Powiadom gracza
        if (gameManager != null)
        {
            gameManager.ShowPortalMessage("🌀 Portal jest gotowy! Wejdź w niego, aby wygrać!");
        }

        Debug.Log("🌀 Portal AKTYWNY! Gracz może wygrać!");
    }

    void OnTriggerEnter(Collider other)
    {
        // Sprawdź czy gracz wszedł i portal jest aktywny
        if (!isActive || isUsed) return;

        if (other.CompareTag("Player"))
        {
            isUsed = true;
            Debug.Log("🎉 Gracz wszedł do portalu! ZWYCIĘSTWO!");

            // Powiadom GameManager o zwycięstwie
            if (gameManager != null)
            {
                gameManager.Victory("🌀 PRZESZEDŁEŚ PRZEZ PORTAL! ZWYCIĘSTWO!");
            }
            else
            {
                Debug.Log("🏆 ZWYCIĘSTWO! (GameManager nie znaleziony)");
            }
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public bool IsUsed()
    {
        return isUsed;
    }

    public void ForceActivate()
    {
        if (!isActive && !isActivating)
        {
            StartCoroutine(ActivatePortal());
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(transform.position, 2f);

        if (isActive)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, 2f);
        }
    }
}