using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("═══════════════ GÓRAL ═══════════════")]
    public float goralHealth = 100f;
    public float goralSpeed = 5f;

    [Header("═══════════════ SERAPHIM ═══════════════")]
    public float seraphimHealth = 40f;
    public float seraphimSpeed = 6f;

    [Header("═══════════════ PASTERZ ═══════════════")]
    public float pasterzHealth = 50f;
    public float pasterzSpeed = 5.5f;
    public float pasterzArmor = 20f;

    private GameManager gameManager;

    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }

    public void AssignToPlayer(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (health == null || movement == null)
        {
            Debug.LogWarning("PlayerHealth lub PlayerMovement nie znaleziony!");
            return;
        }

        if (player.name.Contains("Mountain") || player.name.Contains("Goral"))
        {
            health.SetBaseHealth(goralHealth, 0);
            movement.maxSpeed = goralSpeed;
            Debug.Log($"Goral: HP={goralHealth}, Speed={goralSpeed}");
        }
        else if (player.name.Contains("Seraphim"))
        {
            health.SetBaseHealth(seraphimHealth, 0);
            movement.maxSpeed = seraphimSpeed;
            Debug.Log($"Seraphim: HP={seraphimHealth}, Speed={seraphimSpeed}");
        }
        else if (player.name.Contains("Shepherd") || player.name.Contains("Pasterz"))
        {
            health.SetBaseHealth(pasterzHealth, pasterzArmor);
            movement.maxSpeed = pasterzSpeed;
            Debug.Log($"Pasterz: HP={pasterzHealth}, Speed={pasterzSpeed}, Armor={pasterzArmor}");
        }

        // === TYLKO GAME MANAGER ===
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    public void UpdateUI()
    {
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }

    public void RefreshUI()
    {
        if (gameManager != null)
        {
            gameManager.UpdateUI();
        }
    }
}