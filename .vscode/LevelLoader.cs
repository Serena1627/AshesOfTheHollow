using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    
    public static Dictionary<string, string> enemyTypes = new Dictionary<string, string>
    {
        {"template", "testScene"}
    };

    public static void loadLevel(String enemyType)
    {
        String sceneName = enemyTypes[enemyType];
        SceneManager.LoadScene(sceneName);
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
