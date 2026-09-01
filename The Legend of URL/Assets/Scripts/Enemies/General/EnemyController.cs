using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour
{
    public CharacterController characterController => _characterController;
    [SerializeField] private CharacterController _characterController;
    public NavMeshAgent navMeshAgent => _navMeshAgent;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    public Transform eyesTransform => _eyesTransform;
    [SerializeField] private Transform _eyesTransform;
    public PlayerController player { get; private set; }
    
    public short health { get; protected set; }
    protected EnemyControllerType type;
    public EnemyData data { get; private set; }
    public EnemyWaypoint[] patrolPath { get; private set; }

    private IEnemyState currentState;

    public IEnemyState idleState;
    public IEnemyState moveState;
    public IEnemyState lookState;
    public IEnemyState attackState;
    
    public virtual void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        data = receivedData;
        health = data.health;
        gameObject.name = data.enemyName;
        type = data.controllerType;
        patrolPath = receivedPath;
        player = FindAnyObjectByType<PlayerController>();
    }

    public virtual void ChangeState(IEnemyState newState)
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