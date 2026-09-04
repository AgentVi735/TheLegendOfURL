using System;
using System.Collections;
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

    private short health { get; set; }
    protected EnemyControllerType type;
    public EnemyData data { get; private set; }
    public EnemyWaypoint[] patrolPath { get; private set; }

    private IEnemyState currentState;

    public IEnemyState idleState;
    public IEnemyState moveState;
    public IEnemyState lookState;
    public IEnemyState attackState;
    public IEnemyState knockbackState;
    public IEnemyState stunState;

    public Transform obj;

    private WaitForSeconds waitInvincibleTimeAfterHit;
    private bool canBeHit;
    public Vector3 hitVelocity;
    
    public virtual void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        data = receivedData;
        health = data.health;
        gameObject.name = data.enemyName;
        type = data.controllerType;
        patrolPath = receivedPath;
        player = FindAnyObjectByType<PlayerController>();
        navMeshAgent.autoTraverseOffMeshLink = false;
        if (gameObject.name == "Freddy Fazbear")
            obj = GameObject.Find("Cube").transform;
        waitInvincibleTimeAfterHit = new WaitForSeconds(data.invincibleTimeAfterHit);
        canBeHit = true;
        
        idleState = new BasicPatrolState();
        lookState = new LookForPlayerState();
        knockbackState = new KnockbackState();
        stunState = new StunState();
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

    protected void OnDrawGizmosSelected()
    {
        currentState?.OnDrawGizmosSelected(this);
    }

    protected void OnTriggerEnter(Collider trigger)
    {
        if (trigger.name != "Sword" || !canBeHit) return;
        GetDamage(player.EnemyGetDamage(), trigger.ClosestPoint(transform.position) - transform.position);
    }

    private void GetDamage(short amount, Vector3 swordPoint)
    {
        if (!canBeHit) return;
        
        health -= amount;
        if (ShouldDie())
        {
            KillEnemy();
            return;
        }

        StartCoroutine(HitCooldown());
        GetKnockback();
    }

    private IEnumerator HitCooldown()
    {
        canBeHit = false;
        yield return waitInvincibleTimeAfterHit;
        canBeHit = true;
    }

    private void GetKnockback()
    {
        hitVelocity = transform.TransformDirection(data.knockbackVelocityOffset);
        hitVelocity *= data.knockbackMultiplier;
        ChangeState(knockbackState);
    }

    private bool ShouldDie()
    {
        return health <= 0;
    }

    private void KillEnemy()
    {
        Destroy(gameObject);
    }
}