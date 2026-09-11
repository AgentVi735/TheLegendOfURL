using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyToSpawn;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private EnemyWaypoint patrolPath;
    private EnemyController enemy;

    private void Awake() => SpawnEnemy();

    private void SpawnEnemy()
    {
        List<EnemyWaypoint> path = new() { patrolPath };
        foreach (EnemyWaypoint availableWaypoint in patrolPath.availableWaypoints)
        {
            if (!path.Contains(availableWaypoint))
                path.Add(availableWaypoint);
        }

        Vector3 spawnPos = transform.position + spawnOffset;
        enemy = Instantiate(enemyToSpawn.prefab, spawnPos, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn, path.ToArray());
    }
}