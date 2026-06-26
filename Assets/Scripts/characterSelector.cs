using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("Postacie")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject shepherd;

    [Header("Systemy")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        SetActiveAll(false);
        Debug.Log("Wcisnij 1 - Goral, 2 - Seraphim, 3 - Pasterz");
    }

    void Update()
    {
        if (hasSelected) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(mountainMan, "Goral");
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(seraphim, "Seraphim");
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(shepherd, "Pasterz");
    }

    void SetActiveAll(bool active)
    {
        if (mountainMan != null) mountainMan.SetActive(active);
        if (seraphim != null) seraphim.SetActive(active);
        if (shepherd != null) shepherd.SetActive(active);
    }

    void SelectCharacter(GameObject character, string name)
    {
        if (character == null)
        {
            Debug.LogError($"{name} nie jest przypisany!");
            return;
        }

        SetActiveAll(false);
        currentCharacter = character;
        currentCharacter.SetActive(true);
        currentCharacter.tag = "Player";

        if (cameraController != null) cameraController.SetTarget(currentCharacter.transform);

        AddMissingComponents(name);

        PlayerHealth health = currentCharacter.GetComponent<PlayerHealth>();
        if (health == null) health = currentCharacter.AddComponent<PlayerHealth>();

        if (levelSystem != null)
        {
            levelSystem.StartGame();
        }

        if (playerStats != null) playerStats.AssignToPlayer(currentCharacter);

        hasSelected = true;
        Debug.Log($"Aktywny: {name}");
    }

    void AddMissingComponents(string name)
    {
        if (currentCharacter.GetComponent<PlayerMovement>() == null)
            currentCharacter.AddComponent<PlayerMovement>();

        if (name == "Goral" && currentCharacter.GetComponent<AbilitiesMountainMan>() == null)
            currentCharacter.AddComponent<AbilitiesMountainMan>();
        else if (name == "Seraphim" && currentCharacter.GetComponent<AbilitiesSeraphim>() == null)
            currentCharacter.AddComponent<AbilitiesSeraphim>();
        else if (name == "Pasterz" && currentCharacter.GetComponent<ShepherdAbilities>() == null)
            currentCharacter.AddComponent<ShepherdAbilities>();
    }
}