using System.Collections;
using UnityEngine;

public class CultistsDefeatedDialogue : MonoBehaviour
{
    [Header("Encounter")]
    [SerializeField] private string cultistEncounterId = "CultistScene_Cultists_01";

    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Tooltip("Dialogue played after Kael and Paladin defeat the cultists.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] defeatedDialogue;

    [Header("Timing")]
    [SerializeField] private float dialogueDelay = 0.75f;

    [Header("Optional")]
    [SerializeField] private bool disablePlayerMovementDuringDialogue = true;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        if (StoryProgress.CultistsDefeatedDialoguePlayed)
        {
            yield break;
        }

        if (!BattleData.IsEncounterDefeated(cultistEncounterId))
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

        yield return StartCoroutine(PlayDialogue(defeatedDialogue));

        if (disablePlayerMovementDuringDialogue && kaelController != null)
        {
            kaelController.enabled = true;
        }

        StoryProgress.MarkCultistsDefeatedDialoguePlayed();

        Debug.Log("Cultists defeated dialogue played.");
    }

    private IEnumerator PlayDialogue(DialogueTypewriter.DialogueLine[] dialogueLines)
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("CultistsDefeatedDialogue is missing DialogueTypewriter.");
            yield break;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Cultists defeated dialogue has no lines assigned.");
            yield break;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();

        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);
    }
}