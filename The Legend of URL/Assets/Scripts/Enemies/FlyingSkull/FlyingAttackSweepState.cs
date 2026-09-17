using UnityEngine;

public class FlyingAttackSweepState : IEnemyState
{
    private FlyingSkullController _controller;

    private Transform enemyTrans;
    private Transform playerTrans;
    private Transform targetTrans;

    private float timeSpent;
    private float totalTime;
    private float distance;

    private Quaternion enemyStartRotation;
    private Quaternion enemyEndRotation;
    private Quaternion targetStartRotation;
    private Quaternion targetEndRotation;

    private Vector3 playerPos;
    private Vector3 endPos;
    
    public void Initialise(EnemyController controller)
    {
        _controller = (FlyingSkullController) controller;
        enemyTrans = _controller.transform;
        playerTrans = _controller.player.transform;
        targetTrans = _controller.targetTrans;
        totalTime = _controller.data.attackRadius;
    }

    public void UpdateState()
    {
        float ratio = timeSpent / totalTime;
        Quaternion targetRot = Quaternion.Slerp(targetStartRotation, targetEndRotation, ratio);
        Quaternion enemyRot = Quaternion.Slerp(enemyStartRotation, enemyEndRotation, ratio);

        targetTrans.rotation = targetRot;
        enemyTrans.position = targetTrans.position + targetTrans.forward * distance;

        enemyTrans.rotation = enemyRot;
        
        timeSpent += Time.deltaTime;

        if (timeSpent >= totalTime)
            _controller.Stun(_controller.data.attackCooldown);
    }

    public void OnEnter()
    {
        timeSpent = 0;
        _controller.meshRenderer.material.color = Color.red;
        Vector3 targetPos = playerTrans.position;
        targetPos.y = enemyTrans.position.y;
        targetTrans.position = targetPos;
        enemyTrans.LookAt(targetTrans.position);
        targetTrans.rotation = enemyTrans.rotation;
        enemyStartRotation = Quaternion.Euler(89, enemyTrans.rotation.eulerAngles.y, 0);
        enemyEndRotation = Quaternion.Euler(-90, enemyTrans.rotation.eulerAngles.y, 0);
        targetStartRotation = Quaternion.Euler(180, enemyTrans.rotation.eulerAngles.y, 0);
        targetEndRotation = Quaternion.Euler(360, enemyTrans.rotation.eulerAngles.y, 0);
        distance = Vector3.Distance(targetTrans.position, enemyTrans.position);
        playerPos = playerTrans.position;
        playerPos.y += _controller.player._characterController.height / 2;
        endPos = enemyTrans.forward * (distance * 2) + enemyTrans.position;
    }

    public void OnExit()
    {
        enemyTrans.rotation = Quaternion.Euler(0, enemyTrans.rotation.eulerAngles.y, 0);
    }

    public void OnHurt()
    {
    }

    public void OnDrawGizmosSelected()
    {
    }
}