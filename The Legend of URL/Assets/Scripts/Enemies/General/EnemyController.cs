using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyController : MonoBehaviour
{
    public CharacterController characterController => _characterController;
    [SerializeField] private CharacterController _characterController;
    public MeshRenderer meshRenderer => _meshRenderer;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    public Transform eyesTransform => _eyesTransform;
    [SerializeField] private Transform _eyesTransform;
    public Transform rightTransform => _rightTransform;
    [SerializeField] private Transform _rightTransform;
    public Transform leftTransform => _leftTransform;
    [SerializeField] private Transform _leftTransform;
    public PlayerController player { get; private set; }

    private short health { get; set; }
    protected EnemyControllerType type;
    public EnemyData data { get; private set; }
    public EnemyWaypoint[] patrolPath { get; private set; }
    public NavMeshQueryFilter navMeshFilter { get; private set; }

    private IEnemyState currentState;

    public IEnemyState idleState;
    public IEnemyState moveState;
    public IEnemyState lookState;
    public IEnemyState attackState;
    protected IEnemyState knockbackState;
    protected IEnemyState stunState;

    private WaitForSeconds waitInvincibleTimeAfterHit;
    private bool canBeHit;
    public Vector3 hitVelocity;
    public float stunTime;
    public LayerMask raycastLayers;
    public LayerMask swordLayer;
    
    public virtual void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        data = receivedData;
        health = data.health;
        gameObject.name = data.enemyName;
        type = data.controllerType;
        patrolPath = receivedPath;
        player = FindAnyObjectByType<PlayerController>();
        navMeshFilter = new NavMeshQueryFilter
        {
            agentTypeID = _navMeshAgent.agentTypeID,
            areaMask = _navMeshAgent.areaMask
        };
        Destroy(_navMeshAgent);
        waitInvincibleTimeAfterHit = new WaitForSeconds(data.invincibleTimeAfterHit);
        canBeHit = true;
        
        InitialiseStates();
    }

    protected virtual void InitialiseStates()
    {
        idleState = new BasicPatrolState();
        idleState.Initialise(this);
        moveState = new BasicMoveState();
        moveState.Initialise(this);
        lookState = new LookForPlayerState();
        lookState.Initialise(this);
        attackState = new BasicAttackState();
        attackState.Initialise(this);
        knockbackState = new KnockbackState();
        knockbackState.Initialise(this);
        stunState = new StunState();
        stunState.Initialise(this);
    }

    public virtual void ChangeState(IEnemyState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    protected void Update()
    { 
        currentState?.UpdateState();
    }

    protected void OnDrawGizmosSelected()
    {
        if (data == null) return;
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, data.attackRadius);
        Gizmos.color = Color.mediumSlateBlue;
        Gizmos.DrawWireSphere(transform.position, data.detectDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, data.forceDetectDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, data.followRange);
        currentState?.OnDrawGizmosSelected();
    }

    protected void OnTriggerEnter(Collider trigger)
    {
        if (((1 << trigger?.gameObject.layer) & swordLayer.value) == 0 || !canBeHit) return;
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
        transform.parent.gameObject.SetActive(false);
        Destroy(gameObject);
    }
}