using UnityEngine;

public class SpiderMinionSpawner : EnemySpawner
{
    private SpiderQueenController queenController;

    private void Start()
    {
        queenController = FindAnyObjectByType<SpiderQueenController>();
    }

    public override void TrySpawnOnStart()
    {
    }
    
    public override void SpawnEnemy()
    {
        Vector3 spawnPos = (spawnTrans != null ? spawnTrans.position : transform.position) + spawnOffset;
        enemy = Instantiate(enemyToSpawn.prefab, spawnPos, transform.rotation, transform);
        ((SpiderMinionController)enemy).Initialise(enemyToSpawn, queenController, EnemyID);
    }
}