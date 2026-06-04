using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private string itemName = "[Health Potion]";
    [SerializeField] private InventoryManager.InventoryItemType itemType =
        InventoryManager.InventoryItemType.HealingPotion;
    [SerializeField] private InventoryManager.InventoryTargetType targetType =
        InventoryManager.InventoryTargetType.Single;
    [SerializeField] private int power = 10;
    [SerializeField] private int quantity = 1;

    [Header("Pickup Settings")]
    [SerializeField] private bool destroyAfterPickup = true;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Item could not be collected because InventoryManager is missing.");
            return;
        }

        collected = true;

        InventoryManager.Instance.AddItem(
            itemName,
            itemType,
            targetType,
            power,
            quantity
        );

        if (destroyAfterPickup)
        {
            Destroy(gameObject);
        }
    }
}