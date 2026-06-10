using System.Collections;
using UnityEngine;

public class MiraVillageArrivalDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Tooltip("Dialogue played when Mira enters VillageScene with the party for the first time.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] miraFirstVillageDialogue;

    [Header("Timing")]
    [SerializeField] private float dialogueDelay = 0.75f;

    [Header("Optional")]
    [Tooltip("If true, player movement is disabled during the dialogue.")]
    [SerializeField] private bool disablePlayerMovementDuringDialogue = true;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        if (StoryProgress.MiraFirstVillageArrivalDialoguePlayed)
        {
            yield break;
        }

        if (PartyManager.Instance == null)
        {
            yield break;
        }

        if (!PartyManager.Instance.MiraRecruited)
        {
            yield break;
        }

        yield return new WaitForSeconds(dialogueDelay);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        KaelTopDownController kaelController = null;

        if (playerObject != null)
        {
            kaelController = playerObject.GetComponent<KaelTopDownController>();
        }

        if (disablePlayerMovementDuringDialogue && kaelController != null)
        {
            kaelController.enabled = false;
        }

        yield return StartCoroutine(PlayDialogue(miraFirstVillageDialogue));

        if (disablePlayerMovementDuringDialogue && kaelController != null)
        {
            kaelController.enabled = true;
        }

        StoryProgress.MarkMiraFirstVillageArrivalDialoguePlayed();

        Debug.Log("Mira first VillageScene arrival dialogue played.");
    }

    private IEnumerator PlayDialogue(DialogueTypewriter.DialogueLine[] dialogueLines)
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("MiraVillageArrivalDialogue is missing DialogueTypewriter.");
            yield break;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Mira first village dialogue has no lines assigned.");
            yield break;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();

        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);
    }
}