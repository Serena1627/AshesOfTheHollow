using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEncounterTrigger : MonoBehaviour
{
    [Header("Battle Settings")]
    [SerializeField] private BattleBackgroundType battleBackground;
    [SerializeField] private List<GameObject> enemyPrefabs;

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        BattleData.previousSceneName = SceneManager.GetActiveScene().name;
        BattleData.backgroundType = battleBackground;
        BattleData.enemyPrefabs = enemyPrefabs;

        BattleData.partyPrefabs = PartyManager.Instance.GetCurrentPartyPrefabs();

        SceneManager.LoadScene(battleSceneName);
    }
}