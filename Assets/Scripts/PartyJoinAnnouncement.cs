using System.Collections;
using TMPro;
using UnityEngine;

public class PartyJoinAnnouncement : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup blackPanelGroup;
    [SerializeField] private TMP_Text announcementText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private float holdDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private Coroutine activeAnnouncement;

    private void Awake()
    {
        if (blackPanelGroup == null)
        {
            Debug.LogWarning("PartyJoinAnnouncement is missing the Black Panel Group reference.");
            return;
        }

        blackPanelGroup.gameObject.SetActive(true);
        blackPanelGroup.alpha = 0f;
        blackPanelGroup.blocksRaycasts = false;
        blackPanelGroup.interactable = false;
    }

    public IEnumerator ShowAnnouncement(string message)
    {
        Debug.Log("Starting party announcement: " + message);

        if (blackPanelGroup == null)
        {
            Debug.LogWarning("Cannot show announcement: Black Panel Group is not assigned.");
            yield break;
        }

        if (announcementText == null)
        {
            Debug.LogWarning("Cannot show announcement: Announcement Text is not assigned.");
            yield break;
        }

        blackPanelGroup.gameObject.SetActive(true);
        announcementText.gameObject.SetActive(true);
        announcementText.text = message;

        blackPanelGroup.blocksRaycasts = true;
        blackPanelGroup.interactable = true;

        yield return FadeCanvasGroup(0f, 1f, fadeInDuration);

        Debug.Log("Party announcement visible.");

        yield return new WaitForSecondsRealtime(holdDuration);

        yield return FadeCanvasGroup(1f, 0f, fadeOutDuration);

        blackPanelGroup.alpha = 0f;
        blackPanelGroup.blocksRaycasts = false;
        blackPanelGroup.interactable = false;

        Debug.Log("Party announcement finished.");
    }

    private IEnumerator FadeCanvasGroup(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            blackPanelGroup.alpha = endAlpha;
            yield break;
        }

        float timer = 0f;
        blackPanelGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / duration);
            blackPanelGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            yield return null;
        }

        blackPanelGroup.alpha = endAlpha;
    }

    public void TestAnnouncement()
    {
        if (activeAnnouncement != null)
        {
            StopCoroutine(activeAnnouncement);
        }

        activeAnnouncement = StartCoroutine(
            ShowAnnouncement("Paladin has joined your party.")
        );
    }
}