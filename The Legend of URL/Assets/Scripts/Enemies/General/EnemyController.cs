using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private PlayerController player;
    
    protected short health;
    protected short damage;
    protected float walkSpeed;
    protected float turnSpeed;
    protected EnemyControllerType type;
    private EnemyData data;

    private IEnemyState currentState;
    
    protected IEnemyState moveState;
    protected IEnemyState attackState;
    
    public EnemyData GetData() => data;
    public CharacterController GetCharacterController() => characterController;
    public NavMeshAgent GetNavMeshAgent() => navMeshAgent;
    public PlayerController GetPlayer() => player;
    
    public virtual void Initialise(EnemyData receivedData)
    {
        data = receivedData;
        health = data.health;
        damage = data.damage;
        walkSpeed = data.walkSpeed;
        navMeshAgent.speed = walkSpeed;
        turnSpeed = data.turnSpeed;
        gameObject.name = data.enemyName;
        type = data.controllerType;
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