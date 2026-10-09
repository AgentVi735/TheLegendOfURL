public class SpiderQueenAttackState : IEnemyState
{
    private EnemyController _controller;
    
    public void Initialise(EnemyController controller)
    {
        _controller = controller;
    }

    public void UpdateState()
    {
    }

    public void OnEnter()
    {
        _controller.player.OnHit(_controller.data.damage);
        _controller.ChangeState(_controller.moveState);
    }

    public void OnExit()
    {
    }

    public void OnHurt( )
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}