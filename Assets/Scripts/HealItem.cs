using System.Collections.Generic;
using UnityEngine;

public class HealItem : Item
{
    private int healAmount;

    public void Init(string _name, string _itemType, int _healAmount)
    {
        base.Init(_name, _itemType);
        healAmount = _healAmount;
    }

    public int GetHealAmount()
    {
        return healAmount;
    }

    public override void useItem(List<BattleEntity> targets)
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

            Debug.Log(target.entityName + " healed " + healAmount + " HP!");
            target.heal(healAmount);
        }
    }
}