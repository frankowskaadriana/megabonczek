using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("═══════════════ POSTACIE ═══════════════")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject shepherd;

    [Header("═══════════════ GROUND CHECK ═══════════════")]
    public Transform mountainManGroundCheck;
    public Transform seraphimGroundCheck;
    public Transform shepherdGroundCheck;

    [Header("═══════════════ SYSTEMY ═══════════════")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        if (mountainMan != null) mountainMan.SetActive(false);
        if (seraphim != null) seraphim.SetActive(false);
        if (shepherd != null) shepherd.SetActive(false);

        Debug.Log("Wcisnij 1 - Goral, 2 - Seraphim, 3 - Pasterz");
    }

    void Update()
    {
        if (hasSelected) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(mountainMan, mountainManGroundCheck, "Goral");
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(seraphim, seraphimGroundCheck, "Seraphim");
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(shepherd, shepherdGroundCheck, "Pasterz");
    }

    void SelectCharacter(GameObject character, Transform groundCheck, string name)
    {
        if (character == null)
        {
            Debug.LogError(name + " nie jest przypisany!");
            return;
        }

        if (currentCharacter != null) currentCharacter.SetActive(false);

        currentCharacter = character;
        currentCharacter.SetActive(true);
        currentCharacter.tag = "Player";

        if (cameraController != null) cameraController.SetTarget(currentCharacter.transform);

        PlayerMovement movement = currentCharacter.GetComponent<PlayerMovement>();
        if (movement == null) movement = currentCharacter.AddComponent<PlayerMovement>();

        if (groundCheck != null && movement != null)
            movement.SetGroundCheck(groundCheck);

        if (playerStats != null)
            playerStats.AssignToPlayer(currentCharacter);

        if (levelSystem != null)
        {
            if (name == "Goral")
            {
                if (currentCharacter.GetComponent<AbilitiesMountainMan>() == null)
                    currentCharacter.AddComponent<AbilitiesMountainMan>();
            }
            else if (name == "Seraphim")
            {
                if (currentCharacter.GetComponent<SeraphimAbilities>() == null)
                    currentCharacter.AddComponent<SeraphimAbilities>();
            }
            else if (name == "Pasterz")
            {
                if (currentCharacter.GetComponent<ShepherdAbilities>() == null)
                    currentCharacter.AddComponent<ShepherdAbilities>();
            }

            if (currentCharacter.GetComponent<PlayerHealth>() == null)
                currentCharacter.AddComponent<PlayerHealth>();

            levelSystem.playerHealth = currentCharacter.GetComponent<PlayerHealth>();
            levelSystem.StartGame();
        }

        hasSelected = true;
        Debug.Log("Aktywny: " + name);
    }
}