using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Current Party Members")]
    [SerializeField] private List<PartyMemberData> partyMembers = new List<PartyMemberData>();

    private int currentPage = 0;

    private const int membersPerPage = 2;
    private const int maxPartyMembers = 4;

    private readonly List<PartyMemberCardUI> spawnedCards = new List<PartyMemberCardUI>();

    [System.Serializable]
    public class CharacterPortraitEntry
    {
        public string characterName;
        public Sprite portrait;
    }

    private void Awake()
    {
        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(GoToPreviousPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(GoToNextPage);
    }

    public void LoadPartyFromPrefabs(List<GameObject> partyPrefabs)
    {
        partyMembers.Clear();

        if (partyPrefabs == null)
        {
            RefreshMenu();
            return;
        }

        int count = Mathf.Min(partyPrefabs.Count, maxPartyMembers);

        for (int i = 0; i < count; i++)
        {
            GameObject prefab = partyPrefabs[i];

            if (prefab == null)
                continue;

            PlayerBattle battleData = prefab.GetComponent<PlayerBattle>();

            if (battleData == null)
            {
                Debug.LogWarning(prefab.name + " does not have PlayerBattle, so it cannot be shown in the party menu.");
                continue;
            }

            int currentHP = battleData.CurrentHealth;
            int maxHP = battleData.MaxHealth;

            if (PartyManager.Instance != null &&
                PartyManager.Instance.TryGetPartyHealth(
                    battleData.entityName,
                    out int storedCurrentHP,
                    out int storedMaxHP
                ))
            {
                currentHP = storedCurrentHP;
                maxHP = storedMaxHP;
            }

            PartyMemberData memberData = new PartyMemberData
            {
                characterName = battleData.entityName,
                characterClass = GetClassNameFromPrefabName(prefab.name),
                characterPortrait = GetPortraitForCharacter(battleData.entityName),
                currentHP = currentHP,
                maxHP = maxHP
            };

            partyMembers.Add(memberData);
        }

        currentPage = 0;
        RefreshMenu();
    }

    public void RefreshMenu()
    {
        currentPage = Mathf.Clamp(currentPage, 0, GetMaxPageIndex());

        ShowCorrectPage();
        ClearSpawnedCards();
        SpawnCardsForCurrentPage();
        UpdateNavigationButtons();
    }

    private void SpawnCardsForCurrentPage()
    {
        if (partyCardPrefab == null)
        {
            Debug.LogError("PartyMenuPageManager: Party Card Prefab is not assigned.");
            return;
        }

        Transform[] currentPageSlots = GetSlotsForCurrentPage();

        int startIndex = currentPage * membersPerPage;
        int endIndex = Mathf.Min(startIndex + membersPerPage, partyMembers.Count);

        int slotIndex = 0;

        for (int memberIndex = startIndex; memberIndex < endIndex; memberIndex++)
        {
            if (slotIndex >= currentPageSlots.Length)
                break;

            Transform slot = currentPageSlots[slotIndex];

            if (slot == null)
            {
                Debug.LogWarning("PartyMenuPageManager: Missing slot on page " + (currentPage + 1));
                slotIndex++;
                continue;
            }

            PartyMemberCardUI card = Instantiate(partyCardPrefab);

            RectTransform cardRect = card.GetComponent<RectTransform>();
            RectTransform slotRect = slot.GetComponent<RectTransform>();

            if (cardRect != null && slotRect != null)
            {
                cardRect.SetParent(slotRect, false);

                cardRect.anchorMin = Vector2.zero;
                cardRect.anchorMax = Vector2.one;
                cardRect.offsetMin = Vector2.zero;
                cardRect.offsetMax = Vector2.zero;
                cardRect.localScale = Vector3.one;
                cardRect.localRotation = Quaternion.identity;
                cardRect.anchoredPosition = Vector2.zero;
            }
            else
            {
                card.transform.SetParent(slot, false);
                card.transform.localPosition = Vector3.zero;
                card.transform.localScale = Vector3.one;
                card.transform.localRotation = Quaternion.identity;
            }

            card.Setup(partyMembers[memberIndex]);
            spawnedCards.Add(card);

            slotIndex++;
        }
    }

    private Transform[] GetSlotsForCurrentPage()
    {
        if (currentPage == 0)
        {
            return new Transform[]
            {
                page1Slot1,
                page1Slot2
            };
        }

        return new Transform[]
        {
            page2Slot1,
            page2Slot2
        };
    }

    private void ShowCorrectPage()
    {
        if (page1 != null)
            page1.SetActive(currentPage == 0);

        if (page2 != null)
            page2.SetActive(currentPage == 1);
    }

    public void GoToNextPage()
    {
        int maxPage = GetMaxPageIndex();

        if (currentPage >= maxPage)
            return;

        currentPage++;
        RefreshMenu();
    }

    public void GoToPreviousPage()
    {
        if (currentPage <= 0)
            return;

        currentPage--;
        RefreshMenu();
    }

    private int GetMaxPageIndex()
    {
        if (partyMembers.Count <= 2)
            return 0;

        return 1;
    }

    private void UpdateNavigationButtons()
    {
        int maxPage = GetMaxPageIndex();

        if (previousPageButton != null)
            previousPageButton.gameObject.SetActive(currentPage > 0);

        if (nextPageButton != null)
            nextPageButton.gameObject.SetActive(currentPage < maxPage);
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

            if (string.Equals(
                entry.characterName,
                characterName,
                System.StringComparison.OrdinalIgnoreCase
            ))
            {
                return entry.portrait;
            }
        }

        return null;
    }
}