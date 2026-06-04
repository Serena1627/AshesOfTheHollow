using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBattle : BattleEntity
{
    private readonly Dictionary<string, Action> playerActions =
        new Dictionary<string, Action>();

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

    private BattleEntity target;

    protected override void Awake()
    {
        base.Awake();
        addActions();
    }

    public override void addActions()
    {
        playerActions.Clear();

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

        Debug.Log(
            entityName +
            " initialized " +
            playerActions.Count +
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

        playerActions.Add(label, action);
    }

    public Dictionary<string, Action> getActionList()
    {
        return playerActions;
    }

    public override BattleEntity getTarget()
    {
        return target;
    }

    public override IEnumerator pickTarget()
    {
        target = null;

        if (BattleUIController.Instance == null ||
            BattleController.Instance == null)
        {
            Debug.LogError(
                entityName +
                " cannot choose a target because battle UI/controller is missing."
            );

            yield break;
        }

        yield return StartCoroutine(
            BattleUIController.Instance.Targeting(
                BattleController.Instance.getEnemies()
            )
        );

        target = BattleUIController.Instance.ReturnTarget();
    }

    public override List<BattleEntity> getAllTargets()
    {
        List<BattleEntity> validTargets = new List<BattleEntity>();

        if (BattleController.Instance == null)
        {
            return validTargets;
        }

        foreach (EnemyBattle enemy in BattleController.Instance.getEnemies())
        {
            if (enemy != null && !enemy.isEntityDead())
            {
                validTargets.Add(enemy);
            }
        }

        return validTargets;
    }

    public override IEnumerator Turn()
    {
        Debug.Log(entityName + " PlayerBattle.Turn() reached.");

        if (BattleController.Instance == null)
        {
            Debug.LogError("BattleController.Instance is missing.");
            yield break;
        }

        yield return StartCoroutine(
            BattleController.Instance.PlayerTurn(this)
        );
    }
}