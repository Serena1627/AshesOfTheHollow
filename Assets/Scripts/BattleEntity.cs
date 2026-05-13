using System;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{

    [SerializeField] public int health;
    [SerializeField] public int speed;
    [SerializeField] public string entityName;
    [SerializeField] public int physDef;
    [SerializeField] public int spDef;
    [SerializeField] public BattleController battleController;




    public Boolean isDead = false;
    //public List <Action> actions = new List<Action>();

    
    Boolean isDefenderDead()
    {
        if (health <= 0)
        {
            isDead = true;
        }
        return isDead;
    }

    public virtual BattleEntity pickTarget()
    {
        BattleEntity battleEntity = new BattleEntity();
        return battleEntity;
    }

    public virtual List <BattleEntity> getAllTargets()
    {
        List <BattleEntity> targets = new List<BattleEntity>();
        return targets;
    }

    public List <BattleEntity> getTargets(BattleEntity player, Action action)
    {
        List <BattleEntity> targets = new List<BattleEntity>();

        if (action.getTargeting() == "SINGLE")
        {
            BattleEntity target = player.pickTarget();
            targets.Add(target);
        }
        else if (action.getTargeting() == "SPREAD")
        {
            targets = player.getAllTargets();
        }
        return targets;
    }

    public void takeDamage(int damage, string attackType)
    {
        int defenceNumberUsed = 0;
        if (attackType == "PHYS")
        {
            defenceNumberUsed = physDef;
        }
        else if (attackType == "MAG")
        {
            defenceNumberUsed = spDef;
        }
        health -= (damage - defenceNumberUsed);
        if (isDefenderDead()) {
            Debug.Log($"{entityName} died!");
        }

    }

    public virtual void addActions()
    {
 
    }

    public virtual void turn()
    {
        
    }
    

    void Start()
    {
        addActions();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
