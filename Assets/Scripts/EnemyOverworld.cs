using UnityEngine;

public class EnemyOverworld : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] public string enemyType = "template";


    public void startFight()
    {
        //LevelLoader.loadBattle(enemyType);
        StartCoroutine(LevelLoader.LoadBattle(this));
    }

    public string getType()
    {
        return enemyType;
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
