using UnityEngine;

public class StunState : IEnemyState
{
    private float timeSpent;
    private float stunTime;
    
    public void UpdateState(EnemyController controller)
    {
        timeSpent += Time.deltaTime;

        if (timeSpent >= stunTime)
            controller.ChangeState(controller.lookState);
    }

    public void OnEnter(EnemyController controller)
    {
        stunTime = controller.data.knockbackStunTime;
        timeSpent = 0;
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