using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Encounter Identity")]
    [Tooltip("Give each overworld encounter a unique ID, such as LioraScene_Monkey_01.")]
    [SerializeField] private string encounterId;

    [Tooltip("Parent object to remove after this encounter is defeated.")]
    [SerializeField] private GameObject encounterRoot;

    [Header("Battle Settings")]
    [SerializeField] private BattleBackgroundType battleBackground;

    [Tooltip("Assign enemy battle prefabs here, such as EnragedMonkeyBattle.")]
    [SerializeField] private List<GameObject> enemyPrefabs =
        new List<GameObject>();

    [Header("Guest Party Members")]
    [Tooltip("Temporary allies added only for this battle. Use this for LioraBattle.")]
    [SerializeField] private List<GameObject> guestPartyPrefabs =
        new List<GameObject>();

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private bool triggered;

    private void Start()
    {
        RemoveIfAlreadyDefeated();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (!CanStartEncounter())
        {
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

        BattleData.partyPrefabs = BuildBattlePartyWithGuests();

        Debug.Log(
            "Starting encounter: " +
            encounterId +
            " | Party count: " +
            BattleData.partyPrefabs.Count +
            " | Enemy count: " +
            BattleData.enemyPrefabs.Count
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

    private List<GameObject> BuildBattlePartyWithGuests()
    {
        List<GameObject> finalParty = new List<GameObject>();

        if (PartyManager.Instance != null)
        {
            finalParty.AddRange(
                PartyManager.Instance.GetCurrentPartyPrefabs()
            );
        }

        foreach (GameObject guestPrefab in guestPartyPrefabs)
        {
            if (guestPrefab == null)
            {
                continue;
            }

            if (!finalParty.Contains(guestPrefab))
            {
                finalParty.Add(guestPrefab);
            }
        }

        return finalParty;
    }

    private void RemoveIfAlreadyDefeated()
    {
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            return;
        }

        if (!BattleData.IsEncounterDefeated(encounterId))
        {
            return;
        }

        GameObject objectToRemove =
            encounterRoot != null ? encounterRoot : gameObject;

        Debug.Log("Removing defeated encounter from scene: " + encounterId);

        Destroy(objectToRemove);
    }

    private bool CanStartEncounter()
    {
        if (string.IsNullOrWhiteSpace(encounterId))
        {
            Debug.LogError(
                "Battle encounter cannot start because Encounter ID is empty."
            );

            return false;
        }

        if (BattleData.IsEncounterDefeated(encounterId))
        {
            Debug.Log("Encounter is already defeated: " + encounterId);
            return false;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogError(
                "Battle encounter cannot start because PartyManager is missing."
            );

            return false;
        }

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError(
                "Battle encounter has no enemy battle prefabs assigned."
            );

            return false;
        }

        List<GameObject> finalParty = BuildBattlePartyWithGuests();

        if (finalParty == null || finalParty.Count == 0)
        {
            Debug.LogError(
                "Battle encounter cannot start because there are no party members."
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(battleSceneName))
        {
            Debug.LogError("Battle Scene Name is empty.");
            return false;
        }

        return true;
    }

    public void ConfigureEncounter(
        string newEncounterId,
        GameObject newEncounterRoot,
        string newBattleBackground,
        List<GameObject> newEnemyPrefabs,
        string newBattleSceneName
    )
    {
        encounterId = newEncounterId;
        encounterRoot = newEncounterRoot;

        if (System.Enum.TryParse(
                newBattleBackground,
                true,
                out BattleBackgroundType result
            ))
        {
            battleBackground = result;
        }
        else
        {
            Debug.LogWarning(
                "Could not parse battle background type: " +
                newBattleBackground
            );
        }

        enemyPrefabs = newEnemyPrefabs != null
            ? new List<GameObject>(newEnemyPrefabs)
            : new List<GameObject>();

        battleSceneName = newBattleSceneName;
    }
}