using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkIconLoader : MonoBehaviour
{
    [Header("Referencje")]
    public Transform perkContainer;     // Miejsca na przyciski (Perk1-Perk4)
    public Transform perkStorage;       // Gotowe przyciski z ikonami

    private void Start()
    {
        if (perkStorage == null)
        {
            GameObject storage = GameObject.Find("PerkStorage");
            if (storage != null) perkStorage = storage.transform;
        }
    }

    /// <summary>
    /// Kopiuje przycisk z PerkStorage do PerkContainer i ustawia nazwê/opis
    /// </summary>
    public GameObject PlacePerkButton(string perkId, string targetSlotName, string name, string description)
    {
        if (perkContainer == null || perkStorage == null) return null;

        // ZnajdŸ miejsce docelowe (np. Perk1)
        Transform targetSlot = perkContainer.Find(targetSlotName);
        if (targetSlot == null)
        {
            Debug.LogWarning("Nie znaleziono slotu: " + targetSlotName);
            return null;
        }

        // ZnajdŸ przycisk w PerkStorage
        string storageButtonName = GetStorageButtonName(perkId);
        Transform sourceButton = perkStorage.Find(storageButtonName);
        if (sourceButton == null)
        {
            Debug.LogWarning("Nie znaleziono przycisku w PerkStorage: " + storageButtonName);
            return null;
        }

        // Usuñ stare dzieci w slocie
        foreach (Transform child in targetSlot)
        {
            Destroy(child.gameObject);
        }

        // Skopiuj przycisk
        GameObject newButton = Instantiate(sourceButton.gameObject, targetSlot);
        newButton.transform.localPosition = Vector3.zero;
        newButton.transform.localScale = Vector3.one;
        newButton.transform.localRotation = Quaternion.identity;
        newButton.name = sourceButton.name;

        // ============================================================
        // USTAW NAZWÊ I OPIS AUTOMATYCZNIE
        // ============================================================
        TextMeshProUGUI nameTxt = newButton.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descTxt = newButton.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();

        if (nameTxt != null) nameTxt.text = name;
        if (descTxt != null) descTxt.text = description;

        Debug.Log("Przycisk " + storageButtonName + " wklejony do " + targetSlotName + " z nazw¹: " + name);

        return newButton;
    }

    /// <summary>
    /// Mapuje ID perka na nazwê przycisku w PerkStorage
    /// </summary>
    string GetStorageButtonName(string perkId)
    {
        switch (perkId)
        {
            // UNIWERSALNE
            case "damage": return "PerkButton_Attack";
            case "range": return "PerkButton_Range";
            case "health": return "PerkButton_Health";
            case "attackSpeed": return "PerkButton_AttackSpeed";
            case "speed": return "PerkButton_Speed";
            case "xp": return "PerkButton_XP";
            case "armor": return "PerkButton_Armor";
            case "bleed": return "PerkButton_Bleed";
            case "shield": return "PerkButton_Shield";
            case "vampire": return "PerkButton_Vampire";
            case "ultimateDuration": return "PerkButton_UltimateDuration";
            case "ultimateDamage": return "PerkButton_UltimateDamage";

            // GÓRAL
            case "goral_mocnyCios": return "PerkButton_GoralMocnyCios";
            case "goral_ziemia": return "PerkButton_GoralZiemia";
            case "goral_wytrzymalosc": return "PerkButton_GoralWytrzymalosc";
            case "goral_stompTime": return "PerkButton_GoralStompCzas";
            case "goral_specialTime": return "PerkButton_GoralSpecialCzas";
            case "goral_ultimateTime": return "PerkButton_GoralUltimateCzas";
            case "goral_specialCD": return "PerkButton_GoralSpecialCD";

            // SERAPHIM
            case "seraphim_swiatlo": return "PerkButton_SeraphimSwiatlo";
            case "seraphim_uzdrowienie": return "PerkButton_SeraphimUzdrowienie";
            case "seraphim_aniol": return "PerkButton_SeraphimAniol";
            case "seraphim_pierce": return "PerkButton_SeraphimPrzebicie";

            // PASTERZ
            case "shepherd_owca": return "PerkButton_ShepherdOwca";
            case "shepherd_stado": return "PerkButton_ShepherdStado";
            case "shepherd_pasterz": return "PerkButton_ShepherdPasterz";

            default: return null;
        }
    }

    /// <summary>
    /// Czyœci wszystkie sloty w PerkContainer
    /// </summary>
    public void ClearSlots()
    {
        if (perkContainer == null) return;

        foreach (Transform slot in perkContainer)
        {
            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }
    }
}