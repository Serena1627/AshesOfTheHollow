using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class CharacterPortraitEntry
{
    public string characterName;
    public Sprite portrait;
}

public class PartyMenuPageManager : MonoBehaviour
{
    [Header("Card Prefab")]
    [SerializeField] private PartyMemberCardUI partyCardPrefab;

    [Header("Pages")]
    [SerializeField] private GameObject page1;
    [SerializeField] private GameObject page2;

    [Header("Page 1 Slots")]
    [SerializeField] private Transform page1Slot1;
    [SerializeField] private Transform page1Slot2;

    [Header("Page 2 Slots")]
    [SerializeField] private Transform page2Slot1;
    [SerializeField] private Transform page2Slot2;

    [Header("Navigation")]
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;

    [Header("Portrait Lookup")]
    [SerializeField] private List<CharacterPortraitEntry> portraitEntries = new List<CharacterPortraitEntry>();

    [Header("Temporary Party Members")]
    [SerializeField] private List<PartyMemberData> partyMembers = new List<PartyMemberData>();

    private int currentPage = 0;
    private int membersPerPage = 2;
    private int maxPartyMembers = 4;

    private readonly List<PartyMemberCardUI> spawnedCards = new List<PartyMemberCardUI>();

    private void Awake()
    {
        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(GoToPreviousPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(GoToNextPage);
    }

    private void OnEnable()
    {
        RefreshMenu();
    }

    public void RefreshMenu()
    {
        if (partyMembers.Count > maxPartyMembers)
        {
            Debug.LogWarning("Party has more than 4 members. Only the first 4 will be shown.");
        }

        currentPage = Mathf.Clamp(currentPage, 0, GetMaxPageIndex());

        ClearSpawnedCards();
        SpawnCards();
        ShowCurrentPage();
        UpdateNavigationButtons();
    }
    
    private void SpawnCards()
    {
        if (partyCardPrefab == null)
        {
            Debug.LogError("PartyMenuPageManager: Party Card Prefab is not assigned.");
            return;
        }

        Transform[] slots =
        {
            page1Slot1,
            page1Slot2,
            page2Slot1,
            page2Slot2
        };

        int count = Mathf.Min(partyMembers.Count, maxPartyMembers);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null)
            {
                Debug.LogWarning("Missing slot assignment for party member index " + i);
                continue;
            }

            PartyMemberCardUI card = Instantiate(partyCardPrefab);

            RectTransform cardRect = card.GetComponent<RectTransform>();
            RectTransform slotRect = slots[i].GetComponent<RectTransform>();

            cardRect.SetParent(slotRect, false);

            cardRect.anchorMin = Vector2.zero;
            cardRect.anchorMax = Vector2.one;
            cardRect.pivot = new Vector2(0.5f, 0.5f);

            cardRect.anchoredPosition = Vector2.zero;
            cardRect.sizeDelta = Vector2.zero;

            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            cardRect.localScale = Vector3.one;
            cardRect.localRotation = Quaternion.identity;

            card.Setup(partyMembers[i]);
            spawnedCards.Add(card);
        }
    }

    private void ShowCurrentPage()
    {
        if (page1 != null)
            page1.SetActive(currentPage == 0);

        if (page2 != null)
            page2.SetActive(currentPage == 1);
    }

    private void UpdateNavigationButtons()
    {
        int maxPage = GetMaxPageIndex();

        if (previousPageButton != null)
            previousPageButton.gameObject.SetActive(currentPage > 0);

        if (nextPageButton != null)
            nextPageButton.gameObject.SetActive(currentPage < maxPage);

    }

    private int GetMaxPageIndex()
    {
        if (partyMembers.Count <= 2)
            return 0;

        return 1;
    }

    public void GoToNextPage()
    {
        int maxPage = GetMaxPageIndex();

        if (currentPage < maxPage)
        {
            currentPage++;
            ShowCurrentPage();
            UpdateNavigationButtons();
        }
    }

    public void GoToPreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowCurrentPage();
            UpdateNavigationButtons();
        }
    }

    private void ClearSpawnedCards()
    {
        foreach (PartyMemberCardUI card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();

        ClearSlot(page1Slot1);
        ClearSlot(page1Slot2);
        ClearSlot(page2Slot1);
        ClearSlot(page2Slot2);
    }

    private void ClearSlot(Transform slot)
    {
        if (slot == null)
            return;

        for (int i = slot.childCount - 1; i >= 0; i--)
        {
            Destroy(slot.GetChild(i).gameObject);
        }
    }

    public void LoadPartyFromPrefabs(List<GameObject> partyPrefabs)
    {
        partyMembers.Clear();

        Debug.Log("PartyMenuPageManager: LoadPartyFromPrefabs called.");

        if (partyPrefabs == null)
        {
            Debug.LogWarning("PartyMenuPageManager: partyPrefabs is null.");
            RefreshMenu();
            return;
        }
        
        Debug.Log("PartyMenuPageManager: partyPrefabs count = " + partyPrefabs.Count);
        int count = Mathf.Min(partyPrefabs.Count, maxPartyMembers);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = partyPrefabs[i];

            if (prefab == null){
                Debug.LogWarning("PartyMenuPageManager: prefab at index " + i + " is null.");
                continue;
            }

            PlayerBattle battleData = prefab.GetComponent<PlayerBattle>();

            if (battleData == null)
            {
                Debug.LogWarning(prefab.name + " does not have PlayerBattle, so it cannot be shown in the party menu.");
                continue;
            }

            Debug.Log("PartyMenuPageManager: Found PlayerBattle entityName = " + battleData.entityName);
            int currentHP = battleData.CurrentHealth;
            int maxHP = battleData.MaxHealth;

            if (PartyManager.Instance != null &&
                PartyManager.Instance.TryGetPartyHealth(battleData.entityName, out int storedCurrentHP, out int storedMaxHP))
            {
                currentHP = storedCurrentHP;
                maxHP = storedMaxHP;
                Debug.Log("PartyMenuPageManager: Stored HP found for " + battleData.entityName + ": " + currentHP + " / " + maxHP);
            }
            else
            {
                Debug.Log("PartyMenuPageManager: No stored HP found. Using PlayerBattle HP.");
            }

            PartyMemberData memberData = new PartyMemberData
            {
                characterName = battleData.entityName,
                characterClass = GetClassNameFromPrefabName(prefab.name),
                characterPortrait = GetPortraitForCharacter(battleData.entityName),
                currentHP = currentHP,
                maxHP = maxHP
            };

            Debug.Log("PartyMenuPageManager: Created memberData for " + memberData.characterName);

            partyMembers.Add(memberData);
        }

        currentPage = 0;
        RefreshMenu();
    }

    private string GetClassNameFromPrefabName(string prefabName)
    {
        string lowerName = prefabName.ToLower();

        if (lowerName.Contains("kael"))
            return "Swordsman";

        if (lowerName.Contains("mira"))
            return "Mage";

        if (lowerName.Contains("paladin") || lowerName.Contains("rowan"))
            return "Paladin";

        if (lowerName.Contains("sister") || lowerName.Contains("elen"))
            return "Priest";

        if (lowerName.Contains("thief") || lowerName.Contains("vey"))
            return "Rogue";

        if (lowerName.Contains("archer") || lowerName.Contains("liora"))
            return "Archer";

        return "Adventurer";
    }

    private Sprite GetPortraitForCharacter(string characterName)
    {
        foreach (CharacterPortraitEntry entry in portraitEntries)
        {
            if (entry == null)
                continue;

            if (string.Equals(entry.characterName, characterName, System.StringComparison.OrdinalIgnoreCase))
                return entry.portrait;
        }

        return null;
    }
}