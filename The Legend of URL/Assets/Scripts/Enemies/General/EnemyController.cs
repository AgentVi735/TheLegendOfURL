using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour
{
    public CharacterController characterController => _characterController;
    [SerializeField] private CharacterController _characterController;
    public MeshRenderer meshRenderer => _meshRenderer;
    [SerializeField] private MeshRenderer _meshRenderer;
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
    private IEnemyState knockbackState;
    private IEnemyState stunState;

    private WaitForSeconds waitInvincibleTimeAfterHit;
    private bool canBeHit;
    public Vector3 hitVelocity;
    public float stunTime;
    public LayerMask raycastLayers;
    
    public virtual void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        data = receivedData;
        health = data.health;
        gameObject.name = data.enemyName;
        type = data.controllerType;
        patrolPath = receivedPath;
        player = FindAnyObjectByType<PlayerController>();
        navMeshAgent.autoTraverseOffMeshLink = false;
        waitInvincibleTimeAfterHit = new WaitForSeconds(data.invincibleTimeAfterHit);
        canBeHit = true;
        
        idleState = new BasicPatrolState();
        lookState = new LookForPlayerState();
        attackState = new BasicAttackState();
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
        if (data == null) return;
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, data.attackRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.forceDetectDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.followRange);
        currentState?.OnDrawGizmosSelected(this);
    }

    protected void OnTriggerEnter(Collider trigger)
    {
        if (trigger.name != "Sword" || !canBeHit) return;
        GetDamage(player.EnemyGetDamage());
    }

    private void GetDamage(short amount)
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

    public void Stun(float time)
    {
        stunTime = time;
        ChangeState(stunState);
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