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
    [Tooltip("Assign the trigger collider on this RecruitTrigger object.")]
    [SerializeField] private Collider2D interactionTrigger;

    [Tooltip("Assign the stationary PaladinNPC parent object shown before recruitment.")]
    [SerializeField] private GameObject paladinNpcVisual;

    [Header("Optional Overworld Follower")]
    [Tooltip("Assign the top-down Paladin follower prefab, not PaladinBattle.")]
    [SerializeField] private GameObject paladinFollowerPrefab;

    [Tooltip("Place this beside or behind Kael/Paladin where the follower should appear.")]
    [SerializeField] private Transform followerSpawnPoint;

    [SerializeField] private bool spawnFollowerAfterRecruitment = true;

    private bool playerInRange;
    private bool sequenceRunning;

    private void Start()
    {
        // If Paladin was already recruited earlier in this play session,
        // remove the recruitable NPC version from the village.
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

        // Start Paladin recruitment dialogue.
        dialogueTypewriter.lines = recruitmentDialogue;
        dialogueTypewriter.StartDialogue();

        // Wait until the player has advanced through all dialogue lines.
        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("No PartyManager exists. Add PartyManager before testing recruitment.");
            sequenceRunning = false;
            yield break;
        }

        // Adds PaladinBattle prefab to the current party and records recruitment.
        PartyManager.Instance.RecruitPaladin();

        // Only continue visually if recruitment succeeded.
        if (!PartyManager.Instance.PaladinRecruited)
        {
            Debug.LogWarning("Paladin recruitment did not complete. Check the Paladin Battle Prefab field on PartyManager.");
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
            Debug.LogWarning("PartyJoinAnnouncement is not assigned. Recruitment will continue without the black-screen message.");
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

        SpriteRenderer npcRenderer = paladinNpcVisual.GetComponent<SpriteRenderer>();

        if (npcRenderer != null)
        {
            npcRenderer.enabled = false;
        }

        Collider2D npcCollider = paladinNpcVisual.GetComponent<Collider2D>();

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

        Vector3 spawnPosition = followerSpawnPoint != null
            ? followerSpawnPoint.position
            : transform.position;

        GameObject follower = Instantiate(
            paladinFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        PaladinFollowPlayer followScript = follower.GetComponent<PaladinFollowPlayer>();

        if (followScript == null)
        {
            Debug.LogWarning("The Paladin follower prefab does not contain PaladinFollowPlayer.");
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            Debug.LogWarning("Paladin follower could not find an object tagged Player.");
            return;
        }

        followScript.SetPlayer(playerObject.transform);
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