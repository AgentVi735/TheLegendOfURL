using UnityEngine;

public interface IEnemyState
{
    public void UpdateState(EnemyController controller);

    public void OnEnter(EnemyController controller);

    public void OnExit(EnemyController controller);

    public void OnHurt(EnemyController controller);

    public void OnDrawGizmosSelected(EnemyController controller);
}