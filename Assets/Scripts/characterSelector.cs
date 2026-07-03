using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("═══════════════ IKONY SPECIAL (Q) ═══════════════")]
    public GameObject goralSpecialIcon;
    public GameObject seraphimSpecialIcon;
    public GameObject shepherdSpecialIcon;
    public TextMeshProUGUI goralSpecialCD;
    public TextMeshProUGUI seraphimSpecialCD;
    public TextMeshProUGUI shepherdSpecialCD;

    [Header("═══════════════ IKONY ULTIMATE (R) ═══════════════")]
    public GameObject goralUltimateIcon;
    public GameObject seraphimUltimateIcon;
    public GameObject shepherdUltimateIcon;
    public TextMeshProUGUI goralUltimateCD;
    public TextMeshProUGUI seraphimUltimateCD;
    public TextMeshProUGUI shepherdUltimateCD;

    [Header("═══════════════ SYSTEMY ═══════════════")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        SetActiveAll(false);
        HideAllIcons();

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

    void HideAllIcons()
    {
        // Special (Q)
        if (goralSpecialIcon != null) goralSpecialIcon.SetActive(false);
        if (seraphimSpecialIcon != null) seraphimSpecialIcon.SetActive(false);
        if (shepherdSpecialIcon != null) shepherdSpecialIcon.SetActive(false);
        if (goralSpecialCD != null) goralSpecialCD.gameObject.SetActive(false);
        if (seraphimSpecialCD != null) seraphimSpecialCD.gameObject.SetActive(false);
        if (shepherdSpecialCD != null) shepherdSpecialCD.gameObject.SetActive(false);

        // Ultimate (R)
        if (goralUltimateIcon != null) goralUltimateIcon.SetActive(false);
        if (seraphimUltimateIcon != null) seraphimUltimateIcon.SetActive(false);
        if (shepherdUltimateIcon != null) shepherdUltimateIcon.SetActive(false);
        if (goralUltimateCD != null) goralUltimateCD.gameObject.SetActive(false);
        if (seraphimUltimateCD != null) seraphimUltimateCD.gameObject.SetActive(false);
        if (shepherdUltimateCD != null) shepherdUltimateCD.gameObject.SetActive(false);
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

        // ============================================================
        // WŁĄCZ TYLKO ODPOWIEDNIE IKONY
        // ============================================================
        HideAllIcons();

        if (name == "Góral" || name == "Goral")
        {
            if (goralSpecialIcon != null) goralSpecialIcon.SetActive(true);
            if (goralUltimateIcon != null) goralUltimateIcon.SetActive(true);
            if (goralSpecialCD != null) goralSpecialCD.gameObject.SetActive(true);
            if (goralUltimateCD != null) goralUltimateCD.gameObject.SetActive(true);
        }
        else if (name == "Seraphim")
        {
            if (seraphimSpecialIcon != null) seraphimSpecialIcon.SetActive(true);
            if (seraphimUltimateIcon != null) seraphimUltimateIcon.SetActive(true);
            if (seraphimSpecialCD != null) seraphimSpecialCD.gameObject.SetActive(true);
            if (seraphimUltimateCD != null) seraphimUltimateCD.gameObject.SetActive(true);
        }
        else if (name == "Pasterz" || name == "Shepherd")
        {
            if (shepherdSpecialIcon != null) shepherdSpecialIcon.SetActive(true);
            if (shepherdUltimateIcon != null) shepherdUltimateIcon.SetActive(true);
            if (shepherdSpecialCD != null) shepherdSpecialCD.gameObject.SetActive(true);
            if (shepherdUltimateCD != null) shepherdUltimateCD.gameObject.SetActive(true);
        }

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
        HideAllIcons();
        if (characterSelectionPanel != null)
            characterSelectionPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("🔄 Zresetowano wybór postaci");
    }
}