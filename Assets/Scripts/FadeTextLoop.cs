using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeTextLoop : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1f;

    private CanvasGroup canvasGroup;
    private bool fadingOut = true;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = maxAlpha;
    }

    private void Update()
    {
        if (fadingOut)
        {
            canvasGroup.alpha -= fadeSpeed * Time.deltaTime;

            if (canvasGroup.alpha <= minAlpha)
            {
                canvasGroup.alpha = minAlpha;
                fadingOut = false;
            }
        }
        else
        {
            canvasGroup.alpha += fadeSpeed * Time.deltaTime;

            if (canvasGroup.alpha >= maxAlpha)
            {
                canvasGroup.alpha = maxAlpha;
                fadingOut = true;
            }
        }
    }
}