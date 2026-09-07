using UnityEngine;

public class BasicAttackState : IEnemyState
{
    public void UpdateState(EnemyController controller)
    {
        
    }

    public void OnEnter(EnemyController controller)
    {
        controller.meshRenderer.material.color = Color.darkCyan;
        controller.player.OnHit(controller.data.damage);
        controller.Stun(controller.data.attackCooldown);
    }

    public void OnExit(EnemyController controller)
    {
    }

    public void OnHurt(EnemyController controller)
    {
    }

    public void OnDrawGizmosSelected(EnemyController controller)
    {
    }
}