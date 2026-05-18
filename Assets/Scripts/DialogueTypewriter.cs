using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public float lettersPerSecond = 35f;

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        StartDialogue();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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

    private void StartDialogue()
    {
        if (lines == null || lines.Length == 0)
            return;

        dialogueBox.SetActive(true);
        currentLineIndex = 0;
        ShowLine(lines[currentLineIndex]);
    }

    private void ShowLine(DialogueLine line)
    {
        nameText.text = line.speakerName;
        dialogueText.text = line.dialogueText;

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
            dialogueBox.SetActive(false);
            return;
        }

        ShowLine(lines[currentLineIndex]);
    }
}