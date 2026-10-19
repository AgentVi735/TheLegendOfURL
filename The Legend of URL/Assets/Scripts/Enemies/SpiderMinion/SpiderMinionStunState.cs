using UnityEngine;

public class SpiderMinionStunState : IEnemyState
{
    private EnemyController _controller;
    
    private float timeSpent;
    private float stunTime;
    
    public void Initialise(EnemyController controller)
    {
        _controller = controller;
    }

    public void UpdateState()
    {
        timeSpent += Time.deltaTime;

        if (timeSpent >= stunTime)
            _controller.ChangeState(_controller.moveState);
    }

    public void OnEnter()
    {
        timeSpent = 0;
        stunTime = _controller.stunTime;
    }

    public void OnExit()
    {
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}