using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] protected EnemyData enemyToSpawn;
    public string EnemyID => _enemyID;
    [SerializeField] private string _enemyID;
    [SerializeField] protected Transform spawnTrans;
    [SerializeField] protected Vector3 spawnOffset;
    [SerializeField] private EnemyWaypoint patrolPath;
    protected EnemyController enemy;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        gameObject.name = $"EnemySpawner ({EnemyID})";
    }
#endif

    public virtual void TrySpawnOnStart()
    {
        SpawnEnemy();
    }

    public virtual void SpawnEnemy()
    {
        if (string.IsNullOrEmpty(_enemyID))
        {
            Debug.LogError($"Enemy Spawner \"{name}\" has no enemyID set");
            gameObject.SetActive(false);
            return;
        }

        List<EnemyWaypoint> path = new();
        if (patrolPath != null)
        {
            path.Add(patrolPath);
            foreach (EnemyWaypoint availableWaypoint in patrolPath.availableWaypoints)
            {
                if (!path.Contains(availableWaypoint))
                    path.Add(availableWaypoint);
            }
        }

        Vector3 spawnPos = (spawnTrans != null ? spawnTrans.position : transform.position) + spawnOffset;
        enemy = Instantiate(enemyToSpawn.prefab, spawnPos, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn, patrolPath != null ? path.ToArray() : null, EnemyID);
    }
}