public class SpiderController : EnemyController
{
    public override void Initialise(EnemyData receivedData, EnemyWaypoint[] receivedPath, string givenID)
    {
        base.Initialise(receivedData, receivedPath, givenID);
        ChangeState(idleState);
    }
}