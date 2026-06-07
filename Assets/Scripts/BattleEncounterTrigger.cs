using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Encounter Identity")]
    [Tooltip("Give each overworld encounter a unique ID, such as Village_Hound_01.")]
    [SerializeField] private string encounterId;

    [Tooltip("Assign the whole overworld enemy object if this script is on a child trigger.")]
    [SerializeField] private GameObject encounterRoot;

    [Header("Battle Settings")]
    [SerializeField] private BattleBackgroundType battleBackground;

    [Tooltip("Assign battle prefabs such as HoundBattle, not overworld sprites.")]
    [SerializeField]
    private List<GameObject> enemyPrefabs =
        new List<GameObject>();

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private bool triggered;

    private void Start()
    {
        if (BattleData.IsEncounterDefeated(encounterId))
        {
            GameObject objectToRemove =
                encounterRoot != null ? encounterRoot : gameObject;

            Destroy(objectToRemove);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(encounterId))
        {
            Debug.LogError("Battle encounter cannot start because Encounter ID is empty.");
            return;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogError("Battle encounter cannot start because PartyManager is missing.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Battle encounter has no enemy battle prefabs assigned.");
            return;
        }

        List<GameObject> currentParty =
            PartyManager.Instance.GetCurrentPartyPrefabs();

        if (currentParty == null || currentParty.Count == 0)
        {
            Debug.LogError("PartyManager has no battle party prefabs assigned.");
            return;
        }

        triggered = true;

        Transform playerTransform = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;

        BattleData.SavePlayerPosition(playerTransform.position);

        BattleData.previousSceneName = SceneManager.GetActiveScene().name;
        BattleData.backgroundType = battleBackground;
        BattleData.currentEncounterId = encounterId;
        BattleData.enemyPrefabs = new List<GameObject>(enemyPrefabs);
        BattleData.partyPrefabs = new List<GameObject>(currentParty);

        Debug.Log(
            "Starting encounter: " + encounterId +
            " | Return position: " + BattleData.previousPlayerPosition
        );

        if (PixelatedBattleTransition.Instance != null)
        {
            PixelatedBattleTransition.Instance.BeginBattleTransition(
                battleSceneName
            );
        }
        else
        {
            Debug.LogWarning(
                "PixelatedBattleTransition was not found. Loading battle normally."
            );

            SceneManager.LoadScene(
                battleSceneName,
                LoadSceneMode.Single
            );
        }
    }
}