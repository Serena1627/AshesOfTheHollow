using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class DialogueTypewriter : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 4)] public string dialogueText;
        public Sprite portrait;
    }

    [Header("UI References")]
    public GameObject dialogueBox;
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Image portraitImage;
    public GameObject continueArrow;

    [Header("Dialogue Lines")]
    public DialogueLine[] lines;

    [Header("Typewriter Settings")]
    public bool playOnStart = true;
    public float lettersPerSecond = 35f;

    [Header("Events")]
    public UnityEvent onDialogueFinished;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private bool dialogueActive = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (playOnStart)
            StartDialogue();
        else if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private void Update()
    {
        if (!dialogueActive)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
                RevealFullLine();
            else
                ShowNextLine();
        }
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
            return;

        dialogueActive = true;
        currentLineIndex = 0;

        if (dialogueBox != null)
            dialogueBox.SetActive(true);

        ShowLine(lines[currentLineIndex]);
    }

    private void ShowLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        dialogueText.text = line.dialogueText;
        dialogueText.maxVisibleCharacters = 0;

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
        }

        if (continueArrow != null)
            continueArrow.SetActive(false);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;

        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
        dialogueText.maxVisibleCharacters = 0;

        float delay = 1f / lettersPerSecond;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;

        if (continueArrow != null)
            continueArrow.SetActive(true);
    }

    private void RevealFullLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        dialogueText.ForceMeshUpdate();
        dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;

        isTyping = false;

        if (continueArrow != null)
            continueArrow.SetActive(true);
    }

    private void ShowNextLine()
    {
        currentLineIndex++;

        if (currentLineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowLine(lines[currentLineIndex]);
    }

    private void EndDialogue()
    {
        dialogueActive = false;

        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        onDialogueFinished?.Invoke();
    }
}