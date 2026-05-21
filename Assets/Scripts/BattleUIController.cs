using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class BattleUIController : MonoBehaviour
{
   [SerializeField] GameObject actionOptions;
   //[SerializeField] Button attackMenuButton;
   //[SerializeField] Button itemMenuButton;
    public enum actionChoices
    {
        NONE,
        ATTACK,
        ITEM
    }
    private actionChoices action = actionChoices.NONE;
    public static BattleUIController Instance;

    List<Button> attackOptions = new List<Button>();
    [SerializeField] private Button attackButton;
    [SerializeField] private Transform attackButtonContainer;
    [SerializeField] private GameObject attackOptionsMenu;
    [SerializeField] private Button targetButton;
    [SerializeField] private Transform targetButtonContainer;
    [SerializeField] private GameObject targetOptionsMenu;
    EnemyBattle target = new EnemyBattle();

    private Action chosenAttack;
   public void attackButtonAction()
    {
        action = actionChoices.ATTACK;
        //BattleController.attackLoop();
    }

    public void itemButtonAction()
    {
        action = actionChoices.ITEM;
    }

    public actionChoices getAction()
    {
        return action;
    }

    public void selectAction (Action attack)
    {
        chosenAttack = attack;
    }

    public void generateAttacks(PlayerBattle player)
    {
        foreach (Action attack in player.getActionList().Values) {
            Button attackActionButton = Instantiate(attackButton, attackButtonContainer);
            TMP_Text text = attackActionButton.GetComponentInChildren<TMP_Text>();
            text.text = attack.getActionName();
            Debug.Log(text.text);
            attackActionButton.onClick.AddListener(() => selectAction(attack));
            attackOptions.Add(attackActionButton);
        }
    }

    public void removeButtons(List<Button> buttonList)
    {
        foreach (Button button in buttonList)
        {
            DestroyImmediate(button.gameObject, true);
        }
        buttonList.Clear();
    }

    /*
    public void removeAttacks()
    {
        foreach (Button attackButton in attackOptions)
        {
            Destroy(attackButton.gameObject);
        }
        attackOptions.Clear();
    }
    */
    public Action getAttack()
    {
        return chosenAttack;
    }

    public IEnumerator playerAttacks(PlayerBattle player)
    {
        attackOptionsMenu.SetActive(true);
        chosenAttack = null;
        generateAttacks(player);
        yield return new WaitUntil(() => chosenAttack != null);
        removeButtons(attackOptions);
        attackOptionsMenu.SetActive(false);
    }

    public IEnumerator playerMenu()
    {
        action = actionChoices.NONE;
        actionOptions.SetActive(true);
        yield return new WaitUntil(() => action is not actionChoices.NONE);
        actionOptions.SetActive(false);
    }

    /*
    public void deleteTargets(List <Button> enemyTargets)
    {
        foreach (Button enemyTargetButton in enemyTargets) {
            Destroy(enemyTargetButton.gameObject);
        }
        enemyTargets.Clear();
    }
    */

    public EnemyBattle returnTarget()
    {
        return target;
    }

    public void selectTarget(EnemyBattle enemy)
    {
        target = enemy;
    }

    public IEnumerator targeting(List<EnemyBattle> enemies)
    {
        List <Button> enemyTargets = new List<Button>();
        target = null;
        targetOptionsMenu.SetActive(true);
        generateTargets(enemies, enemyTargets);
        yield return new WaitUntil(() => target != null);
        removeButtons(enemyTargets);
        targetOptionsMenu.SetActive(false);
    }

    public void generateTargets(List<EnemyBattle> enemies, List <Button> enemyTargets)
    {
        foreach (EnemyBattle enemy in enemies) {
            Button enemyButton = Instantiate(targetButton, targetButtonContainer);
            TMP_Text text = enemyButton.GetComponentInChildren<TMP_Text>();
            text.text = enemy.entityName;
            enemyButton.onClick.AddListener(() => selectTarget(enemy));
            enemyTargets.Add(enemyButton);
        }
    }
   
   void Awake()
    {
        Instance = this;
    }
   
   // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        actionOptions.SetActive(false);
        attackOptionsMenu.SetActive(false);
        targetOptionsMenu.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
