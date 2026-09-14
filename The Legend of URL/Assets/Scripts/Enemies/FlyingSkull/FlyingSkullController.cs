public class FlyingSkullController : EnemyController
{
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath)
    {
        base.Initialise(receivedData, receivedPath);
        idleState = new FlyingPatrolState();
        moveState = new BasicMoveState();
        ChangeState(idleState);
    }
}