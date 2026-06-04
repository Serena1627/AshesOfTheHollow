using System.Collections;
using UnityEngine;

public class BasementDialogueSequence : MonoBehaviour
{
    [Header("Dialogue System")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Header("Dialogue On Scene Load")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] openingDialogue;
    [SerializeField] private float openingDialogueDelay = 0.75f;

    [Header("Dialogue After Chest Sequence")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] afterChestDialogue;
    [SerializeField] private float afterChestDialogueDelay = 0.35f;

    private bool openingDialoguePlayed;
    private bool afterChestDialoguePlayed;

    private void Start()
    {
        StartCoroutine(PlayOpeningDialogueAfterDelay());
    }

    private IEnumerator PlayOpeningDialogueAfterDelay()
    {
        if (openingDialoguePlayed)
            yield break;

        yield return new WaitForSeconds(openingDialogueDelay);

        PlayDialogue(openingDialogue);
        openingDialoguePlayed = true;
    }

    public void PlayAfterChestDialogue()
    {
        if (afterChestDialoguePlayed)
            return;

        StartCoroutine(PlayAfterChestDialogueAfterDelay());
    }

    private IEnumerator PlayAfterChestDialogueAfterDelay()
    {
        yield return new WaitForSeconds(afterChestDialogueDelay);

        PlayDialogue(afterChestDialogue);
        afterChestDialoguePlayed = true;
    }

    private void PlayDialogue(DialogueTypewriter.DialogueLine[] dialogueLines)
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("BasementDialogueSequence is missing the DialogueTypewriter reference.");
            return;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines were assigned.");
            return;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();
    }
}