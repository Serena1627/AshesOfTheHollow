using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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
    private actionChoices actionChoice = actionChoices.NONE;
    private string attackChoice = "";
    private string itemChoice = "";
    public enum actionChoices
    {
        NONE,
        ATTACK,
        ITEM
    }

    public List <PlayerBattle> getPlayers()
    {
        return players;
    }

    public List <EnemyBattle> getEnemies()
    {
        return enemies;
    }



    private void Awake()
    {
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
        /*
        int order = 0;
        foreach (BattleEntity entity in turnOrder)
        {
            order++;
            Debug.Log($"Move {order}: {entity.entityName}");
        }
        */
    }

    Boolean arePlayersWiped()
    {
        int size = players.Count();
        int playersDead = 0;
        foreach(PlayerBattle player in players)
        {
            if (player.isDead)
            {
                playersDead++;
                turnOrder.Remove(player);
                players.Remove(player);
            }
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
        foreach(EnemyBattle enemy in enemies)
        {
            if (enemy.isDead)
            {
                enemiesDead++;
                turnOrder.Remove(enemy);
                enemies.Remove(enemy);
            }
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

    IEnumerator ActionLoop(PlayerBattle player)
    {
        Debug.Log ("PICK ATTACK");
        yield return new WaitUntil(() => attackDecided);

        if (attackChoice == "BACK")
        {
            
            Instance.StartCoroutine(Instance.MenuLoop(player));
        }
        else
        {
            Action action = player.getActionList()[attackChoice];
            List <BattleEntity> targets = player.getTargets(player, action);
            action.doAction(targets);
        }


    }

    IEnumerator MenuLoop(PlayerBattle player)
    {
        Debug.Log ("ATTACK OR ITEM?");
        Debug.Log("Waiting For Button");

        yield return new WaitUntil(() => actionDecided);

        if (actionChoice == actionChoices.ATTACK)
        {
            Instance.StartCoroutine(Instance.ActionLoop(player));
        }
    }

    public static void PlayerTurn(PlayerBattle player)
    {
        Instance.StartCoroutine(Instance.MenuLoop(player));
    }

    void turn(BattleEntity currentBattleEntity)
    {
        Debug.Log($"{currentBattleEntity.entityName}'s Turn!");
        currentBattleEntity.turn();
    }
    
    void battle()
    {
        Boolean enemiesORPlayersDead = false;
        while (!enemiesORPlayersDead)
        {
            foreach (BattleEntity entity in turnOrder) {
                if (arePlayersWiped() || areEnemiesWiped())
                {
                    enemiesORPlayersDead = true;
                    break;
                }
                turn(entity);
            }
        }
        endBattle();
    }
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        findTurnOrder();
        battle();
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
