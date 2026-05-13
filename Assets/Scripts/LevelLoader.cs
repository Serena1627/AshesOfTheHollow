using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    static int currentOverworldSceneIndex;
    
    public static Dictionary<string, string> enemyTypes = new Dictionary<string, string>
    {
        {"template", "testScene"}
    };

    public static void loadBattle(String enemyType)
    {
        String sceneName = enemyTypes[enemyType];
        currentOverworldSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneName);
    }

    public static void unloadBattle()
    {
        SceneManager.LoadScene(currentOverworldSceneIndex);
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
