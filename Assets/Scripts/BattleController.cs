using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public static BattleController Instance { get; private set; }

    [Header("Runtime Battle Participants")]
    [SerializeField] private List<PlayerBattle> players = new List<PlayerBattle>();
    [SerializeField] private List<EnemyBattle> enemies = new List<EnemyBattle>();
    [SerializeField] private List<BattleEntity> battleEntities = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> turnOrder = new List<BattleEntity>();

    [Header("Items")]
    [SerializeField] private List<Item> items = new List<Item>();

    private bool battleEnded;

    public List<PlayerBattle> getPlayers()
    {
        return players;
    }

    public List<EnemyBattle> getEnemies()
    {
        return enemies;
    }

    public List<Item> getItems()
    {
        return items;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        FindBattleParticipants();

        if (players.Count == 0)
        {
            Debug.LogError(
                "Battle cannot begin: no PlayerBattle objects were found. " +
                "Check spawned party prefab tags and components."
            );
            return;
        }

        if (enemies.Count == 0)
        {
            Debug.LogError(
                "Battle cannot begin: no EnemyBattle objects were found. " +
                "Check spawned enemy prefab tags and components."
            );
            return;
        }

        if (BattleUIController.Instance == null)
        {
            Debug.LogError(
                "Battle cannot begin: no BattleUIController exists in BattleScene."
            );
            return;
        }

        BuildTurnOrder();
        SetStartingItems();

        StartCoroutine(BattleLoop());
    }

    private void FindBattleParticipants()
    {
        players.Clear();
        enemies.Clear();
        battleEntities.Clear();
        turnOrder.Clear();

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject playerObject in playerObjects)
        {
            PlayerBattle player = playerObject.GetComponent<PlayerBattle>();

            if (player == null)
            {
                Debug.LogWarning(
                    playerObject.name +
                    " is tagged Player but does not have PlayerBattle."
                );
                continue;
            }

            players.Add(player);
            battleEntities.Add(player);

            Debug.Log("Registered player: " + player.entityName);
        }

        foreach (GameObject enemyObject in enemyObjects)
        {
            EnemyBattle enemy = enemyObject.GetComponent<EnemyBattle>();

            if (enemy == null)
            {
                Debug.LogWarning(
                    enemyObject.name +
                    " is tagged Enemy but does not have EnemyBattle."
                );
                continue;
            }

            enemies.Add(enemy);
            battleEntities.Add(enemy);

            Debug.Log("Registered enemy: " + enemy.entityName);
        }
    }

    private void BuildTurnOrder()
    {
        turnOrder = battleEntities
            .OrderByDescending(entity => entity.speed)
            .ToList();

        Debug.Log("Turn order created with " + turnOrder.Count + " entities.");
    }

    private void SetStartingItems()
    {
        items.Clear();

        HealItem testPotion = new HealItem();
        testPotion.Init("TEST POTION", Item.itemTypes.SINGLE.ToString(), 2);

        items.Add(testPotion);
    }

    private IEnumerator BattleLoop()
    {
        Debug.Log("BATTLE START!");

        while (!battleEnded)
        {
            foreach (BattleEntity entity in turnOrder)
            {
                if (CheckBattleEnd())
                {
                    EndBattle();
                    yield break;
                }

                if (entity == null || entity.isEntityDead())
                {
                    continue;
                }

                Debug.Log(entity.entityName + "'s Turn!");

                yield return StartCoroutine(entity.Turn());

                if (CheckBattleEnd())
                {
                    EndBattle();
                    yield break;
                }
            }
        }
    }

    private bool CheckBattleEnd()
    {
        bool allPlayersDead = players.Count == 0 ||
            players.All(player => player == null || player.isEntityDead());

        bool allEnemiesDead = enemies.Count == 0 ||
            enemies.All(enemy => enemy == null || enemy.isEntityDead());

        return allPlayersDead || allEnemiesDead;
    }

    private void EndBattle()
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;

        bool allPlayersDead = players.Count == 0 ||
            players.All(player => player == null || player.isEntityDead());

        bool allEnemiesDead = enemies.Count == 0 ||
            enemies.All(enemy => enemy == null || enemy.isEntityDead());

        if (allPlayersDead)
        {
            Debug.Log("YOU LOSE");
            return;
        }

        if (allEnemiesDead)
        {
            Debug.Log("YOU WIN!");

            StartCoroutine(ReturnToOverworldAfterVictory());
        }
    }
    // -------------------------------------------------------------------------
    // Player Turns
    // -------------------------------------------------------------------------

    private IEnumerator ReturnToOverworldAfterVictory()
    {
        BattleData.MarkCurrentEncounterDefeated();

        string returnSceneName = BattleData.previousSceneName;

        if (string.IsNullOrWhiteSpace(returnSceneName))
        {
            Debug.LogError(
                "Battle was won, but BattleData.previousSceneName is empty."
            );

            yield break;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;

        BattleData.ClearCurrentBattleSetup();

        Debug.Log("Returning to overworld scene: " + returnSceneName);

        SceneManager.LoadScene(returnSceneName, LoadSceneMode.Single);
    }

    public IEnumerator PlayerTurn(PlayerBattle player)
    {
        if (player == null || player.isEntityDead())
        {
            yield break;
        }

        bool turnComplete = false;

        while (!turnComplete)
        {
            yield return StartCoroutine(BattleUIController.Instance.PlayerMenu());

            BattleUIController.ActionChoice selectedAction =
                BattleUIController.Instance.GetAction();

            switch (selectedAction)
            {
                case BattleUIController.ActionChoice.Attack:
                    turnComplete = false;
                    yield return StartCoroutine(
                        RunPlayerAttackSelection(player, result =>
                        {
                            turnComplete = result;
                        })
                    );
                    break;

                case BattleUIController.ActionChoice.Item:
                    turnComplete = false;
                    yield return StartCoroutine(
                        RunPlayerItemSelection(player, result =>
                        {
                            turnComplete = result;
                        })
                    );
                    break;

                default:
                    Debug.LogWarning("Player selected an unsupported battle action.");
                    break;
            }
        }
    }

    private IEnumerator RunPlayerAttackSelection(
        PlayerBattle player,
        System.Action<bool> onFinished
    )
    {
        Debug.Log("PICK ATTACK");

        yield return StartCoroutine(
            BattleUIController.Instance.PlayerAttacks(player)
        );

        Action selectedAttack = BattleUIController.Instance.GetAttack();

        if (selectedAttack == null)
        {
            Debug.Log("Attack selection cancelled. Returning to action menu.");
            onFinished(false);
            yield break;
        }

        Debug.Log("Executing attack: " + selectedAttack.getActionName());

        yield return StartCoroutine(player.Attack(selectedAttack));

        /*
         * Your PlayerBattle.Attack method is expected to set its target
         * after the target menu is completed. If the target is null,
         * selection was cancelled and the player should return to the menu.
         */
        if (player.getTarget() == null)
        {
            Debug.Log("Target selection cancelled. Returning to action menu.");
            onFinished(false);
            yield break;
        }

        onFinished(true);
    }

    private IEnumerator RunPlayerItemSelection(
        PlayerBattle player,
        System.Action<bool> onFinished
    )
    {
        Debug.Log("PICK ITEM");

        yield return StartCoroutine(
            BattleUIController.Instance.PlayerItems()
        );

        Item selectedItem = BattleUIController.Instance.GetItem();

        if (selectedItem == null)
        {
            Debug.Log("Item selection cancelled. Returning to action menu.");
            onFinished(false);
            yield break;
        }

        List<BattleEntity> itemTargets = GetItemTargets(selectedItem, player);

        Debug.Log(player.entityName + " used " + selectedItem.getName() + "!");

        selectedItem.useItem(itemTargets);
        items.Remove(selectedItem);

        onFinished(true);
    }

    private List<BattleEntity> GetItemTargets(Item item, PlayerBattle player)
    {
        List<BattleEntity> itemTargets = new List<BattleEntity>();

        if (item == null)
        {
            return itemTargets;
        }

        if (item.getType() == Item.itemTypes.SINGLE.ToString())
        {
            itemTargets.Add(player);
        }
        else if (item.getType() == Item.itemTypes.PARTY.ToString())
        {
            foreach (PlayerBattle partyMember in players)
            {
                if (partyMember != null && !partyMember.isEntityDead())
                {
                    itemTargets.Add(partyMember);
                }
            }
        }

        return itemTargets;
    }

    // -------------------------------------------------------------------------
    // Enemy Turns
    // -------------------------------------------------------------------------

    public IEnumerator EnemyTurn(EnemyBattle enemy)
    {
        if (enemy == null || enemy.isEntityDead())
        {
            yield break;
        }

        Action selectedAttack = enemy.chosenAttack();

        if (selectedAttack == null)
        {
            Debug.LogWarning(
                enemy.entityName +
                " has no valid configured attack and skips its turn."
            );

            yield break;
        }

        yield return StartCoroutine(enemy.Attack(selectedAttack));
    }
}