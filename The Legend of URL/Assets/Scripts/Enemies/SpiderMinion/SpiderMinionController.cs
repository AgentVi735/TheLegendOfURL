public class SpiderMinionController : EnemyController
{
    private SpiderQueenController queenController;
    
    public void Initialise(EnemyData receivedData, SpiderQueenController controller, string givenID)
    {
        queenController = controller;
        base.Initialise(receivedData, null, givenID);
        ChangeState(moveState);
    }
    
    protected override void InitialiseStates()
    {
        moveState = new SpiderMinionMoveState();
        moveState.Initialise(this);
        attackState = new BasicAttackState();
        attackState.Initialise(this);
        stunState = new SpiderMinionStunState();
        stunState.Initialise(this);
    }
    
    protected override void KillEnemy()
    {
        player.OnEnemyKill();
        queenController.OnMinionKilled();
        Destroy(gameObject);
    }
}