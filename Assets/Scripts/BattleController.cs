using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleController : MonoBehaviour
{
    public static BattleController Instance { get; private set; }

    [Header("Runtime Participants")]
    [SerializeField] private List<PlayerBattle> players = new List<PlayerBattle>();
    [SerializeField] private List<EnemyBattle> enemies = new List<EnemyBattle>();
    [SerializeField] private List<BattleEntity> battleEntities = new List<BattleEntity>();
    [SerializeField] private List<BattleEntity> turnOrder = new List<BattleEntity>();

    [Header("Support Entities")]
    [Tooltip("Hidden support turn entities, like Liora's fairy.")]
    [SerializeField] private List<FairySupportBattle> supportEntities =
        new List<FairySupportBattle>();

    [Header("Runtime Items")]
    [SerializeField] private List<Item> items = new List<Item>();

    [Header("Battle Messages")]
    [SerializeField] private bool showActionSummaryMessages = true;
    [SerializeField] private bool showDamageLines = true;
    [SerializeField] private bool showDefeatLines = true;

    private bool battleEnded;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        FindBattleParticipants();
        RestorePartyHealthFromManager();

        if (players.Count == 0 || enemies.Count == 0)
        {
            Debug.LogError(
                "Battle cannot start: spawned players or enemies were not registered."
            );

            return;
        }

        if (BattleUIController.Instance == null)
        {
            Debug.LogError(
                "Battle cannot start: BattleUIController.Instance is null."
            );

            return;
        }

        BuildTurnOrder();
        LoadItemsFromInventory();

        StartCoroutine(BattleLoop());
    }

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

    private void FindBattleParticipants()
    {
        players.Clear();
        enemies.Clear();
        battleEntities.Clear();
        turnOrder.Clear();
        supportEntities.Clear();

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerBattle player = obj.GetComponent<PlayerBattle>();

            if (player == null)
            {
                Debug.LogWarning(
                    obj.name + " is tagged Player but does not have PlayerBattle."
                );

                continue;
            }

            players.Add(player);
            battleEntities.Add(player);

            Debug.Log("Registered player: " + player.entityName);
        }

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            EnemyBattle enemy = obj.GetComponent<EnemyBattle>();

            if (enemy == null)
            {
                Debug.LogWarning(
                    obj.name + " is tagged Enemy but does not have EnemyBattle."
                );

                continue;
            }

            enemies.Add(enemy);
            battleEntities.Add(enemy);

            Debug.Log("Registered enemy: " + enemy.entityName);
        }

        FairySupportBattle[] foundSupportEntities =
            FindObjectsOfType<FairySupportBattle>();

        foreach (FairySupportBattle supportEntity in foundSupportEntities)
        {
            if (supportEntity == null)
            {
                continue;
            }

            if (!supportEntities.Contains(supportEntity))
            {
                supportEntities.Add(supportEntity);
                Debug.Log(
                    "Registered support entity: " +
                    supportEntity.FairyName
                );
            }
        }
    }

    private void RestorePartyHealthFromManager()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning(
                "PartyManager was not found. Battle party will use prefab health values."
            );

            return;
        }

        foreach (PlayerBattle player in players)
        {
            if (player != null)
            {
                PartyManager.Instance.ApplyStoredHealth(player);
            }
        }
    }

    private void BuildTurnOrder()
    {
        turnOrder = battleEntities
            .OrderByDescending(entity => entity.speed)
            .ThenBy(entity => entity is EnemyBattle ? 1 : 0)
            .ThenBy(entity => entity.entityName)
            .ToList();

        Debug.Log("Turn order created with " + turnOrder.Count + " entities.");

        foreach (BattleEntity entity in turnOrder)
        {
            Debug.Log(
                "Turn order: " +
                entity.entityName +
                " | Speed: " +
                entity.speed
            );
        }

        foreach (FairySupportBattle supportEntity in supportEntities)
        {
            Debug.Log(
                "Support entity will act last each round: " +
                supportEntity.FairyName
            );
        }
    }

    private void LoadItemsFromInventory()
    {
        items.Clear();

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning(
                "InventoryManager was not found. No battle items will be available."
            );

            return;
        }

        items = InventoryManager.Instance.CreateBattleItems();

        Debug.Log("Loaded " + items.Count + " battle item type(s).");
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

            if (CheckBattleEnd())
            {
                EndBattle();
                yield break;
            }

            yield return StartCoroutine(RunSupportTurns());

            if (CheckBattleEnd())
            {
                EndBattle();
                yield break;
            }
        }
    }

    private IEnumerator RunSupportTurns()
    {
        foreach (FairySupportBattle supportEntity in supportEntities)
        {
            if (supportEntity == null)
            {
                continue;
            }

            yield return StartCoroutine(
                supportEntity.TakeSupportTurn(this)
            );
        }
    }

    private bool CheckBattleEnd()
    {
        bool partyDefeated =
            players.Count == 0 ||
            players.All(player => player == null || player.isEntityDead());

        bool enemiesDefeated =
            enemies.Count == 0 ||
            enemies.All(enemy => enemy == null || enemy.isEntityDead());

        return partyDefeated || enemiesDefeated;
    }

    private void SavePartyHealthToManager()
    {
        if (PartyManager.Instance == null)
        {
            Debug.LogWarning(
                "PartyManager was not found. Battle HP could not be saved."
            );

            return;
        }

        foreach (PlayerBattle player in players)
        {
            if (player != null)
            {
                PartyManager.Instance.SaveBattleHealth(player);
            }
        }
    }

    private void EndBattle()
    {
        if (battleEnded)
        {
            return;
        }

        battleEnded = true;

        bool allPlayersDead =
            players.Count == 0 ||
            players.All(player => player == null || player.isEntityDead());

        bool allEnemiesDead =
            enemies.Count == 0 ||
            enemies.All(enemy => enemy == null || enemy.isEntityDead());

        SavePartyHealthToManager();

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

    private IEnumerator ReturnToOverworldAfterVictory()
    {
        BattleData.MarkCurrentEncounterDefeated();

        string returnSceneName = BattleData.previousSceneName;

        if (string.IsNullOrWhiteSpace(returnSceneName))
        {
            Debug.LogError(
                "Battle won, but BattleData.previousSceneName is empty."
            );

            yield break;
        }

        yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 1f;

        BattleData.ClearCurrentBattleSetup();

        SceneManager.LoadScene(
            returnSceneName,
            LoadSceneMode.Single
        );
    }

    // -------------------------------------------------------------------------
    // Player Turn
    // -------------------------------------------------------------------------

    public IEnumerator PlayerTurn(PlayerBattle player)
    {
        if (player == null || player.isEntityDead())
        {
            yield break;
        }

        Debug.Log("PlayerTurn reached for: " + player.entityName);

        bool turnComplete = false;

        while (!turnComplete)
        {
            yield return StartCoroutine(
                BattleUIController.Instance.PlayerMenu()
            );

            switch (BattleUIController.Instance.GetAction())
            {
                case BattleUIController.ActionChoice.Attack:
                {
                    yield return StartCoroutine(
                        BattleUIController.Instance.PlayerAttacks(player)
                    );

                    Action selectedAttack =
                        BattleUIController.Instance.GetAttack();

                    if (selectedAttack == null)
                    {
                        Debug.Log(
                            "No attack selected. Returning to the main action menu."
                        );

                        break;
                    }

                    List<BattleEntity> targets =
                        new List<BattleEntity>();

                    yield return StartCoroutine(
                        GetPlayerTargetsForAction(
                            selectedAttack,
                            targets
                        )
                    );

                    if (targets.Count == 0)
                    {
                        Debug.Log(
                            "No target selected. Returning to the main action menu."
                        );

                        break;
                    }

                    yield return StartCoroutine(
                        ResolveActionEffects(
                            player.entityName,
                            selectedAttack,
                            targets
                        )
                    );

                    turnComplete = true;
                    break;
                }

                case BattleUIController.ActionChoice.Item:
                {
                    if (items.Count == 0)
                    {
                        yield return StartCoroutine(
                            ShowBattleMessage("No usable items are available.")
                        );

                        break;
                    }

                    yield return StartCoroutine(
                        BattleUIController.Instance.PlayerItems()
                    );

                    Item selectedItem =
                        BattleUIController.Instance.GetItem();

                    if (selectedItem == null)
                    {
                        Debug.Log(
                            "No item selected. Returning to the main action menu."
                        );

                        break;
                    }

                    selectedItem.useItem(
                        GetItemTargets(selectedItem, player)
                    );

                    yield return StartCoroutine(
                        ShowBattleMessage(
                            player.entityName +
                            " used " +
                            selectedItem.getName() +
                            "."
                        )
                    );

                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.ConsumeItem(
                            selectedItem.getName()
                        );

                        LoadItemsFromInventory();
                    }
                    else
                    {
                        items.Remove(selectedItem);
                    }

                    turnComplete = true;
                    break;
                }
            }
        }
    }

    private IEnumerator GetPlayerTargetsForAction(
        Action selectedAttack,
        List<BattleEntity> targets
    )
    {
        targets.Clear();

        if (selectedAttack == null)
        {
            yield break;
        }

        if (selectedAttack.getTargeting() ==
            Action.targetingTypes.SINGLE.ToString())
        {
            yield return StartCoroutine(
                BattleUIController.Instance.Targeting(enemies)
            );

            EnemyBattle selectedTarget =
                BattleUIController.Instance.ReturnTarget();

            if (selectedTarget != null && !selectedTarget.isEntityDead())
            {
                targets.Add(selectedTarget);
            }

            yield break;
        }

        if (selectedAttack.getTargeting() ==
            Action.targetingTypes.SPREAD.ToString())
        {
            foreach (EnemyBattle enemy in enemies)
            {
                if (enemy != null && !enemy.isEntityDead())
                {
                    targets.Add(enemy);
                }
            }
        }
    }

    private List<BattleEntity> GetItemTargets(
        Item item,
        PlayerBattle actingPlayer
    )
    {
        List<BattleEntity> result = new List<BattleEntity>();

        if (item == null)
        {
            return result;
        }

        if (item.getType() == Item.itemTypes.SINGLE.ToString())
        {
            result.Add(actingPlayer);
        }
        else if (item.getType() == Item.itemTypes.PARTY.ToString())
        {
            foreach (PlayerBattle player in players)
            {
                if (player != null && !player.isEntityDead())
                {
                    result.Add(player);
                }
            }
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // Enemy Turn
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

            yield return StartCoroutine(
                ShowBattleMessage(enemy.entityName + " hesitates.")
            );

            yield break;
        }

        List<BattleEntity> targets = new List<BattleEntity>();

        yield return StartCoroutine(
            GetEnemyTargetsForAction(
                enemy,
                selectedAttack,
                targets
            )
        );

        if (targets.Count == 0)
        {
            yield return StartCoroutine(
                ShowBattleMessage(
                    enemy.entityName + " has no valid target."
                )
            );

            yield break;
        }

        yield return StartCoroutine(
            ResolveActionEffects(
                enemy.entityName,
                selectedAttack,
                targets
            )
        );
    }

    private IEnumerator GetEnemyTargetsForAction(
        EnemyBattle enemy,
        Action selectedAttack,
        List<BattleEntity> targets
    )
    {
        targets.Clear();

        if (enemy == null || selectedAttack == null)
        {
            yield break;
        }

        if (selectedAttack.getTargeting() ==
            Action.targetingTypes.SINGLE.ToString())
        {
            yield return StartCoroutine(enemy.pickTarget());

            BattleEntity selectedTarget = enemy.getTarget();

            if (selectedTarget != null && !selectedTarget.isEntityDead())
            {
                targets.Add(selectedTarget);
            }

            yield break;
        }

        if (selectedAttack.getTargeting() ==
            Action.targetingTypes.SPREAD.ToString())
        {
            foreach (BattleEntity target in enemy.getAllTargets())
            {
                if (target != null && !target.isEntityDead())
                {
                    targets.Add(target);
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Action Resolution
    // -------------------------------------------------------------------------

    private IEnumerator ResolveActionEffects(
        string actorName,
        Action action,
        List<BattleEntity> targets
    )
    {
        if (action == null || targets == null || targets.Count == 0)
        {
            yield break;
        }

        List<string> messageLines = new List<string>();

        if (showActionSummaryMessages)
        {
            messageLines.Add(
                actorName +
                " uses " +
                action.getActionName() +
                "!"
            );
        }

        foreach (BattleEntity target in targets)
        {
            if (target == null || target.isEntityDead())
            {
                continue;
            }

            int healthBefore = target.CurrentHealth;
            bool wasDeadBefore = target.isEntityDead();

            action.doAction(
                new List<BattleEntity>
                {
                    target
                }
            );

            int healthAfter = target.CurrentHealth;
            int damageTaken = Mathf.Max(0, healthBefore - healthAfter);

            if (showDamageLines)
            {
                messageLines.Add(
                    target.entityName +
                    " took " +
                    damageTaken +
                    " damage."
                );
            }

            if (showDefeatLines &&
                !wasDeadBefore &&
                target.isEntityDead())
            {
                messageLines.Add(
                    target.entityName + " was defeated."
                );
            }
        }

        if (messageLines.Count > 0)
        {
            yield return StartCoroutine(
                ShowBattleMessage(
                    string.Join("\n", messageLines)
                )
            );
        }
    }

    // -------------------------------------------------------------------------
    // Fairy Healing
    // -------------------------------------------------------------------------

    public IEnumerator HealLowestWoundedAlly(
        int healAmount,
        string healerName,
        bool showMessageWhenNoOneNeedsHealing
    )
    {
        PlayerBattle target = GetLowestWoundedAlly();

        if (target == null)
        {
            if (showMessageWhenNoOneNeedsHealing)
            {
                yield return StartCoroutine(
                    ShowBattleMessage(
                        healerName + " watches over the party."
                    )
                );
            }

            yield break;
        }

        int healthBefore = target.CurrentHealth;
        int maxHealth = Mathf.Max(1, target.MaxHealth);

        int newHealth = Mathf.Clamp(
            healthBefore + Mathf.Max(0, healAmount),
            0,
            maxHealth
        );

        int actualHealing = newHealth - healthBefore;

        if (actualHealing <= 0)
        {
            yield break;
        }

        target.RestoreHealthFromPartyState(
            newHealth,
            maxHealth
        );

        yield return StartCoroutine(
            ShowBattleMessage(
                healerName +
                " heals " +
                target.entityName +
                " for " +
                actualHealing +
                " HP."
            )
        );
    }

    private PlayerBattle GetLowestWoundedAlly()
    {
        PlayerBattle chosenTarget = null;

        float lowestHealthPercent = float.MaxValue;
        int highestMissingHealth = -1;

        foreach (PlayerBattle player in players)
        {
            if (player == null || player.isEntityDead())
            {
                continue;
            }

            int maxHealth = Mathf.Max(1, player.MaxHealth);
            int currentHealth = Mathf.Clamp(
                player.CurrentHealth,
                0,
                maxHealth
            );

            if (currentHealth >= maxHealth)
            {
                continue;
            }

            int missingHealth = maxHealth - currentHealth;
            float healthPercent = currentHealth / (float)maxHealth;

            bool shouldChoose =
                chosenTarget == null ||
                healthPercent < lowestHealthPercent ||
                (
                    Mathf.Approximately(healthPercent, lowestHealthPercent) &&
                    missingHealth > highestMissingHealth
                );

            if (shouldChoose)
            {
                chosenTarget = player;
                lowestHealthPercent = healthPercent;
                highestMissingHealth = missingHealth;
            }
        }

        return chosenTarget;
    }

    private IEnumerator ShowBattleMessage(string message)
    {
        if (BattleUIController.Instance == null)
        {
            Debug.Log(message);
            yield break;
        }

        yield return StartCoroutine(
            BattleUIController.Instance.ShowBattleMessage(message)
        );
    }
}