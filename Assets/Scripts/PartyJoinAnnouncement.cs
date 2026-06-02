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

    private void Start()
    {
        if (blackPanelGroup != null)
        {
            blackPanelGroup.alpha = 0f;
            blackPanelGroup.blocksRaycasts = false;
            blackPanelGroup.interactable = false;
        }
    }

    public IEnumerator ShowAnnouncement(string message)
    {
        if (blackPanelGroup == null || announcementText == null)
        {
            Debug.LogWarning("PartyJoinAnnouncement is missing UI references.");
            yield break;
        }

        announcementText.text = message;

        blackPanelGroup.blocksRaycasts = true;
        blackPanelGroup.interactable = true;

        float timer = 0f;

        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            blackPanelGroup.alpha = Mathf.Clamp01(timer / fadeInDuration);
            yield return null;
        }

        blackPanelGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        timer = 0f;

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            blackPanelGroup.alpha = 1f - Mathf.Clamp01(timer / fadeOutDuration);
            yield return null;
        }

        blackPanelGroup.alpha = 0f;
        blackPanelGroup.blocksRaycasts = false;
        blackPanelGroup.interactable = false;
    }
}