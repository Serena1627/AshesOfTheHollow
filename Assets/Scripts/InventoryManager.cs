using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public enum InventoryItemType
    {
        HealingPotion
    }

    public enum InventoryTargetType
    {
        Single,
        Party
    }

    [System.Serializable]
    public class InventoryItemStack
    {
        public string itemName;
        public InventoryItemType itemType;
        public InventoryTargetType targetType;

        [Tooltip("For healing items, this is the amount of HP restored.")]
        public int power = 5;

        public int quantity = 1;
    }

    [Header("Current Inventory")]
    [SerializeField] private List<InventoryItemStack> inventoryItems =
        new List<InventoryItemStack>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<InventoryItemStack> GetInventoryItems()
    {
        return new List<InventoryItemStack>(inventoryItems);
    }

    public void AddItem(
        string itemName,
        InventoryItemType itemType,
        InventoryTargetType targetType,
        int power,
        int amount = 1
    )
    {
        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0)
        {
            return;
        }

        InventoryItemStack existingStack =
            inventoryItems.Find(item => item.itemName == itemName);

        if (existingStack != null)
        {
            existingStack.quantity += amount;
            Debug.Log("Received " + itemName + " x" + amount + ".");
            return;
        }

        InventoryItemStack newStack = new InventoryItemStack
        {
            itemName = itemName,
            itemType = itemType,
            targetType = targetType,
            power = power,
            quantity = amount
        };

        inventoryItems.Add(newStack);

        Debug.Log("Received " + itemName + " x" + amount + ".");
    }

    public bool ConsumeItem(string itemName)
    {
        InventoryItemStack itemStack =
            inventoryItems.Find(item => item.itemName == itemName);

        if (itemStack == null || itemStack.quantity <= 0)
        {
            Debug.LogWarning("Cannot use " + itemName + ": item is not in inventory.");
            return false;
        }

        itemStack.quantity--;

        Debug.Log("Used " + itemName + ". Remaining: " + itemStack.quantity);

        if (itemStack.quantity <= 0)
        {
            inventoryItems.Remove(itemStack);
        }

        return true;
    }

    public int GetItemQuantity(string itemName)
    {
        InventoryItemStack itemStack =
            inventoryItems.Find(item => item.itemName == itemName);

        return itemStack != null ? itemStack.quantity : 0;
    }

    public List<Item> CreateBattleItems()
    {
        List<Item> battleItems = new List<Item>();

        foreach (InventoryItemStack inventoryItem in inventoryItems)
        {
            if (inventoryItem == null || inventoryItem.quantity <= 0)
            {
                continue;
            }

            Item runtimeItem = CreateRuntimeBattleItem(inventoryItem);

            if (runtimeItem != null)
            {
                battleItems.Add(runtimeItem);
            }
        }

        return battleItems;
    }

    private Item CreateRuntimeBattleItem(InventoryItemStack inventoryItem)
    {
        string targetType = inventoryItem.targetType == InventoryTargetType.Single
            ? Item.itemTypes.SINGLE.ToString()
            : Item.itemTypes.PARTY.ToString();

        switch (inventoryItem.itemType)
        {
            case InventoryItemType.HealingPotion:
                HealItem healingPotion = new HealItem();

                healingPotion.Init(
                    inventoryItem.itemName,
                    targetType,
                    inventoryItem.power
                );

                return healingPotion;

            default:
                Debug.LogWarning(
                    "Inventory item type is not supported in battle: " +
                    inventoryItem.itemType
                );

                return null;
        }
    }
}