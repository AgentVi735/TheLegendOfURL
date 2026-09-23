using UnityEngine;
using UnityEngine.AI;

public class FlyingSkullController : EnemyController
{
    public Transform targetTrans;
    public NavMeshObstacle targetPrefab;

    public IEnemyState stalkState;
    
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath, string givenID)
    {
        NavMeshObstacle target = Instantiate(targetPrefab, transform.parent, true);
        target.radius = receivedData.stalkRange - 0.5f;
        targetTrans = target.transform;
        targetTrans.position = Vector3.zero;
        base.Initialise(receivedData, receivedPath, givenID);
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
        stalkState = new FlyingStalkState();
        stalkState.Initialise(this);
    }
}