using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyToSpawn;
    public string EnemyID => _enemyID;
    [SerializeField] private string _enemyID;
    [SerializeField] private Transform spawnTrans;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private EnemyWaypoint patrolPath;
    private EnemyController enemy;

    public void SpawnEnemy()
    {
        if (string.IsNullOrEmpty(_enemyID))
        {
            Debug.LogError($"Enemy Spawner \"{name}\" has no enemyID set");
            gameObject.SetActive(false);
            return;
        }
        
        List<EnemyWaypoint> path = new() { patrolPath };
        foreach (EnemyWaypoint availableWaypoint in patrolPath.availableWaypoints)
        {
            if (!path.Contains(availableWaypoint))
                path.Add(availableWaypoint);
        }

        Vector3 spawnPos = (spawnTrans != null ? spawnTrans.position : transform.position) + spawnOffset;
        enemy = Instantiate(enemyToSpawn.prefab, spawnPos, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn, path.ToArray(), EnemyID);
    }
}