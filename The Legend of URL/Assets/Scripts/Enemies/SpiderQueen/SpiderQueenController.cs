using System.Collections;
using System.Linq;
using UnityEngine;

public class SpiderQueenController : EnemyController
{
    public Transform MovementTrans => movementTrans;
    [SerializeField] private Transform movementTrans;

    private SpiderMinionSpawner[] minionSpawners;
    
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
    
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath, string givenID)
    {
        base.Initialise(receivedData, receivedPath, givenID);
        maxHealth = data.health;
        phase = SpiderQueenPhase.Main;
        ChangeState(moveState);
        minionSpawners = FindObjectsByType<SpiderMinionSpawner>();
        Debug.Log(minionSpawners.Length);
        waitMinionSpawnCooldown = new WaitForSeconds(data.minionSpawnCooldown);
        isSpawnCooldownFinished = true;
        areAllMinionsDead = true;
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
                {
                    if (!isAllowedToScream) // TODO: TEMPORARY
                        Scream();
                    else
                        WalkToCeiling();
                }
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
        
        switch (isCharging)
        {
            case false when !isStunned:
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
}