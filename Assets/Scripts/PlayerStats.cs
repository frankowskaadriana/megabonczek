using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Statystyki dla ka¿dej postaci")]
    public float mountainManHealth = 100f;
    public float mountainManSpeed = 5f;

    public float seraphimHealth = 40f;
    public float seraphimSpeed = 6f;

    public float character3Health = 50f;
    public float character3Speed = 5.5f;

    private GameObject currentPlayer;

    public void AssignToPlayer(GameObject player)
    {
        currentPlayer = player;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();

        if (health == null || movement == null) return;

        // Ustaw statystyki w zale¿noœci od nazwy postaci
        if (player.name.Contains("Mountain"))
        {
            health.maxHealth = mountainManHealth;
            health.currentHealth = mountainManHealth;
            movement.maxSpeed = mountainManSpeed;
            Debug.Log("Ustawiono statystyki Górala: HP=" + mountainManHealth + ", Speed=" + mountainManSpeed);
        }
        else if (player.name.Contains("Seraphim"))
        {
            health.maxHealth = seraphimHealth;
            health.currentHealth = seraphimHealth;
            movement.maxSpeed = seraphimSpeed;
            Debug.Log("Ustawiono statystyki Seraphima: HP=" + seraphimHealth + ", Speed=" + seraphimSpeed);
        }
        else if (player.name.Contains("Character3"))
        {
            health.maxHealth = character3Health;
            health.currentHealth = character3Health;
            movement.maxSpeed = character3Speed;
            Debug.Log("Ustawiono statystyki Character3: HP=" + character3Health + ", Speed=" + character3Speed);
        }

        health.UpdateUI();
    }
}