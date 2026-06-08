using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PartyMemberCardUI : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Image hpBarFill;

    [Header("Text")]
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterClassText;
    [SerializeField] private TMP_Text hpCurrentText;
    [SerializeField] private TMP_Text hpMaxText;

    public void Setup(PartyMemberData member)
    {
        if (member == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (characterPortrait != null)
            characterPortrait.sprite = member.characterPortrait;

        if (characterNameText != null)
            characterNameText.text = member.characterName;

        if (characterClassText != null)
            characterClassText.text = member.characterClass;

        UpdateHP(member.currentHP, member.maxHP);
    }

    public void UpdateHP(int currentHP, int maxHP)
    {
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        if (hpCurrentText != null)
            hpCurrentText.text = currentHP.ToString();

        if (hpMaxText != null)
            hpMaxText.text = maxHP.ToString();

        if (hpBarFill != null)
            hpBarFill.fillAmount = maxHP > 0 ? (float)currentHP / maxHP : 0f;
    }
}