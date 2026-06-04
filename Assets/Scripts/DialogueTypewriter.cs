using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueTypewriter : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;

        [TextArea(2, 4)]
        public string dialogueText;

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

    private int currentLineIndex;
    private bool isTyping;
    private bool dialogueActive;
    private Coroutine typingCoroutine;

    // Other scripts use this to wait until dialogue is finished.
    public bool IsDialogueActive => dialogueActive;

    private void Start()
    {
        if (playOnStart)
        {
            StartDialogue();
        }
        else if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }

    private void Update()
    {
        if (!dialogueActive || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isTyping)
            {
                RevealFullLine();
            }
            else
            {
                ShowNextLine();
            }
        }
    }

    public void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("DialogueTypewriter has no dialogue lines assigned.");
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = true;
        isTyping = false;
        currentLineIndex = 0;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        ShowLine(lines[currentLineIndex]);
    }

    private void ShowLine(DialogueLine line)
    {
        if (nameText != null)
        {
            nameText.text = line.speakerName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.dialogueText;
            dialogueText.maxVisibleCharacters = 0;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = line.portrait;
            portraitImage.enabled = line.portrait != null;
        }

        if (continueArrow != null)
        {
            continueArrow.SetActive(false);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        if (dialogueText == null)
        {
            isTyping = false;
            yield break;
        }

        isTyping = true;

        dialogueText.ForceMeshUpdate();
        int totalCharacters = dialogueText.textInfo.characterCount;
        dialogueText.maxVisibleCharacters = 0;

        float safeLettersPerSecond = Mathf.Max(1f, lettersPerSecond);
        float delay = 1f / safeLettersPerSecond;

        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(delay);
        }

        typingCoroutine = null;
        isTyping = false;

        if (continueArrow != null)
        {
            continueArrow.SetActive(true);
        }
    }

    private void RevealFullLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.ForceMeshUpdate();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
        }

        isTyping = false;

        if (continueArrow != null)
        {
            continueArrow.SetActive(true);
        }
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
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogueActive = false;
        isTyping = false;

        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        if (continueArrow != null)
        {
            continueArrow.SetActive(false);
        }

        onDialogueFinished?.Invoke();
    }
}