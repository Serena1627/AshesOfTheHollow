using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using UnityEditor;


public class EnemyBattle : BattleEntity
{
    Dictionary<string, Action> enemyActions = new Dictionary <string, Action>();

    public enum aiTypes
    {
        RAND,
        BOSS
    }

    [SerializeField] aiTypes AI;

    [Header("Action #1")]
    [SerializeField] int action1Damage;
    [SerializeField] string action1Name;
    [SerializeField] Action.actionTypes action1Type;
    [SerializeField] Action.targetingTypes action1TargetingType;


    [Header("Action #2")]
    [SerializeField] int action2Damage;
    [SerializeField] string action2Name;
    [SerializeField] Action.actionTypes action2Type;
    [SerializeField] Action.targetingTypes action2TargetingType;

    [Header("Action #3")]
    [SerializeField] int action3Damage;
    [SerializeField] string action3Name;
    [SerializeField] Action.actionTypes action3Type;
    [SerializeField] Action.targetingTypes action3TargetingType;


    [Header("Action #4")]
    [SerializeField] int action4Damage;
    [SerializeField] string action4Name;
    [SerializeField] Action.actionTypes action4Type;
    [SerializeField] Action.targetingTypes action4TargetingType;

    BattleEntity target;

    public override void addActions()
    {
        string action1Label = "Action1";
        string action2Label = "Action2";
        string action3Label = "Action3";
        string action4Label = "Action4";

        Action action1 = new Action();
        action1.Init(action1Damage, action1Name, action1TargetingType.ToString(), action1Type.ToString());
        enemyActions.Add(action1Label, action1);

        Action action2 = new Action();
        action2.Init(action2Damage, action2Name, action2TargetingType.ToString(), action2Type.ToString());
        enemyActions.Add(action2Label, action2);

        Action action3 = new Action();
        action3.Init(action3Damage, action3Name, action3TargetingType.ToString(), action3Type.ToString());
        enemyActions.Add(action3Label, action3);

        Action action4 = new Action();
        action4.Init(action4Damage, action4Name, action4TargetingType.ToString(), action4Type.ToString());
        enemyActions.Add(action4Label, action4);
    }

    public override List <BattleEntity> getAllTargets()
    {
        List <PlayerBattle> enemyTargets = BattleController.Instance.getPlayers();
        List <BattleEntity> targets = new List<BattleEntity>();
        foreach (PlayerBattle enemy in enemyTargets)
        {
            targets.Add(enemy);
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
        List <PlayerBattle> players = BattleController.Instance.getPlayers();
        if (AI == aiTypes.RAND)
        {
            int playerTargetIndex = Random.Range(0,players.Count());
            target = players[playerTargetIndex];
        } else if (AI == aiTypes.BOSS)
        {
            //BOSS code here
        }
        yield return null;
    }

    Action randomAttack()
    {
        int actionIndex = Random.Range(0,4);
        return enemyActions.ElementAt(actionIndex).Value;
    }
    public Action chosenAttack()
    {
        Action attack = new Action();
        if (AI == aiTypes.RAND)
        {
            attack = randomAttack();
        }
        else if (AI == aiTypes.BOSS)
        {
            attack = null;
        }
        return attack;
    }
    
    public override IEnumerator Turn()
    {
        yield return BattleController.Instance.EnemyTurn(this);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
