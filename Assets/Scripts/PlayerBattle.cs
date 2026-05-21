using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class PlayerBattle : BattleEntity
{
    public enum actionTypes
    {
        PHYS,
        MAG
    }

    public enum targetingTypes
    {
        SINGLE,
        SPREAD
    }
 
    [Header("ACTIONS")]
    Dictionary<string, Action> playerActions = new Dictionary <string, Action>();

    [Header("Action #1")]
    [SerializeField] int action1Damage;
    [SerializeField] string action1Name;
    [SerializeField] actionTypes action1Type;
    [SerializeField] targetingTypes action1TargetingType;


    [Header("Action #2")]
    [SerializeField] int action2Damage;
    [SerializeField] string action2Name;
    [SerializeField] actionTypes action2Type;
    [SerializeField] targetingTypes action2TargetingType;
    
    int numberOfActions = 2;
    public BattleEntity singleTargetEnemy;
    public BattleEntity target = new BattleEntity();
    bool waitingForChoice = true;

    public void targetButton(EnemyBattle enemy)
    {
        waitingForChoice = false;
        target = enemy;
    }

    /*
    IEnumerator selectTarget()
    {
        waitingForChoice = true;
        battleController.generateTargets();
        yield return new WaitUntil(() => waitingForChoice == false);
        
    }
    
    public override BattleEntity pickTarget()
    {
        target = new BattleEntity();
        StartCoroutine(selectTarget());
        return target;
    }
    */

    public override List <BattleEntity> getAllTargets()
    {
        List <EnemyBattle> enemyTargets = battleController.getEnemies();
        List <BattleEntity> targets = new List<BattleEntity>();
        foreach (EnemyBattle enemy in enemyTargets)
        {
            targets.Add(enemy);
        }
        return targets;
    }

    
    public override void addActions()
    {
        string action1Label = "Action1";
        string action2Label = "Action2";
        Action action1 = new Action();
        action1.Init(action1Damage, action1Name, action1TargetingType.ToString(), action1Type.ToString());
        playerActions.Add(action1Label, action1);

        Action action2 = new Action();
        action2.Init(action2Damage, action2Name, action2TargetingType.ToString(), action2Type.ToString());
        playerActions.Add(action2Label, action2);
    }

    public Dictionary<string, Action> getActionList()
    {
        return playerActions;
    }


    public override IEnumerator Turn()
    {
        //BattleController.PlayerTurn(this);
        yield return StartCoroutine(BattleController.Instance.PlayerTurn(this));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
