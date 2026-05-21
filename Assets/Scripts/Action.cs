using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Action
{
    int damage;
    string actionName;
    string actionType;
    string targeting;
    
    //The reason I'm not using a contructor is that if we add partcle effects
    //we may need to use Unity functions and stuff, that which the Unity API
    //is unavailable for
    public void Init(int _damage, string _name, string _targeting, string _type)
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

    public void doAction(List <BattleEntity> targets)
    {
        Debug.Log(targets.Count);
        foreach (BattleEntity target in targets)
        {
            target.takeDamage(damage, actionType);
        }
    }

    public string getActionName()
    {
        return actionName;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
