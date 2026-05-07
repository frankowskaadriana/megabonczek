using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("Postacie w hierarchii")]
    public GameObject mountainMan;
    public GameObject seraphim;
    public GameObject character3;

    [Header("Kamera")]
    public CameraController cameraController;

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

        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCharacter(mountainMan, "Mountain Man");
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCharacter(seraphim, "Seraphim");
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCharacter(character3, "Character3");
    }

    void SelectCharacter(GameObject character, string name)
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

        if (currentCharacter.GetComponent<PlayerMovement>() == null)
            currentCharacter.AddComponent<PlayerMovement>();

        if (currentCharacter.GetComponent<PlayerHealth>() == null)
            currentCharacter.AddComponent<PlayerHealth>();

        hasSelected = true;
        Debug.Log("Aktywny: " + name + " - wybor zablokowany");
    }
}