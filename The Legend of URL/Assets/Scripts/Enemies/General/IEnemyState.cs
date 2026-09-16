using UnityEngine;

public interface IEnemyState
{
    public void Initialise(EnemyController controller);
    
    public void UpdateState();

    public void OnEnter();

    public void OnExit();

    public void OnHurt();

    public void OnDrawGizmosSelected();
}