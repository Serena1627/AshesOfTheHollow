using System.Collections.Generic;
using UnityEngine;

public class Action
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

    private int damage;
    private string actionName;
    private string actionType;
    private string targeting;

    public void Init(
        int _damage,
        string _name,
        string _targeting,
        string _type
    )
    {
        damage = _damage;
        actionName = _name;
        targeting = _targeting;
        actionType = _type;
    }

    public string getTargeting()
    {
        return targeting;
    }

    public string getActionName()
    {
        return actionName;
    }

    public int getDamage()
    {
        return damage;
    }

    public string getActionType()
    {
        return actionType;
    }

    public void doAction(List<BattleEntity> targets)
    {
        if (targets == null)
        {
            return;
        }

        foreach (BattleEntity target in targets)
        {
            if (target == null)
            {
                continue;
            }

            target.takeDamage(damage, actionType);
        }
    }

    public void doAction(BattleEntity target)
    {
        if (target == null)
        {
            return;
        }

        target.takeDamage(damage, actionType);
    }
}