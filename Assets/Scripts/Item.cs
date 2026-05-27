using System.Collections.Generic;
using UnityEngine;

public class Item
{
    string itemName;
    string itemType;

    public enum itemTypes
    {
        SINGLE,
        PARTY
    }
    public virtual void Init(string _name, string _itemType)
    {
        itemName = _name;
        itemType = _itemType;
    }

    public string getName()
    {
        return itemName;
    }

    public string getType()
    {
        return itemType;
    }





    public virtual void useItem(List <BattleEntity> targets)
    {
  
    }
}
