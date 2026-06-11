using System.Collections;
using UnityEngine;

public class LioraMonkeyVictoryRecruitment : MonoBehaviour
{
    [Header("Encounter")]
    [SerializeField] private string monkeyEncounterId = "LioraScene_Monkey_01";

    [Header("Dialogue")]
    [SerializeField] private DialogueTypewriter dialogueTypewriter;

    [Tooltip("Dialogue played after the monkey is defeated and before Liora joins.")]
    [SerializeField] private DialogueTypewriter.DialogueLine[] postBattleDialogue;

    [Header("Announcement")]
    [SerializeField] private PartyJoinAnnouncement partyJoinAnnouncement;
    [SerializeField] private string joinMessage = "Liora has joined your party.";

    [Header("Scene Visuals")]
    [Tooltip("The Liora overworld sprite standing in the scene.")]
    [SerializeField] private GameObject lioraSceneVisual;

    [Tooltip("Optional. Assign the monkey overworld sprite if you want this script to force-hide it too.")]
    [SerializeField] private GameObject monkeySceneVisual;

    [Header("Follower Spawn")]
    [SerializeField] private GameObject lioraFollowerPrefab;
    [SerializeField] private float followerSpawnDistance = 1.1f;

    [Header("Timing")]
    [SerializeField] private float startDelay = 0.5f;

    [Header("Options")]
    [SerializeField] private bool disablePlayerMovementDuringSequence = true;

    private bool sequenceRunning;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        if (sequenceRunning)
        {
            yield break;
        }

        if (!BattleData.IsEncounterDefeated(monkeyEncounterId))
        {
            yield break;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogWarning("Liora recruitment cannot run because PartyManager is missing.");
            yield break;
        }

        if (PartyManager.Instance.LioraRecruited)
        {
            HideSceneVisualsAfterRecruitment();
            SpawnLioraFollowerNow();
            yield break;
        }

        sequenceRunning = true;

        yield return new WaitForSeconds(startDelay);

        if (monkeySceneVisual != null)
        {
            monkeySceneVisual.SetActive(false);
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        KaelTopDownController kaelController = null;

        if (playerObject != null)
        {
            kaelController = playerObject.GetComponent<KaelTopDownController>();
        }

        if (disablePlayerMovementDuringSequence && kaelController != null)
        {
            kaelController.enabled = false;
        }

        yield return StartCoroutine(PlayDialogue(postBattleDialogue));

        PartyManager.Instance.RecruitLiora();

        if (partyJoinAnnouncement != null)
        {
            yield return StartCoroutine(
                partyJoinAnnouncement.ShowAnnouncement(joinMessage)
            );
        }
        else
        {
            Debug.LogWarning("PartyJoinAnnouncement is not assigned for Liora recruitment.");
        }

        HideSceneVisualsAfterRecruitment();
        SpawnLioraFollowerNow();

        if (disablePlayerMovementDuringSequence && kaelController != null)
        {
            kaelController.enabled = true;
        }

        sequenceRunning = false;
    }

    private IEnumerator PlayDialogue(DialogueTypewriter.DialogueLine[] dialogueLines)
    {
        if (dialogueTypewriter == null)
        {
            Debug.LogWarning("LioraMonkeyVictoryRecruitment is missing DialogueTypewriter.");
            yield break;
        }

        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("Liora post-battle dialogue has no lines assigned.");
            yield break;
        }

        dialogueTypewriter.lines = dialogueLines;
        dialogueTypewriter.StartDialogue();

        yield return new WaitUntil(() => !dialogueTypewriter.IsDialogueActive);
    }

    private void HideSceneVisualsAfterRecruitment()
    {
        if (monkeySceneVisual != null)
        {
            monkeySceneVisual.SetActive(false);
        }

        if (lioraSceneVisual != null)
        {
            lioraSceneVisual.SetActive(false);
        }
    }

    private void SpawnLioraFollowerNow()
    {
        if (lioraFollowerPrefab == null)
        {
            Debug.LogWarning("Liora follower prefab is not assigned.");
            return;
        }

        GameObject existingLioraFollower = GameObject.Find("LioraFollower");

        if (existingLioraFollower != null)
        {
            return;
        }

        Transform target = FindLioraFollowTarget();

        if (target == null)
        {
            Debug.LogWarning("Could not find target for Liora follower.");
            return;
        }

        Vector3 spawnPosition =
            target.position + Vector3.down * followerSpawnDistance;

        GameObject lioraFollower = Instantiate(
            lioraFollowerPrefab,
            spawnPosition,
            Quaternion.identity
        );

        lioraFollower.name = "LioraFollower";

        PaladinFollowPlayer followScript =
            lioraFollower.GetComponent<PaladinFollowPlayer>();

        if (followScript != null)
        {
            followScript.SetPlayer(target);
        }
        else
        {
            Debug.LogWarning("Liora follower prefab is missing PaladinFollowPlayer.");
        }

        Debug.Log("Liora follower spawned after recruitment.");
    }

    private Transform FindLioraFollowTarget()
    {
        GameObject miraFollower = GameObject.Find("MiraFollower");

        if (miraFollower != null)
        {
            return miraFollower.transform;
        }

        GameObject paladinFollower = GameObject.Find("PaladinFollower");

        if (paladinFollower != null)
        {
            return paladinFollower.transform;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            return playerObject.transform;
        }

        return null;
    }
}