using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackScreenTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject blackScreenObject;
    [SerializeField] private CanvasGroup blackScreenGroup;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float holdDuration = 1.0f;
    [SerializeField] private string nextSceneName = "BasementScene";

    private bool isTransitioning = false;

    private void Start()
    {
        if (blackScreenObject != null)
            blackScreenObject.SetActive(true);

        if (blackScreenGroup != null)
            blackScreenGroup.alpha = 0f;
    }

    public void StartTransition()
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        if (blackScreenObject != null)
            blackScreenObject.SetActive(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeDuration;

            if (blackScreenGroup != null)
                blackScreenGroup.alpha = alpha;

            yield return null;
        }

        if (blackScreenGroup != null)
            blackScreenGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}