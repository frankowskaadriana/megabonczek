using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("═══════════════ POSTACIE ═══════════════")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject shepherd;

    [Header("═══════════════ SYSTEMY ═══════════════")]
    public CameraController cameraController;
    public LevelSystem levelSystem;
    public PlayerStats playerStats;

    private GameObject currentCharacter;
    private bool hasSelected = false;

    void Start()
    {
        SetActiveAll(false);

        // KURSOR ZAWSZE WIDOCZNY I ODBLOKOWANY
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic();
        }

        Debug.Log("🎮 Wybierz postać: 1 - Góral, 2 - Seraphim, 3 - Pasterz");
    }

    void Update()
    {
        if (hasSelected) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectCharacter(mountainMan, "Góral");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectCharacter(seraphim, "Seraphim");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectCharacter(shepherd, "Pasterz");
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log($"🔍 Góral aktywny: {(mountainMan != null ? mountainMan.activeSelf : false)}");
            Debug.Log($"🔍 Seraphim aktywny: {(seraphim != null ? seraphim.activeSelf : false)}");
            Debug.Log($"🔍 Pasterz aktywny: {(shepherd != null ? shepherd.activeSelf : false)}");
            Debug.Log($"🔍 hasSelected: {hasSelected}");
        }
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
            Debug.LogError($"❌ {name} nie jest przypisany w Inspectorze!");
            return;
        }

        SetActiveAll(false);

        currentCharacter = character;
        currentCharacter.SetActive(true);
        currentCharacter.tag = "Player";

        Debug.Log($"✅ Aktywowano: {name}");

        if (cameraController != null)
        {
            cameraController.SetTarget(currentCharacter.transform);
            Debug.Log("📷 Kamera ustawiona na gracza");
        }

        AddMissingComponents(currentCharacter, name);

        PlayerHealth health = currentCharacter.GetComponent<PlayerHealth>();
        if (health == null)
        {
            health = currentCharacter.AddComponent<PlayerHealth>();
            Debug.Log("❤️ Dodano PlayerHealth");
        }

        PlayerMovement movement = currentCharacter.GetComponent<PlayerMovement>();
        if (movement == null)
        {
            movement = currentCharacter.AddComponent<PlayerMovement>();
            Debug.Log("🏃 Dodano PlayerMovement");
        }

        if (playerStats != null)
        {
            playerStats.AssignToPlayer(currentCharacter);
            Debug.Log("📊 Przypisano statystyki");
        }

        if (levelSystem != null)
        {
            levelSystem.StartGame();
            Debug.Log("🎮 LevelSystem rozpoczęty");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.OnCharacterSelected();
            Debug.Log("🎵 Przełączono na muzykę walki!");
        }

        hasSelected = true;

        // KURSOR ZAWSZE WIDOCZNY - NIE BLOKUJEMY!
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"🎮 Wybrano: {name}! Gra rozpoczęta.");
    }

    void AddMissingComponents(GameObject character, string name)
    {
        if (character == null) return;

        if (name == "Góral" || name == "Goral")
        {
            if (character.GetComponent<AbilitiesMountainMan>() == null)
            {
                character.AddComponent<AbilitiesMountainMan>();
                Debug.Log("⚔️ Dodano AbilitiesMountainMan");
            }
        }
        else if (name == "Seraphim")
        {
            if (character.GetComponent<AbilitiesSeraphim>() == null)
            {
                character.AddComponent<AbilitiesSeraphim>();
                Debug.Log("✨ Dodano AbilitiesSeraphim");
            }
        }
        else if (name == "Pasterz" || name == "Shepherd")
        {
            if (character.GetComponent<ShepherdAbilities>() == null)
            {
                character.AddComponent<ShepherdAbilities>();
                Debug.Log("🐕 Dodano ShepherdAbilities");
            }
        }
    }

    public GameObject GetCurrentCharacter()
    {
        return currentCharacter;
    }

    public bool HasSelected()
    {
        return hasSelected;
    }

    public void ResetSelection()
    {
        hasSelected = false;
        SetActiveAll(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("🔄 Zresetowano wybór postaci");
    }
}