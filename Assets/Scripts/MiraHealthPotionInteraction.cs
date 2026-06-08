using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiraHealthPotionInteraction : MonoBehaviour
{
    [Header("Required Item")]
    [SerializeField] private string requiredPotionName = "[Health Potion]";

    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Tooltip("Played if Kael does not have the health potion.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] noPotionDialogue;

    [Tooltip("Played if Kael has the health potion. Mira joins after this dialogue.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] hasPotionDialogue;

    [Tooltip("Played if Mira has already joined.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] alreadyJoinedDialogue;

    [Header("Announcement")]
    [SerializeField] private PartyJoinAnnouncement partyJoinAnnouncement;
    [SerializeField] private string joinMessage = "Mira has joined your party.";

    [Header("Visuals")]
    [SerializeField] private GameObject injuredMiraVisual;
    [SerializeField] private GameObject recoveredMiraVisual;

    [Header("Interaction")]
    [SerializeField] private Collider2D interactionTrigger;
    [SerializeField] private bool disableAfterRecruitment = true;
    [Header("Mira Follower Spawn")]
    [SerializeField] private GameObject miraFollowerPrefab;
    [SerializeField] private float miraSpawnDistance = 1.1f;

    private bool playerInRange;
    private bool sequenceRunning;

    private void Start()
    {
        if (recoveredMiraVisual != null)
        {
            recoveredMiraVisual.SetActive(false);
        }

        if (PartyManager.Instance != null && PartyManager.Instance.MiraRecruited)
        {
            ShowRecoveredMira();

            if (disableAfterRecruitment && interactionTrigger != null)
            {
                interactionTrigger.enabled = false;
            }
        }
    }

    private void Update()
    {
        if (!playerInRange || sequenceRunning)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(HandleMiraInteraction());
        }
    }

    private IEnumerator HandleMiraInteraction()
    {
        sequenceRunning = true;

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("Cannot recruit Mira because PartyManager is missing.");
            sequenceRunning = false;
            yield break;
        }

        if (PartyManager.Instance.MiraRecruited)
        {
            yield return StartCoroutine(PlayDialogue(alreadyJoinedDialogue));
            sequenceRunning = false;
            yield break;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("Cannot check potion because InventoryManager is missing.");
            yield return StartCoroutine(PlayDialogue(noPotionDialogue));
            sequenceRunning = false;
            yield break;
        }

        int potionCount = InventoryManager.Instance.GetItemQuantity(requiredPotionName);

        if (potionCount <= 0)
        {
            yield return StartCoroutine(PlayDialogue(noPotionDialogue));
            sequenceRunning = false;
            yield break;
        }

        bool consumed = InventoryManager.Instance.ConsumeItem(requiredPotionName);

        if (!consumed)
        {
            Debug.LogWarning("Potion existed, but could not be consumed.");
            sequenceRunning = false;
            yield break;
        }

        yield return StartCoroutine(PlayDialogue(hasPotionDialogue));

        PartyManager.Instance.RecruitMira();

        ShowRecoveredMira();
        SpawnMiraFollowerNow();

        if (partyJoinAnnouncement != null)
        {
            yield return StartCoroutine(
                partyJoinAnnouncement.ShowAnnouncement(joinMessage)
            );
        }

        if (disableAfterRecruitment && interactionTrigger != null)
        {
            interactionTrigger.enabled = false;
        }

        sequenceRunning = false;
    }

    private IEnumerator PlayDialogue(DialogueTypewriter.DialogueLine[] dialogueLines)
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("Mira interaction is missing DialogueTypewriter.");
            yield break;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            yield break;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();

        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);
    }

    private void ShowRecoveredMira()
    {
        if (injuredMiraVisual != null)
        {
            injuredMiraVisual.SetActive(false);
        }

        if (recoveredMiraVisual != null)
        {
            recoveredMiraVisual.SetActive(true);
        }
    }

    private void SpawnMiraFollowerNow()
    {
        if (miraFollowerPrefab == null)
        {
            Debug.LogWarning("Mira follower prefab is not assigned.");
            return;
        }

        GameObject paladinFollower =
            GameObject.Find("PaladinFollower(Clone)");

        Transform target = null;

        if (paladinFollower != null)
        {
            target = paladinFollower.transform;
        }
        else
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                target = playerObject.transform;
            }
        }

        if (target == null)
        {
            Debug.LogWarning("Could not find target for Mira follower.");
            return;
        }

        Vector3 spawnPosition = target.position + Vector3.down * miraSpawnDistance;

        GameObject miraFollower = Instantiate(
            miraFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PaladinFollowPlayer followScript =
            miraFollower.GetComponent<PaladinFollowPlayer>();

        if (followScript != null)
        {
            followScript.SetPlayer(target);
        }

        Debug.Log("Mira follower spawned after recruitment.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Press E to help Mira.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}