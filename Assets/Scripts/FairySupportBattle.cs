using System.Collections;
using UnityEngine;

public class FairySupportBattle : MonoBehaviour
{
    [Header("Support Identity")]
    [SerializeField] private string fairyName = "Forest Pixie";

    [Header("Healing")]
    [SerializeField] private int healAmount = 5;

    [Tooltip("If false, the fairy says nothing when everyone is already full HP.")]
    [SerializeField] private bool showMessageWhenNoOneNeedsHealing = false;

    [Header("Visuals")]
    [Tooltip("Keeps this support entity invisible even if a SpriteRenderer is accidentally added.")]
    [SerializeField] private bool hideRenderersOnStart = true;

    public string FairyName => fairyName;
    public int HealAmount => healAmount;
    public bool ShowMessageWhenNoOneNeedsHealing => showMessageWhenNoOneNeedsHealing;

    private void Awake()
    {
        if (!hideRenderersOnStart)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = false;
        }
    }

    public IEnumerator TakeSupportTurn(BattleController battleController)
    {
        if (battleController == null)
        {
            yield break;
        }

        yield return StartCoroutine(
            battleController.HealLowestWoundedAlly(
                healAmount,
                fairyName,
                showMessageWhenNoOneNeedsHealing
            )
        );
    }
}