using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event System.Action OnInventoryChanged;

    public enum InventoryItemType
    {
        HealingPotion,
        Weapon
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

        Debug.Log("InventoryManager active instance: " + gameObject.name);
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

        InventoryItemStack existingStack = FindItemStack(itemName);

        if (existingStack != null)
        {
            existingStack.quantity += amount;

            Debug.Log("Received " + itemName + " x" + amount + ".");
            OnInventoryChanged?.Invoke();

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
        OnInventoryChanged?.Invoke();
    }

    public bool ConsumeItem(string itemName)
    {
        InventoryItemStack itemStack = FindItemStack(itemName);

        if (itemStack == null || itemStack.quantity <= 0)
        {
            Debug.LogWarning("Cannot use " + itemName + ": item is not in inventory.");
            return false;
        }

        itemStack.quantity--;

        Debug.Log("Used " + itemStack.itemName + ". Remaining: " + itemStack.quantity);

        if (itemStack.quantity <= 0)
        {
            inventoryItems.Remove(itemStack);
            Debug.Log(itemStack.itemName + " removed from inventory.");
        }

        OnInventoryChanged?.Invoke();

        return true;
    }

    public int GetItemQuantity(string itemName)
    {
        InventoryItemStack itemStack = FindItemStack(itemName);

        return itemStack != null ? itemStack.quantity : 0;
    }

    public List<Item> CreateBattleItems()
    {
        List<Item> battleItems = new List<Item>();

        Debug.Log("Creating battle items from inventory. Stack count: " + inventoryItems.Count);

        foreach (InventoryItemStack inventoryItem in inventoryItems)
        {
            if (inventoryItem == null)
            {
                continue;
            }

            if (inventoryItem.quantity <= 0)
            {
                Debug.Log("Skipping " + inventoryItem.itemName + " because quantity is 0.");
                continue;
            }

            if (inventoryItem.itemType == InventoryItemType.Weapon)
            {
                Debug.Log("Skipping weapon in battle item list: " + inventoryItem.itemName);
                continue;
            }

            Item runtimeItem = CreateRuntimeBattleItem(inventoryItem);

            if (runtimeItem != null)
            {
                battleItems.Add(runtimeItem);

                Debug.Log(
                    "Added battle item: " +
                    runtimeItem.getName() +
                    " x" +
                    inventoryItem.quantity
                );
            }
        }

        Debug.Log("Final battle item count: " + battleItems.Count);

        return battleItems;
    }

    private Item CreateRuntimeBattleItem(InventoryItemStack inventoryItem)
    {
        if (inventoryItem == null)
        {
            return null;
        }

        string targetType = inventoryItem.targetType == InventoryTargetType.Single
            ? Item.itemTypes.SINGLE.ToString()
            : Item.itemTypes.PARTY.ToString();

        switch (inventoryItem.itemType)
        {
            case InventoryItemType.HealingPotion:
            {
                HealItem healingPotion = new HealItem();

                healingPotion.Init(
                    inventoryItem.itemName,
                    targetType,
                    inventoryItem.power
                );

                return healingPotion;
            }

            case InventoryItemType.Weapon:
                return null;

            default:
                Debug.LogWarning(
                    "Inventory item type is not supported in battle: " +
                    inventoryItem.itemType
                );

                return null;
        }
    }

    private InventoryItemStack FindItemStack(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return null;
        }

        string normalizedSearchName = NormalizeItemName(itemName);

        return inventoryItems.Find(item =>
            item != null &&
            NormalizeItemName(item.itemName) == normalizedSearchName
        );
    }

    private string NormalizeItemName(string itemName)
    {
        if (string.IsNullOrWhiteSpace(itemName))
        {
            return string.Empty;
        }

        return itemName
            .Replace("[", "")
            .Replace("]", "")
            .Trim()
            .ToLowerInvariant();
    }
}