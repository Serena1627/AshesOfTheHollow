using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BlackScreenDialogueSequence : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup blackScreenGroup;
    [SerializeField] private GameObject blackScreenObject;
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Header("Black Screen Dialogue")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] blackScreenLines;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private string nextSceneName = "BasementScene";

    private bool blackDialogueStarted = false;

    private void Start()
    {
        if (blackScreenObject != null)
            blackScreenObject.SetActive(true);

        if (blackScreenGroup != null)
            blackScreenGroup.alpha = 0f;
    }

    public void HandleDialogueFinished()
    {
        if (!blackDialogueStarted)
        {
            StartCoroutine(StartBlackDialogueRoutine());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    private IEnumerator StartBlackDialogueRoutine()
    {
        blackDialogueStarted = true;

        if (blackScreenObject != null)
            blackScreenObject.SetActive(true);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            blackScreenGroup.alpha = Mathf.Clamp01(timer / fadeDuration);
            yield return null;
        }

        blackScreenGroup.alpha = 1f;

        yield return new WaitForSeconds(0.3f);

        dialogueTypewriter.lines = blackScreenLines;
        dialogueTypewriter.StartDialogue();
    }
}