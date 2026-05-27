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
    [SerializeField] private float flashOutDuration = 0.45f;

    [Header("Player Upgrade")]
    [SerializeField] private KaelTopDownController playerController;

    private bool playerInRange = false;
    private bool opened = false;
    private bool sequenceRunning = false;

    private void Start()
    {
        if (risingSwordRenderer != null)
            risingSwordRenderer.enabled = false;

        if (whiteFlashGroup != null)
            whiteFlashGroup.alpha = 0f;
    }

    private void Update()
    {
        if (opened || sequenceRunning)
            return;

        if (!playerInRange)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(OpenChestSequence());
        }
    }

    private IEnumerator OpenChestSequence()
    {
        sequenceRunning = true;
        opened = true;

        yield return StartCoroutine(RiseSword());

        yield return StartCoroutine(WhiteFlash());

        SwapBackgroundToOpenChest();
        GiveKaelArmorAndSword();

        if (risingSwordRenderer != null)
            risingSwordRenderer.enabled = false;

        sequenceRunning = false;

        Debug.Log("Kael received the armor and holy sword.");
    }

    private IEnumerator RiseSword()
    {
        if (risingSwordRenderer == null || risingSwordTransform == null)
            yield break;

        risingSwordRenderer.enabled = true;

        Vector3 startPosition = risingSwordTransform.position;
        Vector3 endPosition = startPosition + new Vector3(0f, swordRiseDistance, 0f);

        float timer = 0f;

        while (timer < swordRiseDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / swordRiseDuration);
            float easedT = Mathf.SmoothStep(0f, 1f, t);

            risingSwordTransform.position = Vector3.Lerp(startPosition, endPosition, easedT);

            yield return null;
        }

        risingSwordTransform.position = endPosition;

        yield return new WaitForSeconds(swordHoldDuration);
    }

    private IEnumerator WhiteFlash()
    {
        if (whiteFlashGroup == null)
            yield break;

        float timer = 0f;

        while (timer < flashInDuration)
        {
            timer += Time.deltaTime;
            whiteFlashGroup.alpha = Mathf.Clamp01(timer / flashInDuration);
            yield return null;
        }

        whiteFlashGroup.alpha = 1f;

        // Swap while the screen is fully white.
        SwapBackgroundToOpenChest();
        GiveKaelArmorAndSword();

        yield return new WaitForSeconds(0.08f);

        timer = 0f;

        while (timer < flashOutDuration)
        {
            timer += Time.deltaTime;
            whiteFlashGroup.alpha = 1f - Mathf.Clamp01(timer / flashOutDuration);
            yield return null;
        }

        whiteFlashGroup.alpha = 0f;
    }

    private void SwapBackgroundToOpenChest()
    {
        if (basementBackgroundRenderer != null && openChestBackground != null)
            basementBackgroundRenderer.sprite = openChestBackground;
    }

    private void GiveKaelArmorAndSword()
    {
        if (playerController != null)
            playerController.SwitchToArmoredSprites();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Press E to inspect the chest.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}