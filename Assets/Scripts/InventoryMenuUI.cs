using System.Collections.Generic;
using UnityEngine;

public class InventoryMenuUI : MonoBehaviour
{
    [System.Serializable]
    public class InventoryIconEntry
    {
        public string itemName;
        public Sprite icon;
    }

    [Header("Slots")]
    [SerializeField] private List<InventorySlotUI> slots =
        new List<InventorySlotUI>();

    [Header("Icon Lookup")]
    [SerializeField] private List<InventoryIconEntry> iconEntries =
        new List<InventoryIconEntry>();

    private void OnEnable()
    {
        RefreshInventoryUI();

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshInventoryUI;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryUI;
    }

    public void RefreshInventoryUI()
    {
        foreach (InventorySlotUI slot in slots)
        {
            if (slot != null)
                slot.ClearSlot();
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryMenuUI: InventoryManager.Instance is missing.");
            return;
        }

        List<InventoryManager.InventoryItemStack> inventoryItems =
            InventoryManager.Instance.GetInventoryItems();

        int count = Mathf.Min(inventoryItems.Count, slots.Count);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null)
                continue;

            InventoryManager.InventoryItemStack itemStack = inventoryItems[i];

            Sprite icon = GetIconForItem(itemStack.itemName);

            slots[i].SetItem(itemStack, icon);
        }
    }

    private Sprite GetIconForItem(string itemName)
    {
        string normalizedItemName = NormalizeName(itemName);

        foreach (InventoryIconEntry entry in iconEntries)
        {
            if (entry == null)
                continue;

            if (NormalizeName(entry.itemName) == normalizedItemName)
                return entry.icon;
        }

        return null;
    }

    private string NormalizeName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
            return "";

        return itemName
            .Replace("[", "")
            .Replace("]", "")
            .Trim()
            .ToLowerInvariant();
    }
}