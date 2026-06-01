using System.Collections;
using UnityEngine;

public class SceneLoadDialogue : MonoBehaviour
{
    [Header("Dialogue System")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Header("Dialogue To Play")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] dialogueLines;

    [Header("Timing")]
    [SerializeField] private float delayBeforeDialogue = 0.75f;

    private bool hasPlayed;

    private void Start()
    {
        StartCoroutine(PlayDialogueAfterDelay());
    }

    private IEnumerator PlayDialogueAfterDelay()
    {
        if (hasPlayed)
        {
            yield break;
        }

        yield return new WaitForSeconds(delayBeforeDialogue);

        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("SceneLoadDialogue is missing its DialogueTypewriter reference.");
            yield break;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("SceneLoadDialogue has no dialogue lines assigned.");
            yield break;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();

        hasPlayed = true;

        Debug.Log("Scene-load dialogue started.");
    }
}