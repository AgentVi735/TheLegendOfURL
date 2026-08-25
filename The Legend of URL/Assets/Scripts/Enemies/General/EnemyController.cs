using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour
{
    public CharacterController characterController => _characterController;
    [SerializeField] private CharacterController _characterController;
    public NavMeshAgent navMeshAgent => _navMeshAgent;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    public PlayerController player { get; private set; }
    
    public short health { get; protected set; }
    public short damage { get; protected set; }
    public float walkSpeed { get; private set; }
    public float turnSpeed { get; private set; }
    protected EnemyControllerType type;
    private EnemyData data;
    public EnemyWaypoint[] patrolPath { get; private set; }

    private IEnemyState currentState;

    protected IEnemyState idleState;
    protected IEnemyState moveState;
    protected IEnemyState attackState;
    
    public virtual void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        data = receivedData;
        health = data.health;
        damage = data.damage;
        walkSpeed = data.walkSpeed;
        turnSpeed = data.turnSpeed;
        gameObject.name = data.enemyName;
        type = data.controllerType;
        patrolPath = receivedPath;
        player = FindAnyObjectByType<PlayerController>();
    }

    protected void ChangeState(IEnemyState newState)
    {
        currentState?.OnExit(this);
        currentState = newState;
        currentState.OnEnter(this);
    }

    protected void Update()
    { 
        currentState?.UpdateState(this);
    }
}