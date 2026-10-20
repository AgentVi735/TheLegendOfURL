using UnityEngine;

public class SpiderQueenScreamState : IEnemyState
{
    private SpiderQueenController _controller;
    
    private float timeSpent;
    private float screamTime;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (SpiderQueenController) controller;
    }

    public void UpdateState()
    {
        timeSpent += Time.deltaTime;

        if (!(timeSpent >= screamTime)) return;
        _controller.SpawnSpiders();
        _controller.GetNewState();
    }

    public void OnEnter()
    {
        _controller.AudioPlayer.StopPersistentAudio();
        _controller.AudioPlayer.PlayClip(1);
        _controller.Animator.SetTrigger("scream");
        _controller.isScreaming = true;
        timeSpent = 0;
        screamTime = _controller.data.screamAnimationDuration;
    }

    public void OnExit()
    {
        _controller.isScreaming = false;
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}