using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PaladinRecruitInteraction : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;
    [SerializeField] private DialogueTypewriter.DialogueLine[] recruitmentDialogue;

    [Header("Recruitment Announcement")]
    [SerializeField] private PartyJoinAnnouncement partyJoinAnnouncement;
    [SerializeField] private string joinMessage = "Paladin has joined your party.";

    [Header("NPC Interaction")]
    [Tooltip("Assign the BoxCollider2D on the RecruitTrigger object.")]
    [SerializeField] private Collider2D interactionTrigger;

    [Tooltip("Assign the stationary PaladinNPC scene object.")]
    [SerializeField] private GameObject paladinNpcVisual;

    [Header("Optional Overworld Follower")]
    [Tooltip("Assign the top-down walking Paladin prefab, not PaladinBattle.")]
    [SerializeField] private GameObject paladinFollowerPrefab;

    [Tooltip("How far behind Kael the follower appears after recruitment.")]
    [SerializeField] private float followerSpawnDistance = 1.1f;

    [SerializeField] private bool spawnFollowerAfterRecruitment = true;

    private bool playerInRange;
    private bool sequenceRunning;

    private void Start()
    {
        if (PartyManager.Instance != null && PartyManager.Instance.PaladinRecruited)
        {
            HideRecruitableNpc();
        }
    }

    private void Update()
    {
        if (!playerInRange || sequenceRunning)
        {
            return;
        }

        if (PartyManager.Instance != null && PartyManager.Instance.PaladinRecruited)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(RecruitPaladinSequence());
        }
    }

    private IEnumerator RecruitPaladinSequence()
    {
        sequenceRunning = true;

        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("Paladin recruitment is missing the DialogueTypewriter reference.");
            sequenceRunning = false;
            yield break;
        }

        if (recruitmentDialogue == null || recruitmentDialogue.Length == 0)
        {
            Debug.LogWarning("Paladin recruitment has no dialogue lines assigned.");
            sequenceRunning = false;
            yield break;
        }

        dialogueTypewriter.lines = recruitmentDialogue;
        dialogueTypewriter.StartDialogue();

        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("No PartyManager exists. Add PartyManager before testing recruitment.");
            sequenceRunning = false;
            yield break;
        }

        PartyManager.Instance.RecruitPaladin();

        if (!PartyManager.Instance.PaladinRecruited)
        {
            Debug.LogWarning("Paladin recruitment did not complete. Check PartyManager setup.");
            sequenceRunning = false;
            yield break;
        }

        if (partyJoinAnnouncement != null)
        {
            yield return StartCoroutine(
                partyJoinAnnouncement.ShowAnnouncement(joinMessage)
            );
        }
        else
        {
            Debug.LogWarning("PartyJoinAnnouncement is not assigned.");
        }

        HideRecruitableNpc();

        if (spawnFollowerAfterRecruitment)
        {
            SpawnPaladinFollower();
        }

        sequenceRunning = false;

        Debug.Log("Paladin recruitment sequence complete.");
    }

    private void HideRecruitableNpc()
    {
        if (interactionTrigger != null)
        {
            interactionTrigger.enabled = false;
        }

        if (paladinNpcVisual == null)
        {
            return;
        }

        SpriteRenderer npcRenderer =
            paladinNpcVisual.GetComponent<SpriteRenderer>();

        if (npcRenderer != null)
        {
            npcRenderer.enabled = false;
        }

        Collider2D npcCollider =
            paladinNpcVisual.GetComponent<Collider2D>();

        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }
    }

    private void SpawnPaladinFollower()
    {
        if (paladinFollowerPrefab == null)
        {
            Debug.LogWarning("Spawn Follower After Recruitment is enabled, but Paladin Follower Prefab is not assigned.");
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Cannot spawn Paladin follower because no object tagged Player was found.");
            return;
        }

        KaelTopDownController kaelController =
            playerObject.GetComponent<KaelTopDownController>();

        if (kaelController == null)
        {
            Debug.LogWarning("The Player object does not contain KaelTopDownController.");
            return;
        }

        Vector3 spawnPosition =
            kaelController.GetFollowerSpawnPosition(followerSpawnDistance);

        GameObject follower = Instantiate(
            paladinFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PaladinFollowPlayer followScript =
            follower.GetComponent<PaladinFollowPlayer>();

        if (followScript == null)
        {
            Debug.LogWarning("The Paladin follower prefab does not contain PaladinFollowPlayer.");
            Destroy(follower);
            return;
        }

        followScript.SetPlayer(playerObject.transform);

        Debug.Log("Paladin follower spawned behind Kael.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        if (PartyManager.Instance == null || !PartyManager.Instance.PaladinRecruited)
        {
            Debug.Log("Press E to speak to Paladin.");
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