using System.Collections.Generic;
using UnityEngine;

public class Item
{
    string itemName;
    string itemType;
    int statAlterAmount;

    public enum itemTypes
    {
        HEAL,
        BUFF
    }
    public void Init(string _name, string _type, int _statAlterAmount)
    {
        itemName = _name;
        itemType = _type;
        statAlterAmount = _statAlterAmount;
    }

    public void heal(BattleEntity entity)
    {
        
    }



    public void useItem(List <BattleEntity> targets)
    {
        if (itemType == itemTypes.HEAL.ToString())
        {
            foreach (BattleEntity target in targets)
            {
                target.heal(statAlterAmount);
            }
        } else if (itemType == itemTypes.HEAL.ToString())
        {
            
        }
    }
}
