using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBattle : BattleEntity
{
    private readonly Dictionary<string, Action> enemyActions =
        new Dictionary<string, Action>();

    public enum aiTypes
    {
        RAND,
        BOSS
    }

    [Header("AI")]
    [SerializeField] private aiTypes AI = aiTypes.RAND;

    [Header("Action #1")]
    [SerializeField] private int action1Damage;
    [SerializeField] private string action1Name;
    [SerializeField] private Action.actionTypes action1Type;
    [SerializeField] private Action.targetingTypes action1TargetingType;

    [Header("Action #2")]
    [SerializeField] private int action2Damage;
    [SerializeField] private string action2Name;
    [SerializeField] private Action.actionTypes action2Type;
    [SerializeField] private Action.targetingTypes action2TargetingType;

    [Header("Action #3")]
    [SerializeField] private int action3Damage;
    [SerializeField] private string action3Name;
    [SerializeField] private Action.actionTypes action3Type;
    [SerializeField] private Action.targetingTypes action3TargetingType;

    [Header("Action #4")]
    [SerializeField] private int action4Damage;
    [SerializeField] private string action4Name;
    [SerializeField] private Action.actionTypes action4Type;
    [SerializeField] private Action.targetingTypes action4TargetingType;

    private BattleEntity target;

    private void Awake()
    {
        addActions();
    }

    public override void addActions()
    {
        enemyActions.Clear();

        AddActionIfAssigned(
            "Action1",
            action1Damage,
            action1Name,
            action1TargetingType,
            action1Type
        );

        AddActionIfAssigned(
            "Action2",
            action2Damage,
            action2Name,
            action2TargetingType,
            action2Type
        );

        AddActionIfAssigned(
            "Action3",
            action3Damage,
            action3Name,
            action3TargetingType,
            action3Type
        );

        AddActionIfAssigned(
            "Action4",
            action4Damage,
            action4Name,
            action4TargetingType,
            action4Type
        );

        Debug.Log(entityName + " initialized " + enemyActions.Count + " action(s).");
    }

    private void AddActionIfAssigned(
        string label,
        int damage,
        string actionName,
        Action.targetingTypes targetingType,
        Action.actionTypes actionType
    )
    {
        if (string.IsNullOrWhiteSpace(actionName))
        {
            return;
        }

        Action action = new Action();
        action.Init(
            damage,
            actionName,
            targetingType.ToString(),
            actionType.ToString()
        );

        enemyActions.Add(label, action);
    }

    public override List<BattleEntity> getAllTargets()
    {
        List<BattleEntity> targets = new List<BattleEntity>();

        if (BattleController.Instance == null)
        {
            return targets;
        }

        foreach (PlayerBattle player in BattleController.Instance.getPlayers())
        {
            if (player != null && !player.isEntityDead())
            {
                targets.Add(player);
            }
        }

        return targets;
    }

    public override BattleEntity getTarget()
    {
        return target;
    }

    public override IEnumerator pickTarget()
    {
        target = null;

        if (BattleController.Instance == null)
        {
            Debug.LogWarning(entityName + " cannot choose a target because BattleController is missing.");
            yield break;
        }

        List<PlayerBattle> availablePlayers = BattleController.Instance
            .getPlayers()
            .Where(player => player != null && !player.isEntityDead())
            .ToList();

        if (availablePlayers.Count == 0)
        {
            Debug.LogWarning(entityName + " has no valid player targets.");
            yield break;
        }

        if (AI == aiTypes.RAND)
        {
            int targetIndex = Random.Range(0, availablePlayers.Count);
            target = availablePlayers[targetIndex];
        }

        yield return null;
    }

    private Action RandomAttack()
    {
        if (enemyActions.Count == 0)
        {
            Debug.LogError(
                entityName +
                " has no attacks configured. Assign at least Action #1 on its battle prefab."
            );

            return null;
        }

        int actionIndex = Random.Range(0, enemyActions.Count);
        return enemyActions.ElementAt(actionIndex).Value;
    }

    public Action chosenAttack()
    {
        if (AI == aiTypes.RAND)
        {
            return RandomAttack();
        }

        Debug.LogWarning("Boss AI is not implemented yet for " + entityName + ".");
        return null;
    }

    public override IEnumerator Turn()
    {
        yield return BattleController.Instance.EnemyTurn(this);
    }
}