using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("Postacie w hierarchii")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject character3;

    [Header("Ground Check")]
    public Transform mountainManGroundCheck;
    public Transform seraphimGroundCheck;
    public Transform character3GroundCheck;

    [Header("Systemy")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        if (mountainMan != null) mountainMan.SetActive(false);
        if (seraphim != null) seraphim.SetActive(false);
        if (character3 != null) character3.SetActive(false);

        Debug.Log("Wcisnij 1 - Mountain Man, 2 - Seraphim, 3 - Character3");
    }

    void Update()
    {
        if (hasSelected) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(mountainMan, mountainManGroundCheck, "Mountain Man");
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(seraphim, seraphimGroundCheck, "Seraphim");
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(character3, character3GroundCheck, "Character3");
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
        {
            movement.SetGroundCheck(groundCheck);
        }

        if (playerStats != null)
        {
            playerStats.AssignToPlayer(currentCharacter);
        }

        if (levelSystem != null)
        {
            if (name == "Mountain Man")
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

            levelSystem.playerHealth = currentCharacter.GetComponent<PlayerHealth>();
            if (levelSystem.playerHealth == null)
                levelSystem.playerHealth = currentCharacter.AddComponent<PlayerHealth>();

            levelSystem.StartGame();
        }

        hasSelected = true;
        Debug.Log("Aktywny: " + name);
    }
}