using System.Collections.Generic;
using Mono.Cecil.Cil;
using Unity.VectorGraphics;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] public string enemyType = "template";


    public void startFight()
    {
        LevelLoader.loadLevel(enemyType);
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
