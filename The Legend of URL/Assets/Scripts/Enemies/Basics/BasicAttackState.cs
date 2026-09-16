using UnityEngine;

public class BasicAttackState : IEnemyState
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
        _controller.meshRenderer.material.color = Color.darkCyan;
        _controller.player.OnHit(_controller.data.damage);
        _controller.Stun(_controller.data.attackCooldown);
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