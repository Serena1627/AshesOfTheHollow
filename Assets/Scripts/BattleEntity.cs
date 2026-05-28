using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class BattleEntity : MonoBehaviour
{

    [SerializeField] public int health;
    int maxHealth;
    [SerializeField] public int speed;
    [SerializeField] public string entityName;
    [SerializeField] public int physDef;
    [SerializeField] public int spDef;
    [SerializeField] public BattleController battleController;
    public Boolean isDead = false;
    //public List <Action> actions = new List<Action>();

    public Boolean isEntityDead()
    {
        return isDead;
    }

    Boolean isDefenderDead()
    {
        if (health <= 0)
        {
            isDead = true;
        }
        return isDead;
    }

    public virtual IEnumerator pickTarget()
    {
        yield return null;
    }

    public virtual BattleEntity getTarget()
    {
        return new BattleEntity();
    }

    public virtual List <BattleEntity> getAllTargets()
    {
        List <BattleEntity> targets = new List<BattleEntity>();
        return targets;
    }

    public IEnumerator Attack(Action action)
    {
        List <BattleEntity> targets = new List<BattleEntity>();
        if (action.getTargeting() == "SINGLE")
        {
            yield return StartCoroutine(pickTarget());
            BattleEntity target = getTarget();
            if (target == null)
            {
                yield break;
            }
            targets.Add(target);
        }
        else if (action.getTargeting() == "SPREAD")
        {
            targets = getAllTargets();
        }
        Debug.Log($"{entityName} used {action.getActionName()}!");
        action.doAction(targets);
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
        Debug.Log($"{entityName} took {damage - defenceNumberUsed} damage!");
        Debug.Log($"{entityName} Health: {health}");
        if (isDefenderDead()) {
            Debug.Log($"{entityName} died!");
        }

    }

    public void heal(int healAmount)
    {
        if (health + healAmount > maxHealth)
        {
            health = maxHealth;
        }
        else
        {
            health += healAmount;
        }
        
    }

    public virtual void addActions()
    {
 
    }

    //public virtual void turn()
    public virtual IEnumerator Turn()
    {
        yield return null;
    }
    

    void Start()
    {
        addActions();
        maxHealth = health;
        //isDead = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
