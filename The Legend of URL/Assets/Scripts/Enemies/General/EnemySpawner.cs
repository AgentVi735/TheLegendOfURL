using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyToSpawn;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private EnemyWaypoint[] patrolPath;
    private EnemyController enemy;

    private void Awake()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        List<EnemyWaypoint> path = new();
        foreach (EnemyWaypoint enemyWaypoint in patrolPath)
        {
            path.Add(enemyWaypoint);
            foreach (EnemyWaypoint availableWaypoint in enemyWaypoint.availableWaypoints)
            {
                if (!path.Contains(availableWaypoint))
                    path.Add(availableWaypoint);
            }
        }
        enemy = Instantiate(enemyToSpawn.prefab, transform.position + spawnOffset, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn, path.ToArray());
    }
}