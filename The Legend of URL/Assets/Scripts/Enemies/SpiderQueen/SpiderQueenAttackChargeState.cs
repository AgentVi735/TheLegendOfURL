using UnityEngine;

public class SpiderQueenAttackChargeState : IEnemyState
{
    private SpiderQueenController _controller;
    
    private float timeSpent;
    private float chargeTime;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (SpiderQueenController) controller;
    }

    public void UpdateState()
    {
        timeSpent += Time.deltaTime;

        if (timeSpent >= chargeTime)
            _controller.ChangeState(_controller.attackState);
    }

    public void OnEnter()
    {
        _controller.isCharging = true;
        timeSpent = 0;
        chargeTime = _controller.data.attackChargeTime;
    }

    public void OnExit()
    {
        _controller.isCharging = false;
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}