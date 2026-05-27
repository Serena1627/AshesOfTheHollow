using UnityEngine;
using System.Collections.Generic;

public class HealItem : Item
{
    int healAmount;

    public void Init(string _name, string _itemType, int _healAmount)
    {
        base.Init(_name, _itemType);
        healAmount = _healAmount;
    }

    public override void useItem(List <BattleEntity> targets)
    {
        foreach (BattleEntity target in targets)
        {
            Debug.Log($"{target} healed {healAmount} HP!");
            target.heal(healAmount);
        }
    }
}
