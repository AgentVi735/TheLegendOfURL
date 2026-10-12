using UnityEngine;

public class SpiderQueenAttackState : IEnemyState
{
    private EnemyController _controller;
    private Transform playerTransform;
    private Transform enemyTransform;
    private float attackRange;
    
    public void Initialise(EnemyController controller)
    {
        _controller = controller;
        attackRange = _controller.data.attackRadius;
        playerTransform = _controller.player.transform;
        enemyTransform = _controller.characterController.transform;
    }

    public void UpdateState()
    {
    }

    public void OnEnter()
    {
        float distance = Vector3.Distance(enemyTransform.position, playerTransform.position);
        if (distance <= attackRange)
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