using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework;
using TMPro;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class BattleController : MonoBehaviour
{
    public static BattleController Instance;
    public List <PlayerBattle> players = new List<PlayerBattle>();
    public List <EnemyBattle> enemies = new List<EnemyBattle>();
    public List <BattleEntity> battleEntities = new List <BattleEntity>();
    public List <BattleEntity> turnOrder = new List <BattleEntity>();

    private bool actionDecided = false;
    private bool attackDecided = false;
    private bool itemDecided = false;
    private BattleUIController.actionChoices actionChoice = BattleUIController.actionChoices.NONE;
    private string attackChoice = "";
    private string itemChoice = "";

    public bool waitingForChoice = false;

    public List <PlayerBattle> getPlayers()
    {
        return players;
    }

    public List <EnemyBattle> getEnemies()
    {
        return enemies;
    }
    List <BattleEntity> targets = new List<BattleEntity>();



    private void Awake()
    {
        Instance = this;
        GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject obj in playerList)
        {
            PlayerBattle player = obj.GetComponent<PlayerBattle>();
            players.Add(player);
            battleEntities.Add(player);
            //Debug.Log($"Added {player.entityName}");
        }
        foreach (GameObject obj in enemyList)
        {
            EnemyBattle enemy = obj.GetComponent<EnemyBattle>();
            enemies.Add(enemy);
            battleEntities.Add(enemy);
            //Debug.Log($"Added {enemy.entityName}");
        }
    }

    
    void findTurnOrder()
    {
        turnOrder = battleEntities.OrderByDescending(entity => entity.speed).ToList();
    }
    Boolean arePlayersWiped()
    {
        int size = players.Count();
        int playersDead = 0;
        List <PlayerBattle> playersToRemove = new List<PlayerBattle>();
        //Debug.Log(size);
        foreach(PlayerBattle player in players)
        {
            //Debug.Log(player.entityName);
            if (player.isEntityDead())
            {
                playersDead++;
                //turnOrder.Remove(player);
                //players.Remove(player);
                playersToRemove.Add(player);
            }
        }
        foreach (PlayerBattle player in playersToRemove)
        {
            players.Remove(player);
        }
        if (playersDead == size) {
            return true;
        }
        else
        {
            return false;
        } 
    }

    Boolean areEnemiesWiped()
    {
        int size = enemies.Count();
        int enemiesDead = 0;
        List <EnemyBattle> enemiesToRemove = new List<EnemyBattle>();
        foreach(EnemyBattle enemy in enemies)
        {
            if (enemy.isEntityDead())
            {
                enemiesDead++;
                //turnOrder.Remove(enemy);
                //enemies.Remove(enemy);
                enemiesToRemove.Add(enemy);
            }
        }
        foreach (EnemyBattle enemy in enemiesToRemove)
        {
            enemies.Remove(enemy);
        }
        if (enemiesDead == size) {
            return true;
        }
        else
        {
            return false;
        } 
    }


    void endBattle()
    {
        if (players.Count() == 0)
        {
            Debug.Log("YOU LOSE");
        } 
        else if (enemies.Count() == 0)
        {
            Debug.Log("YOU WIN!");
            LevelLoader.unloadBattle();
        }
    }

    /*
    IEnumerator PlayerAttack(PlayerBattle player, Action action)
    {
        targets.Clear();
        if (action.getTargeting() == "SINGLE")
        {
            //waitingForChoice = true;
            yield return StartCoroutine(BattleUIController.Instance.targeting(enemies));
            EnemyBattle target = BattleUIController.Instance.returnTarget();
            targets.Add(target);
        }
        else if (action.getTargeting() == "SPREAD")
        {
            targets = player.getAllTargets();
        }
        action.doAction(targets);
    }
    */

    IEnumerator AttackLoop(PlayerBattle player)
    //public void AttackLoop(PlayerBattle player)
    {
        Debug.Log ("PICK ATTACK");
        yield return StartCoroutine(BattleUIController.Instance.playerAttacks(player));
        //yield return new WaitUntil(() => attackDecided);

        if (attackChoice == "BACK")
        {
            
            //Instance.StartCoroutine(Instance.MenuLoop(player));
            yield return StartCoroutine(MenuLoop(player));
        }
        else
        {
            Action action = BattleUIController.Instance.getAttack();
            //yield return StartCoroutine(PlayerAttack(player, action));
            yield return StartCoroutine(player.Attack(action));
        }


    }

    IEnumerator ItemLoop(PlayerBattle player)
    {
        Debug.Log ("PICK ITEM");
        //yield return StartCoroutine(BattleUIController.Instance.playerItems(player));
        //yield return new WaitUntil(() => itemDecided);

        if (itemChoice == "BACK")
        {
            
            //Instance.StartCoroutine(MenuLoop(player));
            yield return StartCoroutine(MenuLoop(player));
        }
        else
        {
            //yield return StartCoroutine();
            //player.useItem(itemChoice);
        }
    }

    IEnumerator MenuLoop(PlayerBattle player)
    //public void MenuLoop(PlayerBattle player)
    {
        Debug.Log ("ATTACK OR ITEM?");
        Debug.Log("Waiting For Selection");
        yield return StartCoroutine(BattleUIController.Instance.playerMenu());
        BattleUIController.actionChoices actionChoice = BattleUIController.Instance.getAction();


        //yield return new WaitUntil(() => actionChoice != )

        if (actionChoice == BattleUIController.actionChoices.ATTACK)
        {
            //Instance.StartCoroutine(Instance.AttackLoop(player));
            yield return StartCoroutine(AttackLoop(player));
            //AttackLoop(player);
        }
        else if (actionChoice == BattleUIController.actionChoices.ITEM)
        {
            yield return StartCoroutine(ItemLoop(player));

        }
    }

    //public static void PlayerTurn(PlayerBattle player)
    public IEnumerator PlayerTurn (PlayerBattle player)
    {
        yield return StartCoroutine(MenuLoop(player));
        //Instance.MenuLoop(player);
    }

    public IEnumerator EnemyTurn (EnemyBattle enemy)
    {
        Action action = enemy.chosenAttack();
        //Debug.Log($"{enemy.entityName} used {action.getActionName()}!");
        yield return StartCoroutine(enemy.Attack(action));
    }

    //void Turn(BattleEntity currentBattleEntity)
    IEnumerator Turn (BattleEntity currentBattleEntity)
    {
        Debug.Log($"{currentBattleEntity.entityName}'s Turn!");
        yield return StartCoroutine(currentBattleEntity.Turn());
    }
    
    IEnumerator Battle()
    //void battle()
    {
        Debug.Log("BATTLE START!");
        Boolean enemiesORPlayersDead = false;
        while (!enemiesORPlayersDead)
        {
            foreach (BattleEntity entity in turnOrder) {
                if (arePlayersWiped() || areEnemiesWiped())
                //if (isSideWiped(players) || isSideWiped(enemies))
                {
                    enemiesORPlayersDead = true;
                    break;
                } else if (entity.isDead){
                    continue;
                }
                //yield return turn(entity);
                yield return StartCoroutine(Turn(entity));
                //turn(entity);
            }
        }
        endBattle();
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        findTurnOrder();
        StartCoroutine(Battle());
        //battle();
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
