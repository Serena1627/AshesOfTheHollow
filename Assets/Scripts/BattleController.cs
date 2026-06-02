using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private BattleUIController.actionChoices actionChoice = BattleUIController.actionChoices.NONE;
    private string attackChoice = "";
    private Item itemChoice = null;

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

    public List<Item> items = new List<Item>();



    private void Awake()
    {
        Instance = this;
    }

    private void FindBattleParticipants()
    {
        players.Clear();
        enemies.Clear();
        battleEntities.Clear();
        turnOrder.Clear();

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject obj in playerObjects)
        {
            PlayerBattle player = obj.GetComponent<PlayerBattle>();

            if (player == null)
            {
                Debug.LogWarning(
                    obj.name + " has the Player tag but does not have PlayerBattle."
                );
                continue;
            }

            players.Add(player);
            battleEntities.Add(player);

            Debug.Log("Registered player battle entity: " + player.entityName);
        }

        foreach (GameObject obj in enemyObjects)
        {
            EnemyBattle enemy = obj.GetComponent<EnemyBattle>();

            if (enemy == null)
            {
                Debug.LogWarning(
                    obj.name + " has the Enemy tag but does not have EnemyBattle."
                );
                continue;
            }

            enemies.Add(enemy);
            battleEntities.Add(enemy);

            Debug.Log("Registered enemy battle entity: " + enemy.entityName);
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

    public List<Item> getItems()
    {
        return items;
    }

    void setItems()
    {
        HealItem item = new HealItem();
        item.Init("TESTPOTION", Item.itemTypes.SINGLE.ToString(), 2);
        items.Add(item);
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
            StartCoroutine(LevelLoader.unloadBattle());
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

    public IEnumerator AttackLoop(PlayerBattle player)
    //public void AttackLoop(PlayerBattle player)
    {
        Debug.Log ("PICK ATTACK");
        yield return StartCoroutine(BattleUIController.Instance.playerAttacks(player));
        Action action = BattleUIController.Instance.getAttack();
        if (action == null)
        {
            yield return StartCoroutine(MenuLoop(player));
        }
        else
        {
            yield return StartCoroutine(player.Attack(action));
            if (player.getTarget() == null)
            {
                yield return StartCoroutine(AttackLoop(player));
            }
        }
    }

    public List<BattleEntity> getItemTargets(Item item, PlayerBattle player)
    {
        List<BattleEntity> itemTargets = new List<BattleEntity>();
        if (item.getType() == Item.itemTypes.SINGLE.ToString())
        {
            itemTargets.Add(player);
        } else if (item.getType() == Item.itemTypes.PARTY.ToString())
        {
            foreach (PlayerBattle entity in players)
            {
                itemTargets.Add(entity);
            }
        }
        return itemTargets;
    }

    IEnumerator ItemLoop(PlayerBattle player)
    {
        Debug.Log ("PICK ITEM");
        yield return StartCoroutine(BattleUIController.Instance.playerItems());
        itemChoice = BattleUIController.Instance.getItem();
        //yield return new WaitUntil(() => itemDecided);

        if (itemChoice == null)
        {
            
            //Instance.StartCoroutine(MenuLoop(player));
            yield return StartCoroutine(MenuLoop(player));
        }
        else
        {
            targets = getItemTargets(itemChoice, player);
            Debug.Log($"{player.entityName} used {itemChoice.getName()}!");
            itemChoice.useItem(targets);
            items.Remove(itemChoice);
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

    public IEnumerator EnemyTurn(EnemyBattle enemy)
    {
        if (enemy == null)
        {
            Debug.LogWarning("Enemy turn was called for a missing enemy.");
            yield break;
        }

        Action action = enemy.chosenAttack();

        if (action == null)
        {
            Debug.LogWarning(enemy.entityName + " has no valid attack and skips its turn.");
            yield break;
        }

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
    private void Start()
    {
        FindBattleParticipants();

        if (players.Count == 0)
        {
            Debug.LogError(
                "Battle cannot begin because no PlayerBattle objects were found. " +
                "Check BattleSceneManager spawning and Player prefab tags."
            );
            return;
        }

        if (enemies.Count == 0)
        {
            Debug.LogError(
                "Battle cannot begin because no EnemyBattle objects were found. " +
                "Check BattleEncounterTrigger enemy prefabs or test enemy prefabs."
            );
            return;
        }

        findTurnOrder();
        setItems();

        StartCoroutine(Battle());
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
