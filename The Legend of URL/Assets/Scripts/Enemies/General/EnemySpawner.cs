using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyData enemyToSpawn;
    private EnemyController enemy;

    private void Awake()
    {
        SpawnEnemy();
    }

    private void SpawnEnemy()
    {
        enemy = Instantiate(enemyToSpawn.prefab, transform.position, transform.rotation, transform);
        enemy.Initialise(enemyToSpawn);
    }
}