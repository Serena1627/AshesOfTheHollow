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

    protected override void Awake()
    {
        base.Awake();
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

        Debug.Log(
            entityName +
            " initialized " +
            enemyActions.Count +
            " action(s)."
        );
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
        List<BattleEntity> validTargets = new List<BattleEntity>();

        if (BattleController.Instance == null)
        {
            return validTargets;
        }

        foreach (PlayerBattle player in BattleController.Instance.getPlayers())
        {
            if (player != null && !player.isEntityDead())
            {
                validTargets.Add(player);
            }
        }

        return validTargets;
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
            yield break;
        }

        List<PlayerBattle> availablePlayers = BattleController.Instance
            .getPlayers()
            .Where(player => player != null && !player.isEntityDead())
            .ToList();

        if (availablePlayers.Count == 0)
        {
            yield break;
        }

        target = availablePlayers[
            Random.Range(0, availablePlayers.Count)
        ];

        yield return null;
    }

    public Action chosenAttack()
    {
        if (AI != aiTypes.RAND)
        {
            Debug.LogWarning(
                "Boss AI is not implemented for " + entityName + "."
            );

            return null;
        }

        if (enemyActions.Count == 0)
        {
            Debug.LogError(entityName + " has no configured attacks.");
            return null;
        }

        return enemyActions.ElementAt(
            Random.Range(0, enemyActions.Count)
        ).Value;
    }

    public override IEnumerator Turn()
    {
        if (BattleController.Instance == null)
        {
            yield break;
        }

        yield return StartCoroutine(
            BattleController.Instance.EnemyTurn(this)
        );
    }
}