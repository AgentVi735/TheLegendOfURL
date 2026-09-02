using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyToSpawn;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private EnemyWaypoint[] patrolPath;
    private EnemyController enemy;
    [SerializeField] private int areaToSpawnOn;

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

        // NavMesh.SamplePosition(transform.position + spawnOffset, out NavMeshHit hit, 3, areaToSpawnOn);
        // Vector3 spawnPos = hit.position;
        Vector3 spawnPos = transform.position + spawnOffset;
        enemy = Instantiate(enemyToSpawn.prefab, spawnPos, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn, path.ToArray());
    }
}