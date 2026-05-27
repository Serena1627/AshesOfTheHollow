using UnityEngine;
using UnityEngine.InputSystem;

public class MiraBasementInteract : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;
    [SerializeField] private DialogueTypewriter.DialogueLine[] miraLines;

    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartMiraDialogue();
        }
    }

    private void StartMiraDialogue()
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("DialogueTypewriter is not assigned.");
            return;
        }

        dialogueTypewriter.lines = miraLines;
        dialogueTypewriter.StartDialogue();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Press E to check on Mira.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}