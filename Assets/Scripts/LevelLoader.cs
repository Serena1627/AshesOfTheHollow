using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEditor.PackageManager;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] GameObject levelRootStorage;
    static GameObject levelRoot;
    static int currentOverworldSceneIndex;
    static string battleSceneName;
    static EnemyOverworld encounteredEnemy;
    
    
    
    public static Dictionary<string, string> enemyTypes = new Dictionary<string, string>
    {
        {"template", "testScene"}
    };

    public static void unloadEntities(GameObject[] objs)
    {
        foreach (GameObject obj in objs)
        {
            obj.SetActive(false);
        }
    }

    //public static void loadBattle(String enemyType)
    public static IEnumerator LoadBattle (EnemyOverworld enemy)
    {
        encounteredEnemy = enemy;
        battleSceneName = enemyTypes[enemy.getType()];
        currentOverworldSceneIndex = SceneManager.GetActiveScene().buildIndex;
        //levelRoot.transform.Find("Player").gameObject.SetActive(false);
        //GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
        //unloadEntities(playerList);
        //GameObject[] enemyList = GameObject.FindGameObjectsWithTag("Enemy");
        //unloadEntities(enemyList);
        levelRoot.SetActive(false);
        yield return SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        //levelRoot.SetActive(false);
        //SceneManager.LoadScene(sceneName);
    }

    //public static void unloadBattle()
    public static IEnumerator unloadBattle()
    {
        //SceneManager.LoadScene(currentOverworldSceneIndex);
        levelRoot.SetActive(true);
        encounteredEnemy.gameObject.SetActive(false);
        yield return SceneManager.UnloadSceneAsync(battleSceneName);
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        levelRoot = levelRootStorage;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
