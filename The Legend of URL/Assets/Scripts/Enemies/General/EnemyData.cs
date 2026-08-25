using UnityEngine;

[CreateAssetMenu(menuName = "Enemy")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public EnemyControllerType controllerType;
    public EnemyController prefab;
    public short health;
    public short damage;
    public float walkSpeed;
    public float turnSpeed;
}