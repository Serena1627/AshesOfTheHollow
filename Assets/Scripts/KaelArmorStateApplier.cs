using System.Collections;
using UnityEngine;

public class KaelArmorStateApplier : MonoBehaviour
{
    [SerializeField] private KaelTopDownController kaelController;

    private IEnumerator Start()
    {
        // Wait one frame so KaelTopDownController finishes Awake/Start setup.
        yield return null;

        if (kaelController == null)
        {
            kaelController = GetComponent<KaelTopDownController>();
        }

        if (kaelController == null)
        {
            kaelController = FindFirstObjectByType<KaelTopDownController>();
        }

        if (kaelController == null)
        {
            Debug.LogWarning("KaelArmorStateApplier could not find KaelTopDownController.");
            yield break;
        }

        if (StoryProgress.KaelHasArmorAndSword)
        {
            kaelController.SwitchToArmoredSprites();
            Debug.Log("Applied armored Kael sprites from StoryProgress.");
        }
    }
}