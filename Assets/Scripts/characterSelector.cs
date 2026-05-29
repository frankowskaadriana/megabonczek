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
                AbilitiesMountainMan abilities = currentCharacter.GetComponent<AbilitiesMountainMan>();
                if (abilities == null) abilities = currentCharacter.AddComponent<AbilitiesMountainMan>();
                levelSystem.mountainManAbilities = abilities;
            }
            else if (name == "Seraphim")
            {
                SeraphimAbilities abilities = currentCharacter.GetComponent<SeraphimAbilities>();
                if (abilities == null) abilities = currentCharacter.AddComponent<SeraphimAbilities>();
                levelSystem.seraphimAbilities = abilities;
            }
            else if (name == "Pasterz")
            {
                ShepherdAbilities abilities = currentCharacter.GetComponent<ShepherdAbilities>();
                if (abilities == null) abilities = currentCharacter.AddComponent<ShepherdAbilities>();
                levelSystem.shepherdAbilities = abilities;
            }

            levelSystem.playerHealth = currentCharacter.GetComponent<PlayerHealth>();
            if (levelSystem.playerHealth == null)
                levelSystem.playerHealth = currentCharacter.AddComponent<PlayerHealth>();

            // Uruchom grę - to odpali timer!
            levelSystem.StartGame();
            Debug.Log("LevelSystem.StartGame() wywołane - timer powinien ruszyć");
        }

        hasSelected = true;
        Debug.Log("Aktywny: " + name);
    }
}