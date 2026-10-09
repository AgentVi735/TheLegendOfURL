public class SpiderQueenController : EnemyController
{
    private SpiderQueenPhase phase;
    public bool isCharging;
    public bool isStunned;

    private short maxHealth;
    private short damageDoneWhileStunned;

    public IEnemyState attackChargeState;
    private IEnemyState crawlToCeilingState;
    private IEnemyState walkOnCeilingState;
    private IEnemyState dropState;
    
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath, string givenID)
    {
        base.Initialise(receivedData, receivedPath, givenID);
        maxHealth = data.health;
        phase = SpiderQueenPhase.Main;
        ChangeState(moveState);
    }

    protected override void InitialiseStates()
    {
        moveState = new SpiderQueenMoveState();
        moveState.Initialise(this);
        attackChargeState = new SpiderQueenAttackChargeState();
        attackChargeState.Initialise(this);
        attackState = new SpiderQueenAttackState();
        attackState.Initialise(this);
        knockbackState = new KnockbackState();
        knockbackState.Initialise(this);
        stunState = new SpiderQueenStunState();
        stunState.Initialise(this);
    }

    public void GetNewState()
    {
        switch (phase)
        {
            default:
            case SpiderQueenPhase.None:
            case SpiderQueenPhase.Main:
                ChangeState(moveState);
                break;
            case SpiderQueenPhase.Drop:
            case SpiderQueenPhase.Final:
                break;
        }
    }

    protected override void GetDamage(short amount)
    {
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
        ChangeState(walkOnCeilingState);
    }
}