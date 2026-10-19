using UnityEngine;

public class SpiderQueenStunState : IEnemyState
{
    private SpiderQueenController _controller;
    
    private float timeSpent;
    private float stunTime;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (SpiderQueenController) controller;
    }

    public void UpdateState()
    {
        timeSpent += Time.deltaTime;

        if (timeSpent >= stunTime)
            _controller.GetNewState();
    }

    public void OnEnter()
    {
        _controller.Animator.SetBool("isStunned", true);
        timeSpent = 0;
        stunTime = _controller.stunTime;
    }

    public void OnExit()
    {
        _controller.Animator.SetBool("isStunned", false);
        _controller.isStunned = false;
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}