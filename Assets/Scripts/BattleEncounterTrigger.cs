using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] private BattleBackgroundType battleBackground;

    [Tooltip("Assign battle prefabs such as HoundBattle or CultistBattle, not overworld sprites.")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
        {
            return;
        }

        if (PartyManager.Instance == null)
        {
            Debug.LogError("Cannot start battle: PartyManager does not exist.");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogError("Cannot start battle: no enemy battle prefabs are assigned on this encounter.");
            return;
        }

        if (string.IsNullOrWhiteSpace(battleSceneName))
        {
            Debug.LogError("Cannot start battle: Battle Scene Name is empty.");
            return;
        }

        List<GameObject> partyPrefabs = PartyManager.Instance.GetCurrentPartyPrefabs();

        if (partyPrefabs == null || partyPrefabs.Count == 0)
        {
            Debug.LogError("Cannot start battle: PartyManager has no party battle prefabs.");
            return;
        }

        triggered = true;

        BattleData.previousSceneName = SceneManager.GetActiveScene().name;
        BattleData.backgroundType = battleBackground;
        BattleData.enemyPrefabs = new List<GameObject>(enemyPrefabs);
        BattleData.partyPrefabs = new List<GameObject>(partyPrefabs);

        Debug.Log(
            $"Starting battle with {BattleData.partyPrefabs.Count} party member(s) " +
            $"against {BattleData.enemyPrefabs.Count} enemy/enemies."
        );

        SceneManager.LoadScene(battleSceneName);
    }
}