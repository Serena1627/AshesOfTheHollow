using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxUI : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Image portraitImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;

    public void ShowDialogue(string speakerName, string line, Sprite portrait)
    {
        dialogueBox.SetActive(true);
        nameText.text = speakerName;
        dialogueText.text = line;
        portraitImage.sprite = portrait;
        portraitImage.enabled = portrait != null;
    }

    public void HideDialogue()
    {
        dialogueBox.SetActive(false);
    }
}