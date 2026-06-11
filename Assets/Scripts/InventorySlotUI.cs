using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image itemIconImage;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemStatsText;

    public void SetItem(
        InventoryManager.InventoryItemStack itemStack,
        Sprite itemIcon
    )
    {
        if (itemStack == null)
        {
            ClearSlot();
            return;
        }

        gameObject.SetActive(true);

        if (itemIconImage != null)
        {
            itemIconImage.enabled = itemIcon != null;
            itemIconImage.sprite = itemIcon;
        }

        if (itemNameText != null)
            itemNameText.text = itemStack.itemName.ToUpper();

        if (itemStatsText != null)
        {
            itemStatsText.text =
                "POWER: " + itemStack.power +
                "\nQUANTITY: " + itemStack.quantity;
        }
    }

    public void ClearSlot()
    {
        gameObject.SetActive(false);

        if (itemIconImage != null)
        {
            itemIconImage.sprite = null;
            itemIconImage.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = "";

        if (itemStatsText != null)
            itemStatsText.text = "";
    }
}