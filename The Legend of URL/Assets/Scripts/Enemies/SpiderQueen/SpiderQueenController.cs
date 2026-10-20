using System.Collections;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class SpiderQueenController : EnemyController
{
    public Transform MovementTrans => movementTrans;
    [SerializeField] private Transform movementTrans;

    private SpiderMinionSpawner[] minionSpawners;
    public SpiderQueenWallPoint[] wallPoints { get; private set; }
    
    private SpiderQueenPhase phase;
    public bool isCharging;
    public bool isStunned;
    public bool isScreaming;
    private bool isAllowedToScream;
    private bool isSpawnCooldownFinished;
    private bool areAllMinionsDead;
    private int minionCount;

    private short maxHealth;
    private short damageDoneWhileStunned;
    private WaitForSeconds waitMinionSpawnCooldown;

    public IEnemyState attackChargeState;
    private IEnemyState crawlToCeilingState;
    private IEnemyState walkOnCeilingState;
    private IEnemyState screamState;
    private IEnemyState dropState;
    
    private float walkAnimTargetX = 0f;
    private float walkAnimTargetY = 0f;
    [SerializeField] private float walkAnimSmoothing = 0.03f;
    private float walkAnimCurrentX = 0f;
    private float walkAnimCurrentY = 0f;

    [SerializeField] private float blockAnimationTime;
    private WaitForSeconds waitBlockAnimationTime;
    
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath, string givenID)
    {
        wallPoints = FindObjectsByType<SpiderQueenWallPoint>();
        base.Initialise(receivedData, receivedPath, givenID);
        maxHealth = data.health;
        phase = SpiderQueenPhase.Main;
        ChangeState(moveState);
        minionSpawners = FindObjectsByType<SpiderMinionSpawner>();
        waitMinionSpawnCooldown = new WaitForSeconds(data.minionSpawnCooldown);
        waitBlockAnimationTime = new WaitForSeconds(blockAnimationTime);
        isSpawnCooldownFinished = true;
        areAllMinionsDead = true;
        SceneController.Instance.AudioManager.PlayBossMusic();
    }

    protected override void InitialiseStates()
    {
        moveState = new SpiderQueenMoveState();
        moveState.Initialise(this);
        attackChargeState = new SpiderQueenAttackChargeState();
        attackChargeState.Initialise(this);
        attackState = new SpiderQueenAttackState();
        attackState.Initialise(this);
        stunState = new SpiderQueenStunState();
        stunState.Initialise(this);
        crawlToCeilingState = new SpiderQueenMoveToCeilingState();
        crawlToCeilingState.Initialise(this);
        screamState = new SpiderQueenScreamState();
        screamState.Initialise(this);
    }

    public void GetNewState()
    {
        switch (phase)
        {
            default:
            case SpiderQueenPhase.None:
            case SpiderQueenPhase.Main:
                Debug.Log(currentState);
                if (currentState == stunState)
                    WalkToCeiling();
                else
                    ChangeState(moveState);
                break;
            case SpiderQueenPhase.Drop:
            case SpiderQueenPhase.Final:
                break;
        }
    }

    protected override void GetDamage(short amount)
    {
        if (isScreaming)
            return;
        
        Animator.SetTrigger("gotHit");

        Debug.Log($"{currentState} | {isCharging} | {isStunned}");
        switch (isCharging)
        {
            case false when !isStunned:
                StartCoroutine(BlockAnimation());
                return;
            case false when isStunned:
                damageDoneWhileStunned += amount;
                if (damageDoneWhileStunned > data.maxDamageDuringStun)
                {
                    WalkToCeiling();
                    return;
                }
                break;
            case true:
                Stun(data.interruptStunTime);
                break;
        }
        
        base.GetDamage(amount);
    }

    private IEnumerator BlockAnimation()
    {
        Animator.SetBool("block", true);

        yield return waitBlockAnimationTime;

        Animator.SetBool("block", false);
    }
    
    public override void Stun(float time)
    {
        isStunned = true;
        damageDoneWhileStunned = 0;
        base.Stun(time);
    }

    private void WalkToCeiling()
    {
        Debug.Log("cleing");
        ChangeState(crawlToCeilingState);
    }

    private void Scream()
    {
        isAllowedToScream = true;
        if (isScreaming || !areAllMinionsDead || !isSpawnCooldownFinished)
            return;
        ChangeState(screamState);
    }

    public void SpawnSpiders()
    {
        Debug.Log("Try spawning spiders");
        if (!isScreaming || !areAllMinionsDead || !isSpawnCooldownFinished || !isAllowedToScream)
            return;

        minionCount = Random.Range(data.minionSpawnAmount.x, data.minionSpawnAmount.y);
        System.Random random = new();
        minionSpawners = minionSpawners.OrderBy(_ => random.Next()).ToArray();
        for (int i = 0; i < minionCount; i++)
            minionSpawners[i].SpawnEnemy();
        Debug.Log($"Spawned {minionCount} spiders");
        areAllMinionsDead = false;
        StartCoroutine(ScreamCountdown());
    }
    
    public override void ChangeState(IEnemyState newState)
    {
        if (newState == null)
            return;
        Debug.Log(newState);
        base.ChangeState(newState);
    }

    private IEnumerator ScreamCountdown()
    {
        isSpawnCooldownFinished = false;
        yield return waitMinionSpawnCooldown;
        isSpawnCooldownFinished = true;
        Scream();
    }

    public void OnMinionKilled()
    {
        minionCount--;
        if (minionCount != 0) return;
        areAllMinionsDead = true;
        Scream();
    }

    public void WalkAnimation()
    {
        walkAnimTargetX = 0f;
        walkAnimTargetY = 0f;
        
        Vector3 movePosForward = movementTrans.forward;

        walkAnimTargetX = movePosForward.x;
        walkAnimTargetY = movePosForward.z;
        
        walkAnimCurrentX = Mathf.Lerp(walkAnimCurrentX, walkAnimTargetX, walkAnimSmoothing);
        walkAnimCurrentY = Mathf.Lerp(walkAnimCurrentY, walkAnimTargetY, walkAnimSmoothing);
        
        Animator.SetFloat("walkX", walkAnimCurrentX);
        Animator.SetFloat("walkY", walkAnimCurrentY);
    }

    public void StopWalkAnimation()
    {
        walkAnimTargetX = 0;
        walkAnimTargetY = 0;
        walkAnimCurrentX = 0;
        walkAnimCurrentY = 0;
        
        Animator.SetFloat("walkX", walkAnimCurrentX);
        Animator.SetFloat("walkY", walkAnimCurrentY);
    }

    public void FinishAttackAnimation()
    {
        ChangeState(moveState);
    }
}