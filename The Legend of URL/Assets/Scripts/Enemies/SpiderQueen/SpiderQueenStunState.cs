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
        if (_controller.meshRenderer != null)
            _controller.meshRenderer.material.color = Color.red;
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