using System.Collections;
using UnityEngine;

public class ForceKaelBackIdleOnStart : MonoBehaviour
{
    [SerializeField] private KaelTopDownController kaelController;

    private IEnumerator Start()
    {
        // Wait one frame so any spawn-position scripts finish first.
        yield return null;

        if (kaelController == null)
        {
            kaelController = FindFirstObjectByType<KaelTopDownController>();
        }

        if (kaelController == null)
        {
            Debug.LogWarning("ForceKaelBackIdleOnStart could not find KaelTopDownController.");
            yield break;
        }

        kaelController.FaceBack();
    }
}