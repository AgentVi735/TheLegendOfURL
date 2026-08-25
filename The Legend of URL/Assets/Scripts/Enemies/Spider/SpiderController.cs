using UnityEngine;

public class SpiderController : EnemyController
{
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        base.Initialise(receivedData, receivedPath);
        moveState = new SpiderMoveState();
        idleState = new SpiderPatrolState();
        ChangeState(idleState);
    }
}