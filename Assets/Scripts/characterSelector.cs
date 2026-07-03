using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("═══════════════ POSTACIE ═══════════════")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject shepherd;

    [Header("═══════════════ UI PRZYCISKI ═══════════════")]
    public GameObject characterSelectionPanel;
    public Button mountainManButton;
    public Button seraphimButton;
    public Button shepherdButton;

    [Header("═══════════════ SYSTEMY ═══════════════")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        SetActiveAll(false);

        // ============================================================
        // NIGDY NIE BLOKUJ KURSORA!
        // ============================================================
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        if (mountainManButton != null)
            mountainManButton.onClick.AddListener(() => SelectCharacter(mountainMan, "Góral"));

        if (seraphimButton != null)
            seraphimButton.onClick.AddListener(() => SelectCharacter(seraphim, "Seraphim"));

        if (shepherdButton != null)
            shepherdButton.onClick.AddListener(() => SelectCharacter(shepherd, "Pasterz"));

        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBackgroundMusic();

        Debug.Log("🎮 Wybierz postać: 1 - Góral, 2 - Seraphim, 3 - Pasterz");
    }

    void Update()
    {
        if (hasSelected) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectCharacter(mountainMan, "Góral");
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectCharacter(seraphim, "Seraphim");
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectCharacter(shepherd, "Pasterz");
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
            Debug.LogError($"❌ {name} nie jest przypisany!");
            return;
        }

        if (hasSelected) return;

        SetActiveAll(false);

        currentCharacter = character;
        currentCharacter.SetActive(true);
        currentCharacter.tag = "Player";

        Debug.Log($"✅ Aktywowano: {name}");

        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(false);

        if (cameraController != null)
            cameraController.SetTarget(currentCharacter.transform);

        AddMissingComponents(currentCharacter, name);

        if (currentCharacter.GetComponent<PlayerHealth>() == null)
            currentCharacter.AddComponent<PlayerHealth>();

        if (currentCharacter.GetComponent<PlayerMovement>() == null)
            currentCharacter.AddComponent<PlayerMovement>();

        if (playerStats != null)
            playerStats.AssignToPlayer(currentCharacter);

        if (levelSystem != null)
            levelSystem.StartGame();

        if (AudioManager.Instance != null)
            AudioManager.Instance.OnCharacterSelected();

        hasSelected = true;

        // ============================================================
        // KURSOR ZAWSZE OD BLOKOWANY!
        // ============================================================
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"🎮 Wybrano: {name}!");
    }

    void AddMissingComponents(GameObject character, string name)
    {
        if (character == null) return;

        if (name == "Góral" || name == "Goral")
        {
            if (character.GetComponent<AbilitiesMountainMan>() == null)
                character.AddComponent<AbilitiesMountainMan>();
        }
        else if (name == "Seraphim")
        {
            if (character.GetComponent<AbilitiesSeraphim>() == null)
                character.AddComponent<AbilitiesSeraphim>();
        }
        else if (name == "Pasterz" || name == "Shepherd")
        {
            if (character.GetComponent<ShepherdAbilities>() == null)
                character.AddComponent<ShepherdAbilities>();
        }
    }

    public GameObject GetCurrentCharacter() => currentCharacter;
    public bool HasSelected() => hasSelected;

    public void ResetSelection()
    {
        hasSelected = false;
        SetActiveAll(false);
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("🔄 Zresetowano wybór postaci");
    }
}