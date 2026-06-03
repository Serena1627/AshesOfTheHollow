using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] public int health;
    [SerializeField] public int speed;
    [SerializeField] public string entityName;
    [SerializeField] public int physDef;
    [SerializeField] public int spDef;

    [HideInInspector] public bool isDead;

    private int maxHealth;

    protected virtual void Awake()
    {
        maxHealth = health;
        isDead = false;
    }

    public bool isEntityDead()
    {
        return isDead;
    }

    public virtual IEnumerator pickTarget()
    {
        yield return null;
    }

    public virtual BattleEntity getTarget()
    {
        return null;
    }

    public virtual List<BattleEntity> getAllTargets()
    {
        return new List<BattleEntity>();
    }

    public virtual void addActions()
    {
    }

    public virtual IEnumerator Turn()
    {
        yield return null;
    }

    public IEnumerator Attack(Action action)
    {
        if (action == null)
        {
            Debug.LogWarning(
                entityName + " has no valid action and skips the turn."
            );

            yield break;
        }

        List<BattleEntity> targets = new List<BattleEntity>();

        if (action.getTargeting() == Action.targetingTypes.SINGLE.ToString())
        {
            yield return StartCoroutine(pickTarget());

            BattleEntity selectedTarget = getTarget();

            if (selectedTarget == null)
            {
                Debug.Log(
                    entityName +
                    "'s attack was cancelled because no target was chosen."
                );

                yield break;
            }

            targets.Add(selectedTarget);
        }
        else if (action.getTargeting() ==
                 Action.targetingTypes.SPREAD.ToString())
        {
            targets = getAllTargets();
        }

        if (targets.Count == 0)
        {
            Debug.Log(entityName + "'s attack had no valid targets.");
            yield break;
        }

        Debug.Log(entityName + " used " + action.getActionName() + "!");

        action.doAction(targets);
    }

    public void takeDamage(int damage, string attackType)
    {
        int defence = attackType == Action.actionTypes.PHYS.ToString()
            ? physDef
            : spDef;

        int finalDamage = Mathf.Max(0, damage - defence);

        health = Mathf.Max(0, health - finalDamage);

        Debug.Log(
            entityName +
            " took " +
            finalDamage +
            " damage. Health: " +
            health
        );

        if (health <= 0 && !isDead)
        {
            isDead = true;
            Debug.Log(entityName + " died!");

            SpriteRenderer renderer = GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    public void heal(int healAmount)
    {
        if (isDead)
        {
            return;
        }

        health = Mathf.Min(
            maxHealth,
            health + Mathf.Max(0, healAmount)
        );

        Debug.Log(entityName + " Health: " + health);
    }
}