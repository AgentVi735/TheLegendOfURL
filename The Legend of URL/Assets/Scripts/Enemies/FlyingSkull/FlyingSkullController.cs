using UnityEngine;

public class FlyingSkullController : EnemyController
{
    public Transform targetTrans;
    
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        targetTrans = new GameObject().transform;
        targetTrans.SetParent(transform.parent);
        targetTrans.position = Vector3.zero;
        base.Initialise(receivedData, receivedPath);
        ChangeState(idleState);
    }

    protected override void InitialiseStates()
    {
        idleState = new FlyingPatrolState();
        idleState.Initialise(this);
        moveState = new FlyingFollowState();
        moveState.Initialise(this);
        lookState = new LookForPlayerState();
        lookState.Initialise(this);
        attackState = new FlyingAttackState();
        attackState.Initialise(this);
        knockbackState = new KnockbackState();
        knockbackState.Initialise(this);
        stunState = new StunState();
        stunState.Initialise(this);
    }
}