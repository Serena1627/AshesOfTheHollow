using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class FadeTMPTextLoop : MonoBehaviour
{
    [SerializeField] private float fadeSpeed = 1.5f;
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 1f;

    private TMP_Text text;
    private bool fadingOut = true;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();

        Color color = text.color;
        color.a = maxAlpha;
        text.color = color;
    }

    private void Update()
    {
        Color color = text.color;

        if (fadingOut)
        {
            color.a -= fadeSpeed * Time.deltaTime;

            if (color.a <= minAlpha)
            {
                color.a = minAlpha;
                fadingOut = false;
            }
        }
        else
        {
            color.a += fadeSpeed * Time.deltaTime;

            if (color.a >= maxAlpha)
            {
                color.a = maxAlpha;
                fadingOut = true;
            }
        }

        text.color = color;
    }
}