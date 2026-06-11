using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BasementHolyChest : MonoBehaviour
{
    [Header("Background Swap")]
    [SerializeField] private SpriteRenderer basementBackgroundRenderer;
    [SerializeField] private Sprite openChestBackground;

    [Header("Sword Rise Animation")]
    [SerializeField] private SpriteRenderer risingSwordRenderer;
    [SerializeField] private Transform risingSwordTransform;
    [SerializeField] private float swordRiseDistance = 1.25f;
    [SerializeField] private float swordRiseDuration = 1.2f;
    [SerializeField] private float swordHoldDuration = 0.4f;

    [Header("White Flash")]
    [SerializeField] private CanvasGroup whiteFlashGroup;
    [SerializeField] private float flashInDuration = 0.15f;
    [SerializeField] private float flashHoldDuration = 0.08f;
    [SerializeField] private float flashOutDuration = 0.45f;
    
    [Header("Inventory Reward")]
    [SerializeField] private string holySwordItemName = "Holy Sword";
    [SerializeField] private int holySwordPower = 50;
    [SerializeField] private int holySwordQuantity = 1;

    [Header("Player Upgrade")]
    [SerializeField] private KaelTopDownController playerController;

    [Header("Dialogue After Chest")]
    [SerializeField] private BasementDialogueSequence basementDialogueSequence;

    private bool playerInRange;
    public bool opened;
    private bool sequenceRunning;

    private void Start()
    {
        if (risingSwordRenderer != null)
        {
            risingSwordRenderer.enabled = false;
        }

        if (whiteFlashGroup != null)
        {
            whiteFlashGroup.gameObject.SetActive(true);
            whiteFlashGroup.alpha = 0f;
            whiteFlashGroup.interactable = false;
            whiteFlashGroup.blocksRaycasts = false;
        }

        if (StoryProgress.HolyChestOpened)
        {
            opened = true;
            RestoreOpenedChestState();
        }
    }

    private void Update()
    {
        if (opened || sequenceRunning || !playerInRange)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(OpenChestSequence());
        }
    }

    private IEnumerator OpenChestSequence()
    {
        sequenceRunning = true;
        opened = true;

        Debug.Log("Chest sequence started.");

        yield return StartCoroutine(RiseSword());
        yield return StartCoroutine(WhiteFlash());

        if (risingSwordRenderer != null)
        {
            risingSwordRenderer.enabled = false;
        }

        StoryProgress.MarkHolyChestOpened();
        AddHolySwordToInventory();

        sequenceRunning = false;

        if (basementDialogueSequence != null)
        {
            basementDialogueSequence.PlayAfterChestDialogue();
        }
        else
        {
            Debug.LogWarning("BasementDialogueSequence is not assigned on BasementHolyChest.");
        }

        Debug.Log("Kael received the armor and holy sword.");
    }

    private void RestoreOpenedChestState()
    {
        if (basementBackgroundRenderer != null && openChestBackground != null)
        {
            basementBackgroundRenderer.sprite = openChestBackground;
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<KaelTopDownController>();
        }

        if (playerController != null)
        {
            playerController.SwitchToArmoredSprites();
        }

        if (risingSwordRenderer != null)
        {
            risingSwordRenderer.enabled = false;
        }

        Debug.Log("Restored already-opened holy chest state.");
    }

    private IEnumerator RiseSword()
    {
        if (risingSwordRenderer == null || risingSwordTransform == null)
        {
            Debug.LogWarning("Rising sword renderer or transform is not assigned.");
            yield break;
        }

        risingSwordRenderer.enabled = true;

        Vector3 startPosition = risingSwordTransform.position;
        Vector3 endPosition = startPosition + Vector3.up * swordRiseDistance;

        float timer = 0f;

        while (timer < swordRiseDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / swordRiseDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            risingSwordTransform.position =
                Vector3.Lerp(startPosition, endPosition, easedT);

            yield return null;
        }

        risingSwordTransform.position = endPosition;

        yield return new WaitForSeconds(swordHoldDuration);
    }

    private IEnumerator WhiteFlash()
    {
        if (whiteFlashGroup == null)
        {
            Debug.LogWarning("WhiteFlash CanvasGroup is not assigned. Applying upgrade without flash.");
            SwapBackgroundAndUpgradePlayer();
            yield break;
        }

        whiteFlashGroup.gameObject.SetActive(true);
        whiteFlashGroup.alpha = 0f;

        float timer = 0f;

        while (timer < flashInDuration)
        {
            timer += Time.deltaTime;
            whiteFlashGroup.alpha = Mathf.Clamp01(timer / flashInDuration);
            yield return null;
        }

        whiteFlashGroup.alpha = 1f;

        SwapBackgroundAndUpgradePlayer();

        yield return new WaitForSeconds(flashHoldDuration);

        timer = 0f;

        while (timer < flashOutDuration)
        {
            timer += Time.deltaTime;
            whiteFlashGroup.alpha =
                1f - Mathf.Clamp01(timer / flashOutDuration);

            yield return null;
        }

        whiteFlashGroup.alpha = 0f;
    }

    private void SwapBackgroundAndUpgradePlayer()
    {
        if (basementBackgroundRenderer != null && openChestBackground != null)
        {
            basementBackgroundRenderer.sprite = openChestBackground;
            Debug.Log("Basement background changed to opened chest.");
        }
        else
        {
            Debug.LogWarning("Basement background renderer or opened chest sprite is not assigned.");
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<KaelTopDownController>();
        }

        if (playerController != null)
        {
            playerController.SwitchToArmoredSprites();
            Debug.Log("Kael switched to armored sprites.");
        }
        else
        {
            Debug.LogWarning("PlayerController is not assigned on BasementHolyChest.");
        }
    }

    private void AddHolySwordToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Cannot add Holy Sword because InventoryManager is missing.");
            return;
        }

        if (InventoryManager.Instance.GetItemQuantity(holySwordItemName) > 0)
        {
            Debug.Log("Holy Sword is already in inventory.");
            return;
        }

        InventoryManager.Instance.AddItem(
            holySwordItemName,
            InventoryManager.InventoryItemType.Weapon,
            InventoryManager.InventoryTargetType.Single,
            holySwordPower,
            holySwordQuantity
        );

        Debug.Log("Holy Sword added to inventory.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Press E to inspect the chest.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}